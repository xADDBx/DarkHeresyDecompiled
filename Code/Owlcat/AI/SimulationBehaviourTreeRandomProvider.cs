using Kingmaker.Utility.Random;
using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public sealed class SimulationBehaviourTreeRandomProvider : IBehaviourTreeRandomProvider
{
	public float value => PFStatefulRandom.AI.value;

	public int Range(int minInclusive, int maxExclusive)
	{
		return PFStatefulRandom.AI.Range(minInclusive, maxExclusive);
	}
}
