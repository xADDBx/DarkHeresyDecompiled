using Kingmaker.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/PropertyCalculator/Set Calculator From Blueprint", "Set Calculator From Blueprint")]
[TypeId("636008c982eb56746878b39939ec00b2")]
public class SetCalculatorFromBlueprintNodeElement : BehaviourTreeNodeElement<SetCalculatorFromBlueprintNode>
{
	public PropertyCalculatorBlueprintVariableReference Variable;

	[ValidateNotNull]
	public BlueprintEntityPropertyReference Blueprint;

	protected override SetCalculatorFromBlueprintNode CreateTypedNode(Blackboard blackboard)
	{
		return new SetCalculatorFromBlueprintNode(Variable.GetRuntimeVariable(blackboard), Blueprint.Get());
	}
}
