using System;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

public class DefaultBehaviourTreeLogger : IBehaviourTreeLogger
{
	public void Log(string message)
	{
		Debug.Log(message);
	}

	public void Error(string error)
	{
		Debug.LogError(error);
	}

	public void Error(Exception exception)
	{
		Debug.LogException(exception);
	}
}
