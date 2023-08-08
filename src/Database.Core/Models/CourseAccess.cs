using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Ulearn.Common.Extensions;

namespace Database.Models
{
	[Index(nameof(CourseId))]
	[Index(nameof(CourseId), nameof(IsEnabled))]
	[Index(nameof(CourseId), nameof(UserId), nameof(IsEnabled))]
	[Index(nameof(GrantTime))]
	public class CourseAccess
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		public string CourseId { get; set; }

		[StringLength(64)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[StringLength(64)]
		public string GrantedById { get; set; }

		public virtual ApplicationUser GrantedBy { get; set; }

		[Required]
		public CourseAccessType AccessType { get; set; }

		public DateTime GrantTime { get; set; }

		[Required]
		public bool IsEnabled { get; set; }

		[CanBeNull]
		public string Comment { get; set; }
	}

	[JsonConverter(typeof(StringEnumConverter), true)]
	public enum CourseAccessType : short
	{
		/* Редактировать, закреплять, удалять (скрывать) комментарии, видеть неопубликованные комментарии */
		[Display(Name = "Редактировать и удалять комментарии")]
		EditPinAndRemoveComments = 1,

		/* Смотреть решения всех, а не только студентов своих групп */
		[Display(Name = "Видеть решения всех пользователей")]
		ViewAllStudentsSubmissions = 2,

		/* Назначать людей преподавателями */
		[Display(Name = "Назначать преподавателей")]
		AddAndRemoveInstructors = 3,

		[Display(Name = "Видеть, в каких группах состоят все студенты")]
		ViewAllGroupMembers = 4,

		[Display(Name = "Модерировать флешкарты")]
		[StudentCourseAccess]
		ModerateUserGeneratedFlashcards = 5,

		[Display(Name = "Получать в АПИ статистику по код-ревью (/codereveiew/statistics)")]
		ApiViewCodeReviewStatistics = 101,


		/*
		// Antiplagiarism service is enabled for everyone now. But don't use value 1001 for another features to avoid collissions.		
		[Display(Name = "Фича: антиплагиат")]
		FeatureUseAntiPlagiarism = 1001,
		*/
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class StudentCourseAccess : Attribute
	{
	}

	public static class CourseAccessTypeExtensions
	{
		public static string GetAuthorizationPolicyName(this CourseAccessType accessType)
		{
			return "CourseAccess." + Enum.GetName(typeof(CourseAccessType), accessType);
		}

		public static bool IsStudentCourseAccess(this CourseAccessType accessType) =>
			accessType.GetAttribute<StudentCourseAccess>() is not null;
	}
}