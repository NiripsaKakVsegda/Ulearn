using System;
using System.Collections.Generic;
using Vostok.Logging.Abstractions;

namespace CourseToolHotReloader.Application;

public interface ILogManager
{
	void LogInfo(string message);
	void LogInfoWithTime(string message);
	void LogError(string message, Exception? e = null);
	void LogErrorWithTime(string message, Exception? e = null);
	LogPause PauseLogging();
	void ResumeLogging();
}

public class ConsoleLogManager : ILogManager
{
	private readonly object locker = new();
	private bool isLoggingPaused;
	private readonly Queue<LogMessage> logsQueue = new();

	private static ILog Log => LogProvider.Get();

	public void LogInfo(string message)
	{
		Log.Info(message);
		lock (locker)
		{
			if (isLoggingPaused)
				logsQueue.Enqueue(new LogMessage(message));
			else
				WriteInfoInConsole(message);
		}
	}

	public void LogInfoWithTime(string message)
	{
		Log.Info(message);
		message = $"{DateTime.Now:HH:mm:ss} {message}";
		lock (locker)
		{
			if (isLoggingPaused)
				logsQueue.Enqueue(new LogMessage(message));
			else
				WriteInfoInConsole(message);
		}
	}

	public void LogError(string message, Exception? e = null)
	{
		if (e is null)
			Log.Error(message);
		else
			Log.Error(e, message);

		lock (locker)
		{
			if (isLoggingPaused)
				logsQueue.Enqueue(new LogMessage(message, true));
			else
				WriteErrorInConsole(message);
		}
	}

	public void LogErrorWithTime(string message, Exception? e = null)
	{
		if (e is null)
			Log.Error(message);
		else
			Log.Error(e, message);

		message = $"{DateTime.Now:HH:mm:ss} {message}";
		lock (locker)
		{
			if (isLoggingPaused)
				logsQueue.Enqueue(new LogMessage(message, true));
			else
				WriteErrorInConsole(message);
		}
	}

	public LogPause PauseLogging()
	{
		lock (locker)
		{
			isLoggingPaused = true;
		}

		return new LogPause(this);
	}

	public void ResumeLogging()
	{
		lock (locker)
		{
			while (logsQueue.TryDequeue(out var logMessage))
			{
				if (logMessage.IsError)
					WriteErrorInConsole(logMessage.Message);
				else
					WriteInfoInConsole(logMessage.Message);
			}

			isLoggingPaused = false;
		}
	}

	private static void WriteErrorInConsole(string message)
	{
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine(message);
		Console.ResetColor();
	}

	private static void WriteInfoInConsole(string message)
	{
		Console.WriteLine(message);
	}

	private readonly record struct LogMessage(string Message, bool IsError = false);
}

public class LogPause : IDisposable
{
	private readonly ILogManager logManager;

	public LogPause(ILogManager logManager)
	{
		this.logManager = logManager;
	}

	public void Resume()
	{
		logManager.ResumeLogging();
	}

	public void Dispose()
	{
		Resume();
		GC.SuppressFinalize(this);
	}
}