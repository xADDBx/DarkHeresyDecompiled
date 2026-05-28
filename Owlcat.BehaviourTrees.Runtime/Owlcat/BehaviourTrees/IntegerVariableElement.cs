using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Integer", typeof(int))]
[TypeId("a9446837cb6d46c5b99fd7e2eba5f7f4")]
public class IntegerVariableElement : BehaviourTreeVariableElement<int>
{
	public override BlackboardVariable CreateVariable()
	{
		return new IntegerVariable
		{
			Value = Value
		};
	}
}
