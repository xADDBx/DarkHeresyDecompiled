using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Logging;
using Sentry;
using Sentry.Unity;
using UnityEngine;

namespace Kingmaker.QA.Sentry;

public static class SentryService
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("SentryService");

	private static IStringBuilderPool _pool;

	private static BlockingCollection<LogInfo> _logQueue;

	private static int _addToLogQueueTimeoutMs;

	private static CancellationTokenSource _cts;

	private static Thread _workerThread;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Start()
	{
		if (!IsEnabled())
		{
			Logger.Log("SentryService: Sentry service is disabled");
			SentryUnity.Close();
			return;
		}
		SentryServiceOptions sentryServiceOptions = Resources.Load<SentryServiceOptions>("Sentry/SentryServiceOptions");
		if ((object)sentryServiceOptions == null)
		{
			Logger.Error("SentryService: Failed to load SentryServiceOptions");
			return;
		}
		Logger.Log("SentryService: Sentry service started");
		_pool = StringBuilderPool.Create(sentryServiceOptions.stringBuilderPoolCapacity);
		_logQueue = new BlockingCollection<LogInfo>(sentryServiceOptions.logQueueCapacity);
		_addToLogQueueTimeoutMs = sentryServiceOptions.addToLogQueueTimeoutMs;
		_cts = new CancellationTokenSource();
		_workerThread = new Thread(ProcessLogs)
		{
			IsBackground = true
		};
		_workerThread.Start();
		Owlcat.Runtime.Core.Logging.Logger.Instance.OnLogProcess += OnLogProcess;
		Application.quitting += Stop;
	}

	private static void Stop()
	{
		Application.quitting -= Stop;
		Owlcat.Runtime.Core.Logging.Logger.Instance.OnLogProcess -= OnLogProcess;
		_cts.Cancel();
		_cts.Dispose();
		_logQueue.Dispose();
		_workerThread.Join();
	}

	private static void OnLogProcess(ref LogInfo logInfo)
	{
		if (logInfo.Severity == LogSeverity.Error && logInfo.Channel != Logger && !_cts.IsCancellationRequested && !_logQueue.TryAdd(logInfo, _addToLogQueueTimeoutMs))
		{
			Logger.Error($"SentryService: Error logs processing queue overflow! ATTENTION: THIS IS BLOCKING THE GAME THREAD for {_addToLogQueueTimeoutMs} ms. Please increase the SentryServiceOption.LogQueueCapacity parameter.");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsEnabled()
	{
		if (BuildModeUtility.IsDevelopment)
		{
			return SentrySdk.IsEnabled;
		}
		return false;
	}

	private static void ProcessLogs()
	{
		try
		{
			foreach (LogInfo item in _logQueue.GetConsumingEnumerable(_cts.Token))
			{
				ProcessLog(item);
			}
		}
		catch (OperationCanceledException)
		{
		}
		catch (ObjectDisposedException)
		{
		}
	}

	private static void ProcessLog(LogInfo logInfo)
	{
		try
		{
			SentrySdk.CaptureMessage(ConstructMessage(ref logInfo), SentryLevel.Error);
		}
		catch (Exception ex)
		{
			Logger.Error(ex, "SentryService: Failed to process log");
		}
	}

	private static string ConstructMessage(ref LogInfo logInfo)
	{
		if (logInfo.Callstack == null || logInfo.Callstack.Count == 0)
		{
			return logInfo.Message;
		}
		using PooledStringBuilder pooledStringBuilder = _pool.Rent();
		StringBuilder builder = pooledStringBuilder.Builder;
		if (!string.IsNullOrEmpty(logInfo.Message))
		{
			builder.AppendLine(logInfo.Message);
		}
		foreach (LogStackFrame item in logInfo.Callstack)
		{
			builder.AppendLine(item.GetFormattedMethodName());
		}
		return builder.ToString();
	}
}
