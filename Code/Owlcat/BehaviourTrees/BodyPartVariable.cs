using JetBrains.Annotations;
using Kingmaker.Code.Gameplay.Blueprints;
using Owlcat.Fmw.Blueprints;

namespace Owlcat.BehaviourTrees;

public class BodyPartVariable : BlackboardVariable<BpRef<BlueprintBodyPart>>
{
	[CanBeNull]
	public override BpRef<BlueprintBodyPart> Value { get; set; }

	public override string ToString()
	{
		return base.Key + ": " + (Value?.ToString() ?? "<null>");
	}
}
