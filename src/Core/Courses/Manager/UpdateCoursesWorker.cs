using System;
using System.Threading;
using Vostok.Applications.Scheduled;
using Vostok.Hosting.Abstractions;

namespace Ulearn.Core.Courses.Manager
{
	public class UpdateCoursesWorker : VostokScheduledApplication
	{
		private readonly ICourseUpdater courseUpdater;
		private readonly TimeSpan coursesUpdatePeriod = TimeSpan.FromMilliseconds(1000);
		private readonly TimeSpan tempCoursesUpdatePeriod = TimeSpan.FromMilliseconds(500);
		public static readonly string UpdateCoursesJobName = "UpdateCoursesJob";
		public static readonly string UpdateTempCoursesJobName = "UpdateTempCoursesJob";

		public UpdateCoursesWorker(ICourseUpdater courseUpdater)
		{
			this.courseUpdater = courseUpdater;
		}

		public override void Setup(IScheduledActionsBuilder builder, IVostokHostingEnvironment environment)
		{
			RunUpdateCoursesWorker(builder);
		}

		private void RunUpdateCoursesWorker(IScheduledActionsBuilder builder)
		{
			var updateCoursesScheduler = Scheduler.Multi(Scheduler.Periodical(coursesUpdatePeriod), Scheduler.OnDemand(out var updateCourses));
			builder.Schedule(UpdateCoursesJobName, updateCoursesScheduler, courseUpdater.UpdateCoursesAsync);

			var updateTempCoursesScheduler = Scheduler.Multi(Scheduler.Periodical(tempCoursesUpdatePeriod), Scheduler.OnDemand(out var updateTempCourses));
			builder.Schedule(UpdateTempCoursesJobName, updateTempCoursesScheduler, courseUpdater.UpdateTempCoursesAsync);

			courseUpdater.UpdateCoursesAsync().Wait(); // в этом потоке
			updateTempCourses(); // в другом потоке
		}

		public void DoInitialCourseLoadAndRunCoursesUpdateInThreads()
		{
			courseUpdater.UpdateCoursesAsync().Wait();
			var coursesThread = new Thread(UpdateCoursesLoop);
			coursesThread.Start();
			var tempCoursesThread = new Thread(UpdateTempCoursesLoop);
			tempCoursesThread.Start();
		}

		private void UpdateCoursesLoop()
		{
			while (true)
			{
				courseUpdater.UpdateCoursesAsync().Wait();
				Thread.Sleep(coursesUpdatePeriod);
			}
			// ReSharper disable once FunctionNeverReturns
		}

		private void UpdateTempCoursesLoop()
		{
			while (true)
			{
				courseUpdater.UpdateTempCoursesAsync().Wait();
				Thread.Sleep(tempCoursesUpdatePeriod);
			}
			// ReSharper disable once FunctionNeverReturns
		}
	}
}