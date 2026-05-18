using System;
using UnityEngine;

namespace Owlcat.UI;

public static class UIKitLogger
{
	private static ILogger s_Logger;

	private static ILogger Logger => s_Logger ?? Debug.unityLogger;

	public static void SetLogger(ILogger logger)
	{
		s_Logger = logger;
	}

	public static void Log(object message)
	{
		Logger.Log(LogType.Log, message);
	}

	public static void Warning(object message)
	{
		Logger.Log(LogType.Warning, message);
	}

	public static void Error(object message)
	{
		Logger.Log(LogType.Error, message);
	}

	public static void Exception(object message)
	{
		Logger.Log(LogType.Exception, message);
	}

	public static void Exception(string message, Exception exception)
	{
		Logger.Log(LogType.Exception, message);
		Logger.LogException(exception);
	}
}
