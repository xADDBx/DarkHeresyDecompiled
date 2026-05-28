namespace Owlcat.BehaviourTrees;

public interface IBehaviourTreeRandomProvider
{
	float value { get; }

	int Range(int minInclusive, int maxExclusive);
}
