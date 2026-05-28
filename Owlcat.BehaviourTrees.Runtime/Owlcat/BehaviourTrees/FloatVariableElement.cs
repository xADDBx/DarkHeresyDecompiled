using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Float", typeof(float))]
[TypeId("15a41161dfaf434388122fa3d5c1023c")]
public class FloatVariableElement : BehaviourTreeVariableElement<float>
{
	public override BlackboardVariable CreateVariable()
	{
		return new FloatVariable
		{
			Value = Value
		};
	}
}
