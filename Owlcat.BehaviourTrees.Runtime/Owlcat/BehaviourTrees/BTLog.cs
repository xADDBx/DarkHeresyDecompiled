using System;

namespace Owlcat.BehaviourTrees;

public static class BTLog
{
	private static IBehaviourTreeLogger s_Logger = new DefaultBehaviourTreeLogger();

	public static void Log(string message)
	{
		s_Logger?.Log(message);
	}

	public static void Error(string error)
	{
		s_Logger?.Error(error);
	}

	public static void Error(Exception exception)
	{
		s_Logger?.Error(exception);
	}

	public static void SetLogger(IBehaviourTreeLogger logger)
	{
		s_Logger = logger;
	}
}
