using System.Collections.Generic;
using Kingmaker.Code.Gameplay.Blueprints;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Runtime/BodyPartList", typeof(List<BpRef<BlueprintBodyPart>>))]
[TypeId("e53f6290cddb47ee83ebac3f9e271303")]
public class BodyPartListVariableElement : BehaviourTreeVariableElement<List<BpRef<BlueprintBodyPart>>>
{
	public override BlackboardVariable CreateVariable()
	{
		return new BodyPartListVariable();
	}
}
