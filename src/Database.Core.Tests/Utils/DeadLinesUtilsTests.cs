using System;
using System.Collections.Generic;
using Database.Models;
using Database.Repos.DeadLines;
using NUnit.Framework;

namespace Database.Core.Tests.Utils
{

	[TestFixture]
	public class DeadLinesUtilsTests
	{
		//[0* 50*]   [100] -> 100 (always 100)
		//[50* 0*]   [100] -> 100 (always 100)
		[Test, Combinatorial]
		public void HighestScoreLast_ShouldBe_AlwaysLast(
			[Values(0, 50, 100)] int firstDeadLineScore,
			[Values(0, 50, 100)] int secondDeadLineScore)
		{
			var deadLines = new List<DeadLine>
			{
				new()
				{
					Date = new DateTime(0),
					ScorePercent = firstDeadLineScore
				},
				new()
				{
					Date = new DateTime(1),
					ScorePercent = secondDeadLineScore
				},
				new()
				{
					Date = new DateTime(2),
					ScorePercent = 100
				},
			};

			var correctDeadLine = deadLines[^1];

			foreach (var deadLine in deadLines)
				Assert.AreEqual(DeadLinesUtils.GetCurrentDeadLine(deadLines, deadLine.Date), correctDeadLine);
		}

		//[100 50] [0]   -> 50  (100 is overlapped by active 50, in future 0)
		[Test]
		public void DecreasingScore_ShouldBe_CurrentByDate()
		{
			var deadLines = new List<DeadLine>
			{
				new()
				{
					Date = new DateTime(0),
					ScorePercent = 100
				},
				new()
				{
					Date = new DateTime(1),
					ScorePercent = 50
				},
				new()
				{
					Date = new DateTime(2),
					ScorePercent = 0
				},
			};

			foreach (var deadLine in deadLines)
				Assert.AreEqual(DeadLinesUtils.GetCurrentDeadLine(deadLines, deadLine.Date), deadLine);
		}

		[Test, Combinatorial]
		public void SameDates_ShouldBe_HighestByScore(
			[Values(0, 50, 100)] int deadLineScore,
			[Values(0, 50, 100)] int anotherDeadLineScore)
		{
			var sameDate = new DateTime(0);
			var highestScore = Math.Max(deadLineScore, anotherDeadLineScore);

			var deadLines = new List<DeadLine>
			{
				new()
				{
					Date = sameDate,
					ScorePercent = deadLineScore
				},
				new()
				{
					Date = sameDate,
					ScorePercent = anotherDeadLineScore
				},
			};

			Assert.AreEqual(DeadLinesUtils.GetCurrentDeadLine(deadLines, sameDate).ScorePercent, highestScore);
		}
	}
}