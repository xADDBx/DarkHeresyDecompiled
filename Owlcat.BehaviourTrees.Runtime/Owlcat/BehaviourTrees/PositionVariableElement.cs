using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Position", typeof(Vector3))]
[TypeId("13df1aad176e4c1f88097c714492ebb3")]
public class PositionVariableElement : BehaviourTreeVariableElement<Vector3>
{
	public override BlackboardVariable CreateVariable()
	{
		return new PositionVariable
		{
			Value = Value
		};
	}
}
