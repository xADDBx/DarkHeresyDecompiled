using Kingmaker.EntitySystem.Properties;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Runtime/PropertyCalculatorBlueprint", typeof(PropertyCalculatorBlueprint))]
[TypeId("39e30574698371b45897690281c063d6")]
public class PropertyCalculatorBlueprintVariableElement : BehaviourTreeVariableElement<BpRef<PropertyCalculatorBlueprint>>
{
	public override BlackboardVariable CreateVariable()
	{
		return new PropertyCalculatorBlueprintVariable
		{
			Value = Value?.MaybeBlueprint
		};
	}

	public override string GetCaption()
	{
		return Value.Blueprint.Value.ToString();
	}
}
