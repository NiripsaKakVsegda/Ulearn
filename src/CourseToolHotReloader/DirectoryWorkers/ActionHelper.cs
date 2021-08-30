using System;
using System.Threading;
using System.Threading.Tasks;

namespace CourseToolHotReloader.DirectoryWorkers
{
	public static class ActionHelper
	{
		public static Action Debounce(Action func, int milliseconds = 1000)
		{
			CancellationTokenSource cancelTokenSource = null;
			return () =>
			{
				cancelTokenSource?.Cancel();
				cancelTokenSource = new CancellationTokenSource();

				Task.Delay(milliseconds, cancelTokenSource.Token)
					.ContinueWith(t =>
					{
						if (t.IsCompletedSuccessfully)
							func();
					}, TaskScheduler.Default);
			};
		}
	}
}