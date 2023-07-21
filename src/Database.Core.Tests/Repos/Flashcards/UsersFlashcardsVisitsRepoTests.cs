using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos.Flashcards;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Database.Core.Tests.Repos.Flashcards
{
	[TestFixture]
	public class UsersFlashcardsVisitsRepoTests : BaseRepoTests
	{
		private IUsersFlashcardsVisitsRepo usersFlashcardsVisitsRepo;

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			usersFlashcardsVisitsRepo = serviceProvider.GetService<IUsersFlashcardsVisitsRepo>();
		}

		[Test]
		public async Task AddUserFlashcardVisit()
		{
			var userId = Guid.NewGuid().ToString();
			var flashcardId = Guid.NewGuid().ToString();
			var courseId = "courseId";
			var rate = Rate.Rate5;
			var unitId = Guid.NewGuid();

			var userFlashcardVisit = await usersFlashcardsVisitsRepo
				.AddFlashcardVisitAsync(userId, courseId, unitId, flashcardId, rate).ConfigureAwait(false);

			Assert.AreEqual(flashcardId, userFlashcardVisit.FlashcardId);
			Assert.AreEqual(courseId.ToLower(), userFlashcardVisit.CourseId);
			Assert.AreEqual(userId, userFlashcardVisit.UserId);
			Assert.AreEqual(rate, userFlashcardVisit.Rate);
			Assert.AreEqual(unitId, userFlashcardVisit.UnitId);
		}


		[Test]
		public async Task AddAndGetUserFlashcardVisitsByCourse()
		{
			var userId = Guid.NewGuid().ToString();
			var flashcardId1 = Guid.NewGuid().ToString();
			var flashcardId2 = Guid.NewGuid().ToString();
			var flashcardId3 = Guid.NewGuid().ToString();
			var courseId1 = "courseId1";
			var courseId2 = "courseId2";
			var rate = Rate.Rate5;
			var unitId1 = Guid.NewGuid();
			var unitId2 = Guid.NewGuid();

			var userFlashcardsVisit1 = await usersFlashcardsVisitsRepo
				.AddFlashcardVisitAsync(userId, courseId1, unitId1, flashcardId1, rate).ConfigureAwait(false);

			var userFlashcardsVisit2 = await usersFlashcardsVisitsRepo
				.AddFlashcardVisitAsync(userId, courseId2, unitId2, flashcardId2, rate).ConfigureAwait(false);
			var userFlashcardsVisit3 = await usersFlashcardsVisitsRepo
				.AddFlashcardVisitAsync(userId, courseId2, unitId2, flashcardId3, rate).ConfigureAwait(false);

			var userVisitsByCourse1 = await usersFlashcardsVisitsRepo.GetLastUserFlashcardsVisitsAsync(userId, courseId1);
			var userVisitsByCourse2 = await usersFlashcardsVisitsRepo.GetLastUserFlashcardsVisitsAsync(userId, courseId2);

			Assert.AreEqual(new List<UserFlashcardsVisit> { userFlashcardsVisit1 }, userVisitsByCourse1);
			Assert.AreEqual(new List<UserFlashcardsVisit> { userFlashcardsVisit2, userFlashcardsVisit3 }, userVisitsByCourse2);
		}

		[Test]
		public async Task AddAndGetUserFlashcardVisitsByUnit()
		{
			var userId = Guid.NewGuid().ToString();
			var flashcardId1 = Guid.NewGuid().ToString();
			var flashcardId2 = Guid.NewGuid().ToString();
			var flashcardId3 = Guid.NewGuid().ToString();
			var courseId = "courseId";
			var rate = Rate.Rate5;
			var unitId1 = Guid.NewGuid();
			var unitId2 = Guid.NewGuid();

			var userFlashcardsVisit1 = await usersFlashcardsVisitsRepo
				.AddFlashcardVisitAsync(userId, courseId, unitId1, flashcardId1, rate).ConfigureAwait(false);

			var userFlashcardsVisit2 = await usersFlashcardsVisitsRepo
				.AddFlashcardVisitAsync(userId, courseId, unitId2, flashcardId2, rate).ConfigureAwait(false);
			var userFlashcardsVisit3 = await usersFlashcardsVisitsRepo
				.AddFlashcardVisitAsync(userId, courseId, unitId2, flashcardId3, rate).ConfigureAwait(false);

			var userVisitsByUnit1 = await usersFlashcardsVisitsRepo.GetLastUserFlashcardsVisitsAsync(userId, courseId, unitId1);
			var userVisitsByUnit2 = await usersFlashcardsVisitsRepo.GetLastUserFlashcardsVisitsAsync(userId, courseId, unitId2);

			Assert.AreEqual(new List<UserFlashcardsVisit> { userFlashcardsVisit1 }, userVisitsByUnit1);
			Assert.AreEqual(new List<UserFlashcardsVisit> { userFlashcardsVisit2, userFlashcardsVisit3 }, userVisitsByUnit2);
		}
	}
}