using System;
using System.Collections.Generic;
using System.Linq;
using Database.Models;
using NUnit.Framework;

namespace Database.DataContexts.DeadLines
{
	public static class DeadLinesUtils
	{
		/*
		 * Most freed for the user is deadline with highest score among non overlapped deadlines
		 * 
		 * For example, we have 3 scores 0, 50, 100
		 * And we have different situations with this scores
		 * Each situation has 3 deadlines
		 * 1st and 2nd are active, 3rd will be active in future
		 * [Active] [InActive]
		 * * - will never be used
		 * [0* 50*]   [100] -> 100 (always 100)
		 * [0* 100]  [50]  -> 100 (in future 50)
		 * [50* 0*]   [100] -> 100 (always 100)
		 * [50* 100] [0]   -> 100 (in future 0)
		 * [100 0*]  [50]  -> 50  (100 is overlapped by active 0, 0 overlapped by future 50)
		 * [100 50] [0]   -> 50  (100 is overlapped by active 50, in future 0)
		 */
		public static DeadLine GetCurrentDeadLine(List<DeadLine> deadLines, DateTime? currentTime = null)
		{
			if (deadLines.Count == 0) return null;

			currentTime ??= DateTime.Now;
			var inactiveDeadLines = deadLines.Where(d => d.Date > currentTime).ToList();
			var lastActive = deadLines
				.Where(d => d.Date <= currentTime)
				.OrderByDescending(d => d.Date)
				.ThenByDescending(d => d.ScorePercent)
				.FirstOrDefault();

			DeadLine inactive = null;
			if (inactiveDeadLines.Count > 0)
			{
				var maxScoreAmongInActive = inactiveDeadLines.Max(d => d.ScorePercent);
				inactive = inactiveDeadLines
					.OrderByDescending(d => d.Date)
					.FirstOrDefault(d => d.ScorePercent == maxScoreAmongInActive);
			}

			if (inactive == null) return lastActive;
			if (lastActive == null) return inactive;

			return inactive.ScorePercent >= lastActive.ScorePercent
				? inactive
				: lastActive;
		}
	}
}