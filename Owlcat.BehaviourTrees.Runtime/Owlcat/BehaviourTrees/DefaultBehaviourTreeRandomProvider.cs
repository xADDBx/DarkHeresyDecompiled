using UnityEngine;

namespace Owlcat.BehaviourTrees;

public class DefaultBehaviourTreeRandomProvider : IBehaviourTreeRandomProvider
{
	public float value => Random.value;

	public int Range(int minInclusive, int maxExclusive)
	{
		return Random.Range(minInclusive, maxExclusive);
	}
}
