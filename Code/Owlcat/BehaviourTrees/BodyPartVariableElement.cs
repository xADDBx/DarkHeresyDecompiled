using Kingmaker.Code.Gameplay.Blueprints;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("BodyPart", typeof(BpRef<BlueprintBodyPart>))]
[TypeId("3bc4ba84dccf4074b34d89d20e8fc230")]
public class BodyPartVariableElement : BehaviourTreeVariableElement<BpRef<BlueprintBodyPart>>
{
	public override BlackboardVariable CreateVariable()
	{
		return new BodyPartVariable
		{
			Value = Value
		};
	}
}
