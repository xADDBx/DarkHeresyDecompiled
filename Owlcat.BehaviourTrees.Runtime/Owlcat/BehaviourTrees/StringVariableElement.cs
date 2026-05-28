using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("String", typeof(string))]
[TypeId("622afce4f48e4ca9a4d95079920d5840")]
public class StringVariableElement : BehaviourTreeVariableElement<string>
{
	public override BlackboardVariable CreateVariable()
	{
		return new StringVariable
		{
			Value = Value
		};
	}
}
