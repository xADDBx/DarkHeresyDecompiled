using System;

namespace Owlcat.BehaviourTrees;

public interface IBehaviourTreeLogger
{
	void Log(string message);

	void Error(string error);

	void Error(Exception exception);
}
