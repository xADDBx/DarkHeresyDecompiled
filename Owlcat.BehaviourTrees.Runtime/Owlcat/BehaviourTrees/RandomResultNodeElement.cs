using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Debug/Random Result", "Random Result")]
[TypeId("4e3d9d03c860482db83dfcb07a3934ba")]
public class RandomResultNodeElement : ConditionNodeElement<RandomResultNode>
{
	[Range(0f, 1f)]
	public float Chance;

	protected override RandomResultNode CreateTypedNode(Blackboard blackboard)
	{
		return new RandomResultNode(AbortType, Chance);
	}
}
