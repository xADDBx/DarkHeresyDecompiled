using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public interface IParameterizedBehaviourTreeProvider : IBehaviourTreeProvider
{
	ParameterizedBehaviourTree ParameterizedBehaviourTree { get; }
}
