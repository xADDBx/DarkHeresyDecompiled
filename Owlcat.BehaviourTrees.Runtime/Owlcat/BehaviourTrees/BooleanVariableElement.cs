using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Boolean", typeof(bool))]
[TypeId("ccb67a4a623a4a2893520942002943dd")]
public class BooleanVariableElement : BehaviourTreeVariableElement<bool>
{
	public override BlackboardVariable CreateVariable()
	{
		return new BooleanVariable
		{
			Value = Value
		};
	}
}
