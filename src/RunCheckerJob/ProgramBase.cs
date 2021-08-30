using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Vostok.Logging.Abstractions;
using Ulearn.Core.Configuration;
using Ulearn.Core.Metrics;
using Ulearn.Core.RunCheckerJobApi;

namespace RunCheckerJob
{
	public abstract class ProgramBase
	{
		private readonly string address;
		private readonly string token;
		private readonly TimeSpan sleep;
		private readonly string agentName;

		private readonly ManualResetEvent shutdownEvent = new ManualResetEvent(false);
		private readonly List<Thread> threads = new List<Thread>();

		private static ILog log => LogProvider.Get().ForContext(typeof(ProgramBase));
		protected abstract ISandboxRunnerClient SandboxRunnerClient { get; }
		private readonly string[] supportedSandboxes;
		private readonly string serviceName;

		protected ProgramBase(string serviceName, ManualResetEvent externalShutdownEvent = null)
		{
			this.serviceName = serviceName;
			if (externalShutdownEvent != null)
				shutdownEvent = externalShutdownEvent;

			try
			{
				var ulearnConfiguration = ApplicationConfiguration.Read<UlearnConfiguration>();
				address = ulearnConfiguration.SubmissionsUrl;
				token = ulearnConfiguration.RunnerToken;
				var runCheckerJobConfiguration = ApplicationConfiguration.Read<RunCheckerJobConfiguration>().RunCheckerJob;
				sleep = TimeSpan.FromSeconds(runCheckerJobConfiguration.SleepSeconds ?? 1);
				agentName = runCheckerJobConfiguration.AgentName;
				supportedSandboxes = runCheckerJobConfiguration.SupportedSandboxes;
				if (string.IsNullOrEmpty(agentName))
				{
					agentName = Environment.MachineName;
					log.Info($"Автоопределённое имя клиента: {agentName}. Его можно переопределить в настройках (runcheckerjob:agentName)");
				}
			}
			catch (Exception e)
			{
				log.Error(e, "Root error");
				throw;
			}
		}

		protected void Run(bool joinAllThreads = true)
		{
			log.Info($"Отправляю запросы на {address} для получения новых решений");

			var runCheckerJobConfiguration = ApplicationConfiguration.Read<RunCheckerJobConfiguration>().RunCheckerJob;
			var threadsCount = runCheckerJobConfiguration.ThreadsCount ?? 1;
			if (threadsCount < 1)
			{
				log.Error($"Не могу определить количество потоков для запуска из конфигурации: ${threadsCount}. Количество потоков должно быть положительно");
				throw new ArgumentOutOfRangeException(nameof(threadsCount), $"Number of threads (runcheckerjob:threadsCount) should be positive");
			}

			log.Info($"Запускаю {threadsCount} потока(ов)");
			for (var i = 0; i < threadsCount; i++)
			{
				threads.Add(new Thread(WorkerThread)
				{
					Name = $"Worker #{i}",
					IsBackground = true
				});
			}

			threads.ForEach(t => t.Start());

			if (joinAllThreads)
				threads.ForEach(t => t.Join());
		}

		private void WorkerThread()
		{
			log.Info($"Поток {Thread.CurrentThread.Name} запускается");
			RunOneThread();
		}

		public void Stop()
		{
			shutdownEvent.Set();
			log.Info("Получен сигнал остановки");

			foreach (var thread in threads)
			{
				log.Info($"Пробую остановить поток {thread.Name}");
				if (!thread.Join(10000))
				{
					log.Info($"Вызываю Abort() для потока {thread.Name}");
					thread.Abort();
				}
			}
		}

		private void RunOneThread()
		{
			var fullAgentName = $"{agentName}:Process={Process.GetCurrentProcess().Id}:ThreadId={Thread.CurrentThread.ManagedThreadId}:Thread={Thread.CurrentThread.Name}";
			Client client;
			try
			{
				client = new Client(address, token, fullAgentName);
			}
			catch (Exception e)
			{
				log.Error(e, "Не могу создать HTTP-клиента для отправки запроса на ulearn");
				throw;
			}

			MainLoop(client);
		}

		private void MainLoop(Client client)
		{
			var serviceKeepAliver = new ServiceKeepAliver(serviceName);
			var configuration = ApplicationConfiguration.Read<UlearnConfiguration>();
			var keepAliveInterval = TimeSpan.FromSeconds(configuration.KeepAliveInterval ?? 30);
			while (!shutdownEvent.WaitOne(0))
			{
				var unhandledSubmissions = new List<RunnerSubmission>();
				try
				{
					unhandledSubmissions.AddRange(client.TryGetSubmission(supportedSandboxes).Result);
				}
				catch (Exception e)
				{
					log.Error(e, $"Не могу получить решения из ulearn. Следующая попытка через {sleep.TotalSeconds} секунд");
					Thread.Sleep(sleep);
					continue;
				}

				log.Info($"Получил {unhandledSubmissions.Count} решение(й) со следующими ID: [{string.Join(", ", unhandledSubmissions.Select(s => s.Id))}]");

				foreach (var submission in unhandledSubmissions)
				{
					RunningResults result;
					try
					{
						result = SandboxRunnerClient.Run(submission);
						log.Info($"Результат проверки: [{result}]");
					}
					catch (Exception ex)
					{
						result = new RunningResults(submission.Id, Verdict.SandboxError, error: ex.ToString());
						log.Error(ex);
					}

					try
					{
						client.SendResults(result);
					}
					catch (Exception e)
					{
						log.Error(e, "Не могу отправить результаты проверки на ulearn");
					}
				}

				serviceKeepAliver.Ping(keepAliveInterval);
				Thread.Sleep(sleep);
			}
		}
	}
}