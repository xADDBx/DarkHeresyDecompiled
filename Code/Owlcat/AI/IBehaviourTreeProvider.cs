using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public interface IBehaviourTreeProvider
{
	BehaviourTreeSerializableData BehaviourTree { get; }
}
