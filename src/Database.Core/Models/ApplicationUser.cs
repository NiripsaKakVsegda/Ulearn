﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common;

namespace Database.Models
{
	// You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
	[Index(nameof(TelegramChatId))]
	[Index(nameof(IsDeleted))]
	public class ApplicationUser : IdentityUser
	{
		public ApplicationUser()
		{
			Registered = DateTime.Now;
		}

		/* Navigation properties which have been removed from Identity 2.0.
		   See https://docs.microsoft.com/en-us/aspnet/core/migration/1x-to-2x/identity-2x#add-identityuser-poco-navigation-properties
		   for details */

		/// <summary>
		/// Navigation property for the roles this user belongs to.
		/// </summary>
		public virtual ICollection<IdentityUserRole<string>> Roles { get; } = new List<IdentityUserRole<string>>();

		/// <summary>
		/// Navigation property for the claims this user possesses.
		/// </summary>
		public virtual ICollection<IdentityUserClaim<string>> Claims { get; } = new List<IdentityUserClaim<string>>();

		/// <summary>
		/// Navigation property for this users login accounts.
		/// </summary>
		public virtual ICollection<IdentityUserLogin<string>> Logins { get; } = new List<IdentityUserLogin<string>>();

		public string FirstName { get; set; }
		public string LastName { get; set; }
		public DateTime Registered { get; set; }
		public DateTime? LastEdit { get; set; }

		// AvatarUrl is empty if user has no avatar
		public string AvatarUrl { get; set; }

		public bool HasAvatar => !string.IsNullOrEmpty(AvatarUrl);

		// TelegramChatId is null if telegram is not connected to the profile
		public long? TelegramChatId { get; set; }

		public bool HasTelegram => TelegramChatId != null;

		[StringLength(200)]
		public string TelegramChatTitle { get; set; }

		[StringLength(200)]
		public string KonturLogin { get; set; }

		public DateTime? LastConfirmationEmailTime { get; set; }

		public Gender? Gender { get; set; }

		public bool IsDeleted { get; set; }

		// Автогенерируется
		public string Names { get; private set;  }

		public string VisibleName
		{
			get
			{
				if (FirstName + LastName != "")
					return (FirstName + " " + LastName).Trim();
				if (!string.IsNullOrEmpty(UserName))
					return UserName.Trim();
				return "Пользователь";
			}
		}

		public string VisibleNameWithLastNameFirst => ToVisibleNameWithLastNameFirst(UserName, FirstName, LastName);

		public static string ToVisibleNameWithLastNameFirst(string userName, string firstName, string lastName)
		{
			if (firstName + lastName != "")
				return (lastName + " " + firstName).Trim();
			if (!string.IsNullOrEmpty(userName))
				return userName.Trim();
			return "Пользователь";
		}
	}
}