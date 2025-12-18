using System;
using Kingmaker;
using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public class BehaviourTreeLogger : IBehaviourTreeLogger
{
	public void Log(string message)
	{
		PFLog.AI.Log(message);
	}

	public void Error(string error)
	{
		PFLog.AI.Error(error);
	}

	public void Error(Exception exception)
	{
		PFLog.AI.Exception(exception);
	}
}
