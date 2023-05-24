using System;
using System.Threading;
using System.Threading.Tasks;

namespace CourseToolHotReloader.Infrastructure;

public class ActionDebouncer
{
	public int TimeoutInMilliseconds { get; init; } = 1000;
	private CancellationTokenSource? cancellationTokenSource;

	public Action Debounce(Action func) => () =>
	{
		cancellationTokenSource?.Cancel();
		cancellationTokenSource = new CancellationTokenSource();

		Task.Delay(TimeoutInMilliseconds, cancellationTokenSource.Token)
			.ContinueWith(t =>
			{
				if (t.IsCompletedSuccessfully)
					func();
			}, TaskScheduler.Default);
	};
}