using System.ComponentModel.DataAnnotations;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Groups;
using Database.Repos.SystemAccessesRepo;
using Database.Repos.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;
using uLearn.Web.Core.Attributes;
using uLearn.Web.Core.Authorization;
using uLearn.Web.Core.Extensions;
using uLearn.Web.Core.Models;
using Vostok.Logging.Abstractions;
using Web.Api.Configuration;


namespace uLearn.Web.Core.Controllers;

[Authorize(Policy = UlearnAuthorizationBuilder.StudentsPolicyName)] 
public class AccountController : BaseUserController
{
	private readonly ICourseStorage courseStorage = CourseManager.CourseStorageInstance;

	private readonly ICourseRolesRepo courseRolesRepo;
	private readonly IGroupsRepo groupsRepo;
	private readonly IGroupMembersRepo groupMembersRepo;
	private readonly IGroupAccessesRepo groupAccessesRepo;
	private readonly ICertificatesRepo certificatesRepo;
	private readonly IVisitsRepo visitsRepo;
	private readonly INotificationsRepo notificationsRepo;
	private readonly ICoursesRepo coursesRepo;
	private readonly ISystemAccessesRepo systemAccessesRepo;
	private readonly ITempCoursesRepo tempCoursesRepo;
	private readonly ISlideCheckingsRepo slideCheckingsRepo;
	private readonly AuthenticationManager authenticationManager;
	private readonly UlearnDb db;

	private readonly string telegramSecret;
	private static readonly WebConfiguration configuration;

	private static readonly List<string> hijackCookies = new List<string>();
	private static ILog log => LogProvider.Get().ForContext(typeof(AccountController));

	static AccountController()
	{
		configuration = ApplicationConfiguration.Read<WebConfiguration>();
		hijackCookies.Add(configuration.Web.CookieName);
	}

	public AccountController(
		UlearnDb db,
		IGroupsRepo groupsRepo,
		ICertificatesRepo certificatesRepo,
		IVisitsRepo visitsRepo,
		INotificationsRepo notificationsRepo,
		ICoursesRepo coursesRepo,
		ISystemAccessesRepo systemAccessesRepo,
		ITempCoursesRepo tempCoursesRepo,
		ISlideCheckingsRepo slideCheckingsRepo,
		UlearnUserManager userManager,
		ICourseRolesRepo courseRolesRepo,
		IUsersRepo usersRepo,
		IGroupMembersRepo groupMembersRepo,
		IGroupAccessesRepo groupAccessesRepo,
		WebConfiguration configuration,
		AuthenticationManager authenticationManager)
		: base(userManager, usersRepo, configuration)
	{
		this.db = db;
		this.groupsRepo = groupsRepo;
		this.courseRolesRepo = courseRolesRepo;
		this.certificatesRepo = certificatesRepo;
		this.visitsRepo = visitsRepo;
		this.notificationsRepo = notificationsRepo;
		this.coursesRepo = coursesRepo;
		this.systemAccessesRepo = systemAccessesRepo;
		this.tempCoursesRepo = tempCoursesRepo;
		this.slideCheckingsRepo = slideCheckingsRepo;
		this.authenticationManager = authenticationManager;
		this.groupMembersRepo = groupMembersRepo;
		this.groupAccessesRepo = groupAccessesRepo;

		telegramSecret = configuration.OldWebConfig["ulearn.telegram.webhook.secret"] ?? "";
	}

	[AllowAnonymous]
	public ActionResult Login(string returnUrl)
	{
		return RedirectToAction("Index", "Login", new { returnUrl });
	}

	[Authorize(Policy = UlearnAuthorizationBuilder.InstructorsPolicyName)] 
	public ActionResult List(UserSearchQueryModel queryModel)
	{
		return View(queryModel);
	}

	//[ChildActionOnly]
	public async Task<ActionResult> ListPartial(UserSearchQueryModel queryModel)
	{
		var userRolesByEmail = User.IsSystemAdministrator() ? await courseRolesRepo.FilterUsersByEmail(queryModel) : null;
		var userRoles = await courseRolesRepo.FilterUsers(queryModel);
		var model = await GetUserListModel(userRolesByEmail.EmptyIfNull().Concat(userRoles).Deprecated_DistinctBy(r => r.UserId).ToList());

		return PartialView("_UserListPartial", model);
	}

	private async Task<UserListModel> GetUserListModel(List<UserRolesInfo> users)
	{
		var coursesForUsers = await courseRolesRepo.GetCoursesForUsers();

		var courses = User.GetControllableCoursesId().ToList();

		var currentUserId = User.GetUserId();
		var userIds = users.Select(u => u.UserId).ToList();
		var allTempCourses = (await tempCoursesRepo.GetAllTempCourses()) // Все, потому что старые могут быть еще в памяти.
			.ToDictionary(t => t.CourseId, t => t, StringComparer.InvariantCultureIgnoreCase);
		var usersModels = new List<UserModel>();
		foreach (var user in users)
			usersModels.Add(await GetUserModel(user, coursesForUsers, courses, allTempCourses));

		var model = new UserListModel
		{
			CanToggleRoles = User.HasAccess(CourseRoleType.CourseAdmin),
			ShowDangerEntities = User.IsSystemAdministrator(),
			Users = usersModels,
			UsersGroups = await groupAccessesRepo.GetUsersGroupsNamesAsStrings(courses, userIds, currentUserId, actual: true, archived: false),
			UsersArchivedGroups = await groupAccessesRepo.GetUsersGroupsNamesAsStrings(courses, userIds, currentUserId, actual: false, archived: true),
			CanViewAndToggleCourseAccesses = false,
			CanViewAndToogleSystemAccesses = User.IsSystemAdministrator(),
			CanViewProfiles = await systemAccessesRepo.HasSystemAccessAsync(currentUserId, SystemAccessType.ViewAllProfiles) || User.IsSystemAdministrator(),
		};

		return model;
	}

	private async Task<UserModel> GetUserModel(UserRolesInfo userRoles, Dictionary<string, Dictionary<CourseRoleType, List<string>>> coursesForUsers,
		List<string> coursesIds, Dictionary<string, TempCourse> allTempCourses)
	{
		var user = new UserModel(userRoles)
		{
			CourseRoles = new Dictionary<string, ICoursesRolesListModel>
			{
				{
					LmsRoleType.SysAdmin.ToString(),
					new SingleCourseRolesModel
					{
						HasAccess = userRoles.Roles.Contains(LmsRoleType.SysAdmin.ToString()),
						ToggleUrl = Url.Content($"~/Account/{nameof(ToggleSystemRole)}?userId={userRoles.UserId}&role={LmsRoleType.SysAdmin}"), // Url.Action is slow: https://www.jitbit.com/alexblog/263-fastest-way-to-generate-urls-in-an-aspnet-mvc-app/
					}
				}
			}
		};

		if (!coursesForUsers.TryGetValue(userRoles.UserId, out var coursesForUser))
			coursesForUser = new Dictionary<CourseRoleType, List<string>>();

		foreach (var role in Enum.GetValues(typeof(CourseRoleType)).Cast<CourseRoleType>().Where(roles => roles != CourseRoleType.Student))
		{
			user.CourseRoles[role.ToString()] = new ManyCourseRolesModel
			{
				CourseRoles = coursesIds
					.Select(s => new CourseRoleModel
					{
						Role = role,
						CourseId = s,
						VisibleCourseName = allTempCourses.ContainsKey(s)
							? allTempCourses[s].GetVisibleName(courseStorage.GetCourse(s).Title)
							: courseStorage.GetCourse(s).Title,
						HasAccess = coursesForUser.ContainsKey(role) && coursesForUser[role].Contains(s.ToLower()),
						ToggleUrl = Url.Content($"~/Account/{nameof(ToggleRole)}?courseId={s}&userId={user.UserId}&role={role}"),
						UserName = user.UserVisibleName,
					})
					.OrderBy(s => s.VisibleCourseName, StringComparer.InvariantCultureIgnoreCase)
					.ToList()
			};
		}

		var systemAccesses = (await systemAccessesRepo.GetSystemAccessesAsync(user.UserId)).Select(a => a.AccessType);
		user.SystemAccesses = Enum.GetValues(typeof(SystemAccessType))
			.Cast<SystemAccessType>()
			.ToDictionary(
				a => a,
				a => new SystemAccessModel
				{
					HasAccess = systemAccesses.Contains(a),
					ToggleUrl = Url.Content($"~/Account/{nameof(ToggleSystemAccess)}?userId={user.UserId}&accessType={a}"),
					UserName = user.UserVisibleName,
				}
			);

		return user;
	}

	private async Task NotifyAboutUserJoinedToGroup(Group group, string userId)
	{
		var notification = new JoinedToYourGroupNotification
		{
			Group = group,
			JoinedUserId = userId
		};
		await notificationsRepo.AddNotification(group.CourseId, notification, userId);
	}

	public async Task<ActionResult> JoinGroup(Guid hash)
	{
		var userId = User.GetUserId();
		var group = await groupsRepo.FindGroupByInviteHashAsync(hash);

		if (group != null && group.Members.Any(u => u.UserId == userId))
			return Redirect(Url.RouteUrl("Course.Slide", new { courseId = group.CourseId }));

		if (group is not { IsInviteLinkEnabled: true })
			return new NotFoundResult();

		if (Request.Method != "POST")
			return View(group);

		var alreadyInGroup = await groupMembersRepo.AddUserToGroupAsync(group.Id, userId) == null;
		if (!alreadyInGroup)
			await NotifyAboutUserJoinedToGroup(group, userId);

		await slideCheckingsRepo.ResetManualCheckingLimitsForUser(group.CourseId, userId).ConfigureAwait(false);

		return View("JoinedToGroup", group);
	}

	[Authorize(Policy = UlearnAuthorizationBuilder.SysAdminsPolicyName)] //[ULearnAuthorize(ShouldBeSysAdmin = true)]
	[ValidateAntiForgeryToken]
	//[HandleHttpAntiForgeryException]
	public async Task<ActionResult> ToggleSystemRole(string userId, string role)
	{
		if (userId == User.GetUserId())
			return new BadRequestResult();
		var user = await userManager.FindByIdAsync(userId);
		if (await userManager.IsInRoleAsync(user, role))
			await userManager.RemoveFromRoleAsync(user, role);
		else
			await userManager.AddToRoleAsync(user, role);
		return Content(role);
	}

	private async Task NotifyAboutNewInstructor(string courseId, string userId, string initiatedUserId)
	{
		var notification = new AddedInstructorNotification
		{
			AddedUserId = userId,
		};
		await notificationsRepo.AddNotification(courseId, notification, initiatedUserId);
	}

	[Authorize(Policy = UlearnAuthorizationBuilder.InstructorsPolicyName)] 
	[ValidateAntiForgeryToken]
	//[HandleHttpAntiForgeryException]
	public async Task<ActionResult> ToggleRole(string courseId, string userId, CourseRoleType role)
	{
		var comment = Request.Form["comment"];
		var currentUserId = User.GetUserId();
		var isCourseAdmin = User.HasAccessFor(courseId, CourseRoleType.CourseAdmin);
		if ((await userManager.FindByIdAsync(userId) == null || userId == currentUserId) && (!isCourseAdmin || role == CourseRoleType.CourseAdmin) && !User.IsSystemAdministrator())
			return Json(new { status = "error", message = "Вы не можете изменить эту роль у самих себя." });

		var canAddInstructors = await coursesRepo.HasCourseAccess(currentUserId, courseId, CourseAccessType.AddAndRemoveInstructors);
		if (!isCourseAdmin && !canAddInstructors)
			return Json(new { status = "error", message = "У вас нет прав назначать преподавателей или тестеров. Это могут делать только администраторы курса и преподаватели со специальными правами." });

		if (!isCourseAdmin && role == CourseRoleType.CourseAdmin)
			return Json(new { status = "error", message = "Вы не можете назначать администраторов курса. Это могут делать только другие администраторы курса." });

		var enabledRole = await courseRolesRepo.ToggleRole(courseId, userId, role, currentUserId, comment);

		if (enabledRole && (role == CourseRoleType.Instructor || role == CourseRoleType.CourseAdmin))
			await NotifyAboutNewInstructor(courseId, userId, currentUserId);

		return Json(new { status = "ok", role = role.ToString() });
	}

	[HttpPost]
	[Authorize(Policy = UlearnAuthorizationBuilder.SysAdminsPolicyName)] //[ULearnAuthorize(ShouldBeSysAdmin = true)]
	[ValidateAntiForgeryToken]
	[HandleHttpAntiForgeryException]
	public async Task<ActionResult> DeleteUser(string userId)
	{
		var user = await usersRepo.FindUserById(userId);
		if (user != null)
		{
			/* Log out user everywhere: https://msdn.microsoft.com/en-us/library/dn497579%28v=vs.108%29.aspx?f=255&MSPPError=-2147217396 */
			await userManager.UpdateSecurityStampAsync(user);

			await usersRepo.DeleteUserAsync(user);
		}

		return RedirectToAction("List");
	}

	[Authorize(Policy = UlearnAuthorizationBuilder.InstructorsPolicyName)] 
	/* Now we use AccountController.Profile and don't use AccountController.Info, but this method exists for back compatibility */
	public ActionResult Info(string userName)
	{
		var user = db.Users.FirstOrDefault(u => (u.Id == userName || u.UserName == userName) && !u.IsDeleted);
		if (user == null)
			return NotFound();

		return RedirectToAction("Profile", new { userId = user.Id });
	}

	[Authorize(Policy = UlearnAuthorizationBuilder.InstructorsPolicyName)] 
	public async Task<ActionResult> CourseInfo(string userId, string courseId)
	{
		var user = await usersRepo.FindUserById(userId);
		if (user == null)
			return RedirectToAction("List");

		var course = courseStorage.GetCourse(courseId);

		return View(new UserCourseModel(course, user, db));
	}

	[Authorize(Policy = UlearnAuthorizationBuilder.InstructorsPolicyName)] 
	public async Task<ActionResult> ToggleRolesHistory(string userId, string courseId)
	{
		var user = await usersRepo.FindUserById(userId);
		if (user == null)
			return RedirectToAction("List");

		var course = courseStorage.GetCourse(courseId);
		var model = new UserCourseToggleHistoryModel(user, course,
			await ToSingleCourseRolesHistoryModel(await courseRolesRepo.GetUserRolesHistoryByCourseId(userId, courseId)),
			await ToSingleCourseAccessHistoryModel(await coursesRepo.GetUserAccessHistoryByCourseId(userId, courseId)));
		return View(model);
	}

	public async Task<ActionResult> Profile(string userId)
	{
		var user = await usersRepo.FindUserById(userId);
		if (user == null)
			return NotFound();

		if (!await systemAccessesRepo.HasSystemAccessAsync(User.GetUserId(), SystemAccessType.ViewAllProfiles) && !User.IsSystemAdministrator())
			return NotFound();

		var logins = await userManager.GetLoginsAsync(user);

		var userCoursesIds = (await visitsRepo.GetUserCourses(user.Id)).Select(s => s.ToLower());
		var courses = courseStorage.GetCourses().ToList();
		var userCourses = courses.Where(c => userCoursesIds.Contains(c.Id.ToLower())).OrderBy(c => c.Title).ToList();

		var allCourses = courses.ToDictionary(c => c.Id, c => c, StringComparer.InvariantCultureIgnoreCase);
		var allTempCourses = (await tempCoursesRepo.GetAllTempCourses()) // Все, потому что старые могут быть еще в памяти.
			.ToDictionary(t => t.CourseId, t => t, StringComparer.InvariantCultureIgnoreCase);
		var certificates = (await certificatesRepo.GetUserCertificates(user.Id)).OrderBy(c => allCourses.GetOrDefault(c.Template.CourseId)?.Title ?? "<курс удалён>").ToList();

		var courseActualGroups = new Dictionary<string, string>();
		var courseArchivedGroups = new Dictionary<string, string>();
		foreach (var course in userCourses)
		{
			var groups = await groupMembersRepo.GetUserGroupsAsync(course.Id, userId, true);
			courseActualGroups[course.Id] = string.Join(',', groups.Where(g => !g.IsArchived).Select(g => g.Name));
			courseArchivedGroups[course.Id] = string.Join(',', groups.Where(g => g.IsArchived).Select(g => g.Name));
		}

		var coursesWithRoles = (await courseRolesRepo.GetUserRolesHistory(userId))
			.Select(x => x.CourseId.ToLower())
			.Distinct()
			.ToList();
		var coursesWithAccess = (await coursesRepo.GetUserAccessHistory(userId))
			.Select(x => x.CourseId.ToLower())
			.Distinct()
			.ToList();

		return View(new ProfileModel
		{
			User = user,
			Logins = logins,
			UserCourses = userCourses,
			CourseGroups = courseActualGroups,
			CourseArchivedGroups = courseArchivedGroups,
			Certificates = certificates,
			AllCourses = allCourses,
			AllTempCourses = allTempCourses,
			CoursesWithRoles = coursesWithRoles,
			CoursesWithAccess = coursesWithAccess
		});
	}

	private async Task<List<UserToggleModel>> ToSingleCourseAccessHistoryModel(List<CourseAccess> historyByCourse)
	{
		var result = new List<UserToggleModel>();
		foreach (var courseAccess in historyByCourse)
		{
			var grantedBy = await usersRepo.FindUserById(courseAccess.GrantedById);
			result.Add(new UserToggleModel
			{
				IsEnabled = courseAccess.IsEnabled,
				GrantedBy = grantedBy.VisibleName,
				Comment = courseAccess.Comment,
				GrantTimeUtc = courseAccess.GrantTime,
				Grant = courseAccess.AccessType.GetDisplayName(),
				GrantType = GrantType.Access
			});
		}

		return result;
	}

	private async Task<List<UserToggleModel>> ToSingleCourseRolesHistoryModel(List<CourseRole> historyByCourse)
	{
		var userToggleModelsList = new List<UserToggleModel>();
		foreach (var courseRole in historyByCourse)
		{
			var grantedBy = (await usersRepo.FindUserById(courseRole.GrantedById))?.VisibleName;
			userToggleModelsList.Add(new UserToggleModel
			{
				IsEnabled = courseRole.IsEnabled ?? true,
				GrantedBy = courseRole.GrantedById == null ? "" : grantedBy,
				Comment = courseRole.Comment,
				GrantTimeUtc = courseRole.GrantTime ?? DateTime.MinValue,
				Grant = courseRole.Role.GetDisplayName(),
				GrantType = GrantType.Role
			});
		}

		return userToggleModelsList;
	}


	[AllowAnonymous]
	public async Task<IActionResult> Register(string returnUrl = null)
	{
		return View(new RegistrationViewModel { ReturnUrl = returnUrl });
	}

	[HttpPost]
	[AllowAnonymous]
	[ValidateAntiForgeryToken]
	[HandleHttpAntiForgeryException]
	public async Task<ActionResult> Register(RegistrationViewModel model)
	{
		if (ModelState.IsValid)
		{
			/* Some users enter email with trailing whitespaces. Remove them (not users, but spaces!) */
			model.Email = (model.Email ?? "").Trim();

			if (!await CanNewUserSetThisEmail(model.Email))
			{
				ModelState.AddModelError("Email", ManageMessageId.EmailAlreadyTaken.GetDisplayName());
				return View(model);
			}

			var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, Gender = model.Gender };
			var result = await userManager.CreateAsync(user, model.Password);
			if (result.Succeeded)
			{
				await authenticationManager.LoginAsync(HttpContext, user, isPersistent: true);

				if (!await SendConfirmationEmail(user))
				{
					log.Warn("Register(): can't send confirmation email");
					model.ReturnUrl = Url.Action("Manage", "Account", new { Message = ManageMessageId.ErrorOccured });
				}
				else if (string.IsNullOrWhiteSpace(model.ReturnUrl))
					model.ReturnUrl = Url.Action("Index", "Home");
				else
					model.ReturnUrl = this.FixRedirectUrl(model.ReturnUrl);

				metricSender.SendCount("registration.success");

				model.RegistrationFinished = true;
			}
			else
				this.AddErrors(result);
		}

		return View(model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	//[HandleHttpAntiForgeryException]
	public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
	{
		var user = await userManager.FindByIdAsync(User.GetUserId());
		var result = await userManager.RemoveLoginAsync(user, loginProvider, providerKey);
		var message = result.Succeeded ? ManageMessageId.LoginRemoved : ManageMessageId.ErrorOccured;
		return RedirectToAction("Manage", new { Message = message });
	}

	public async Task<ActionResult> Manage(ManageMessageId? message, string provider = "", string otherUserId = "")
	{
		ViewBag.StatusMessage = message?.GetAttribute<DisplayAttribute>().GetName();
		ViewBag.IsStatusMessageAboutSocialLogins = message == ManageMessageId.LoginAdded || message == ManageMessageId.LoginRemoved;
		if (message == ManageMessageId.AlreadyLinkedToOtherUser)
		{
			var otherUser = await userManager.FindByIdAsync(otherUserId);
			ViewBag.StatusMessage += $" {provider ?? ""}. Аккаунт уже привязан к пользователю {otherUser?.UserName ?? ""}.";
		}

		ViewBag.IsStatusError = message?.GetAttribute<IsErrorAttribute>()?.IsError ?? IsErrorAttribute.DefaultValue;
		ViewBag.HasLocalPassword = await ControllerUtils.HasPassword(userManager, User.GetUserId());
		ViewBag.ReturnUrl = Url.Action("Manage");
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[HandleHttpAntiForgeryException]
	public async Task<ActionResult> Manage(ManageUserViewModel model)
	{
		var hasPassword = await ControllerUtils.HasPassword(userManager, User.GetUserId());
		ViewBag.HasLocalPassword = hasPassword;
		ViewBag.ReturnUrl = Url.Action("Manage");
		if (hasPassword)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.GetUserAsync(User);
				var result = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
				if (result.Succeeded)
				{
					return RedirectToAction("Manage", new { Message = ManageMessageId.PasswordChanged });
				}

				this.AddErrors(result);
			}
			else
			{
				ModelState.AddModelError("", "Есть ошибки, давай поправим");
			}
		}
		else
		{
			// User does not have a password so remove any validation errors caused by a missing OldPassword field
			var state = ModelState["OldPassword"];
			state?.Errors.Clear();

			if (ModelState.IsValid)
			{
				var user = await userManager.GetUserAsync(User);
				var result = await userManager.AddPasswordAsync(user, model.NewPassword);
				if (result.Succeeded)
				{
					return RedirectToAction("Manage", new { Message = ManageMessageId.PasswordSet });
				}

				this.AddErrors(result);
			}
			else
			{
				ModelState.AddModelError("", "Есть ошибки, давай поправим");
			}
		}

		// If we got this far, something failed, redisplay form
		return View(model);
	}

	public async Task<ActionResult> StudentInfo()
	{
		var userId = User.GetUserId();
		var user = await userManager.FindByIdAsync(userId);
		return View(new LtiUserViewModel
		{
			FirstName = user.FirstName,
			LastName = user.LastName,
			Email = user.Email,
		});
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[HandleHttpAntiForgeryException]
	public async Task<ActionResult> StudentInfo(LtiUserViewModel userInfo)
	{
		var userId = User.GetUserId();
		var user = await userManager.FindByIdAsync(userId);
		user.FirstName = userInfo.FirstName;
		user.LastName = userInfo.LastName;
		user.Email = (userInfo.Email ?? "").Trim();
		user.LastEdit = DateTime.Now;
		await userManager.UpdateAsync(user);
		return RedirectToAction("StudentInfo");
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && userManager != null)
		{
			userManager.Dispose();
			userManager = null;
		}

		base.Dispose(disposing);
	}

	public enum ManageMessageId
	{
		[Display(Name = "Пароль изменён")]
		PasswordChanged,

		[Display(Name = "Пароль установлен")]
		PasswordSet,

		[Display(Name = "Привязка удалена")]
		LoginRemoved,

		[Display(Name = "Ваша почта уже подтверждена")]
		EmailAlreadyConfirmed,

		[Display(Name = "Не получилось привязать аккаунт")]
		[IsError(true)]
		AlreadyLinkedToOtherUser,

		[Display(Name = "Мы отправили вам письмо для подтверждения адреса")]
		ConfirmationEmailSent,

		[Display(Name = "Адрес электронной почты подтверждён")]
		EmailConfirmed,

		[Display(Name = "Аккаунт telegram добавлен в ваш профиль")]
		TelegramAdded,

		[Display(Name = "Аккаунт telegram удален из вашего профиля")]
		TelegramRemoved,

		[Display(Name = "У вас не указан адрес эл. почты")]
		[IsError(true)]
		UserHasNoEmail,

		[Display(Name = "Произошла ошибка. Если она будет повторяться, напишите нам на support@ulearn.me.")]
		[IsError(true)]
		ErrorOccured,

		[Display(Name = "Аккаунт привязан")]
		LoginAdded,

		[Display(Name = "Это имя уже занято, выберите другое")]
		[IsError(true)]
		NameAlreadyTaken,

		[Display(Name = "Этот адрес электронной почты уже используется другим пользователем")]
		[IsError(true)]
		EmailAlreadyTaken,

		[Display(Name = "Не все поля заполнены верно. Проверьте, пожалуйста, и попробуйте ещё раз")]
		[IsError(true)]
		NotAllFieldsFilled
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[HandleHttpAntiForgeryException]
	public async Task<ActionResult> ChangeDetailsPartial(UserViewModel userModel)
	{
		if (userModel.Render)
		{
			ModelState.Clear();

			return ViewComponent("ChangeDetailsPartial");
		}

		if (string.IsNullOrEmpty(userModel.Name))
		{
			return RedirectToAction("Manage", new { Message = ManageMessageId.NotAllFieldsFilled });
		}

		var user = await userManager.FindByIdAsync(User.GetUserId());
		if (user == null)
		{
			await authenticationManager.Logout(HttpContext);
			return RedirectToAction("Index", "Login");
		}

		var nameChanged = user.UserName != userModel.Name;
		if (nameChanged && await userManager.FindByNameAsync(userModel.Name) != null)
		{
			log.Warn($"ChangeDetailsPartial(): name {userModel.Name} is already taken");
			return RedirectToAction("Manage", new { Message = ManageMessageId.NameAlreadyTaken });
		}

		/* Some users enter email with trailing whitespaces. Remove them (not users, but spaces!) */
		userModel.Email = (userModel.Email ?? "").Trim();
		var emailChanged = string.Compare(user.Email, userModel.Email, StringComparison.OrdinalIgnoreCase) != 0;

		if (emailChanged)
		{
			if (!await CanUserSetThisEmail(user, userModel.Email))
			{
				log.Warn($"ChangeDetailsPartial(): email {userModel.Email} is already taken");
				return RedirectToAction("Manage", new { Message = ManageMessageId.EmailAlreadyTaken });
			}
		}

		user.UserName = userModel.Name;
		user.FirstName = userModel.FirstName;
		user.LastName = userModel.LastName;
		user.Email = userModel.Email;
		user.Gender = userModel.Gender;
		user.LastEdit = DateTime.Now;
		if (!string.IsNullOrEmpty(userModel.Password))
		{
			await userManager.RemovePasswordAsync(user);
			await userManager.AddPasswordAsync(user, userModel.Password);
		}

		await userManager.UpdateAsync(user);

		if (emailChanged)
			await ChangeEmail(user, user.Email).ConfigureAwait(false);

		if (nameChanged)
		{
			await authenticationManager.Logout(HttpContext);
			return RedirectToAction("Index", "Login");
		}

		return RedirectToAction("Manage");
	}

	public async Task<ActionResult> RemoveTelegram()
	{
		var userId = User.GetUserId();
		await usersRepo.ChangeTelegram(userId, null, null).ConfigureAwait(false);
		var telegramTransport = notificationsRepo.FindUsersNotificationTransport<TelegramNotificationTransport>(userId);
		if (telegramTransport != null)
			await notificationsRepo.EnableNotificationTransport(telegramTransport.Id, false).ConfigureAwait(false);
		return RedirectToAction("Manage", new { Message = ManageMessageId.TelegramRemoved });
	}

	[HttpPost]
	[Authorize(Policy = UlearnAuthorizationBuilder.SysAdminsPolicyName)] //[ULearnAuthorize(ShouldBeSysAdmin = true)]
	[ValidateAntiForgeryToken]
	[HandleHttpAntiForgeryException]
	public async Task<ActionResult> ResetPassword(string newPassword, string userId)
	{
		var user = await userManager.FindByIdAsync(userId);
		if (user == null)
			return RedirectToAction("List");
		await userManager.RemovePasswordAsync(user);
		await userManager.AddPasswordAsync(user, newPassword);
		return RedirectToAction("Profile", new { userId = user.Id });
	}

	[AllowAnonymous]
	public async Task<ActionResult> UserMenuPartial()
	{
		var isAuthenticated = User.Identity.IsAuthenticated;
		var userId = User.GetUserId();
		var user = await userManager.FindByIdAsync(userId);
		return PartialView(new UserMenuPartialViewModel
		{
			IsAuthenticated = isAuthenticated,
			User = user,
		});
	}

	public async Task<ActionResult> AddTelegram(long chatId, string chatTitle, string hash)
	{
		metricSender.SendCount("connect_telegram.try");
		var correctHash = notificationsRepo.GetSecretHashForTelegramTransport(chatId, chatTitle, telegramSecret);
		if (hash != correctHash)
			return Forbid();

		var userId = User.GetUserId();
		await usersRepo.ChangeTelegram(userId, chatId, chatTitle).ConfigureAwait(false);
		metricSender.SendCount("connect_telegram.success");
		var telegramTransport = notificationsRepo.FindUsersNotificationTransport<TelegramNotificationTransport>(userId);
		if (telegramTransport != null)
			await notificationsRepo.EnableNotificationTransport(telegramTransport.Id).ConfigureAwait(false);
		else
		{
			await notificationsRepo.AddNotificationTransport(new TelegramNotificationTransport
			{
				UserId = userId,
				IsEnabled = true,
			}).ConfigureAwait(false);
		}

		return RedirectToAction("Manage", new { Message = ManageMessageId.TelegramAdded });
	}

	[AllowAnonymous]
	public async Task<ActionResult> ConfirmEmail(string email, string signature, string userId = "")
	{
		metricSender.SendCount("email_confirmation.go_by_link_from_email");

		var realUserId = string.IsNullOrEmpty(userId) ? User.GetUserId() : userId;
		if (string.IsNullOrEmpty(realUserId))
			return NotFound();

		var correctSignature = GetEmailConfirmationSignature(email, realUserId);
		if (signature != correctSignature)
		{
			metricSender.SendCount("email_confirmation.attempt_to_hack_account"); //https://yt.skbkontur.ru/issue/WHSIyt-2443
			log.Warn($"Invalid signature in confirmation email link, expected \"{correctSignature}\", actual \"{signature}\". Email is \"{email}\",");
			return RedirectToAction("Manage", new { Message = ManageMessageId.ErrorOccured });
		}

		var user = await userManager.FindByIdAsync(realUserId).ConfigureAwait(false);
		if (!User.Identity.IsAuthenticated || User.GetUserId() != realUserId)
			await authenticationManager.LoginAsync(HttpContext, user, isPersistent: false).ConfigureAwait(false);

		if (user.Email != email || user.EmailConfirmed)
			return RedirectToAction("Manage", new { Message = ManageMessageId.EmailAlreadyConfirmed });

		/* Is there are exist other users with same confirmed email, then un-confirm their emails */
		var usersWithSameEmail = await usersRepo.FindUsersByConfirmedEmail(email); //FindUsersByEmail(email);
		foreach (var otherUser in usersWithSameEmail)
			if (otherUser.EmailConfirmed)
				await usersRepo.ConfirmEmail(otherUser.Id, false).ConfigureAwait(false);

		await usersRepo.ConfirmEmail(realUserId).ConfigureAwait(false);
		metricSender.SendCount("email_confirmation.confirmed");

		/* Enable notification transport if it exists or create auto-enabled mail notification transport */
		var mailNotificationTransport = notificationsRepo.FindUsersNotificationTransport<MailNotificationTransport>(realUserId, includeDisabled: true);
		if (mailNotificationTransport != null)
			await notificationsRepo.EnableNotificationTransport(mailNotificationTransport.Id).ConfigureAwait(false);
		else
			await notificationsRepo.AddNotificationTransport(new MailNotificationTransport
			{
				User = user,
				IsEnabled = true,
			}).ConfigureAwait(false);

		return RedirectToAction("Manage", new { Message = ManageMessageId.EmailConfirmed });
	}

	public async Task<ActionResult> SendConfirmationEmail()
	{
		var userId = User.GetUserId();
		var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
		if (string.IsNullOrEmpty(user.Email))
			return RedirectToAction("Manage", new { Message = ManageMessageId.UserHasNoEmail });

		if (user.EmailConfirmed)
			return RedirectToAction("Manage", new { Message = ManageMessageId.EmailAlreadyConfirmed });

		if (!await SendConfirmationEmail(user).ConfigureAwait(false))
		{
			log.Warn($"SendConfirmationEmail(): can't send confirmation email to user {user}");
			return RedirectToAction("Manage", new { Message = ManageMessageId.ErrorOccured });
		}

		return RedirectToAction("Manage");
	}

	public async Task ChangeEmail(ApplicationUser user, string email)
	{
		await usersRepo.ChangeEmail(user, email).ConfigureAwait(false);

		/* Disable mail notification transport if exists */
		var mailNotificationTransport = await notificationsRepo.FindUsersNotificationTransport<MailNotificationTransport>(user.Id);
		if (mailNotificationTransport != null)
			await notificationsRepo.EnableNotificationTransport(mailNotificationTransport.Id, isEnabled: false).ConfigureAwait(false);

		/* Send confirmation email to the new address */
		await SendConfirmationEmail(user).ConfigureAwait(false);
	}

	[Authorize(Policy = UlearnAuthorizationBuilder.SysAdminsPolicyName)] //[ULearnAuthorize(ShouldBeSysAdmin = true)]
	[HttpPost]
	public async Task<ActionResult> ToggleSystemAccess(string userId, SystemAccessType accessType, bool isEnabled)
	{
		var currentUserId = User.GetUserId();
		if (isEnabled)
			await systemAccessesRepo.GrantAccessAsync(userId, accessType, currentUserId);
		else
			await systemAccessesRepo.RevokeAccessAsync(userId, accessType);

		return Json(new { status = "ok" });
	}

	[Authorize(Policy = UlearnAuthorizationBuilder.SysAdminsPolicyName)] //[ULearnAuthorize(ShouldBeSysAdmin = true)]
	[HttpPost]
	public async Task<ActionResult> Hijack(string userId)
	{
		var user = await userManager.FindByIdAsync(userId);
		if (user == null)
			return NotFound("User not found");

		CopyHijackedCookies(HttpContext.Request, HttpContext.Response, s => s, s => s + ".hijack", removeOld: false);
		await authenticationManager.LoginAsync(HttpContext, user, isPersistent: false);

		return Redirect("/");
	}

	[HttpPost]
	[AllowAnonymous]
	public ActionResult ReturnHijack()
	{
		var hijackedUserId = User.GetUserId();
		CopyHijackedCookies(HttpContext.Request, HttpContext.Response, s => s + ".hijack", s => s, removeOld: true);
		return RedirectToAction("Profile", "Account", new { userId = hijackedUserId });
	}

	private void CopyHijackedCookies(HttpRequest request, HttpResponse response, Func<string, string> actualCookie, Func<string, string> newCookie, bool removeOld)
	{
		foreach (var cookieName in hijackCookies)
		{
			var cookie = request.Cookies[actualCookie(cookieName)];
			if (cookie == null)
				continue;

			response.Cookies.Append(newCookie(cookieName), cookie, new CookieOptions()
			{
				Domain = configuration.Web.CookieDomain,
				Secure = configuration.Web.CookieSecure
			});

			if (removeOld)
				response.Cookies.Delete(actualCookie(cookieName));
		}

		// 	response.Cookies.Add(new HttpCookie(newCookie(cookieName), cookie.Value)
		// 	{
		// 		Domain = configuration.Web.CookieDomain,
		// 		Secure = configuration.Web.CookieSecure
		// 	});
		//
		// 	if (removeOld)
		// 		response.Cookies.Add(new HttpCookie(actualCookie(cookieName), "")
		// 		{
		// 			Expires = DateTime.Now.AddDays(-1),
		// 			Domain = configuration.Web.CookieDomain,
		// 			Secure = configuration.Web.CookieSecure
		// 		});
		// }
	}
}

public class ProfileModel
{
	public ApplicationUser User { get; set; }
	public IList<UserLoginInfo> Logins { get; set; }
	public List<Course> UserCourses { get; set; }
	public List<Certificate> Certificates { get; set; }
	public Dictionary<string, Course> AllCourses { get; set; }
	public Dictionary<string, string> CourseGroups { get; set; }
	public Dictionary<string, string> CourseArchivedGroups { get; set; }

	public Dictionary<string, TempCourse> AllTempCourses { get; set; }

	public List<string> CoursesWithRoles;

	public List<string> CoursesWithAccess;
}