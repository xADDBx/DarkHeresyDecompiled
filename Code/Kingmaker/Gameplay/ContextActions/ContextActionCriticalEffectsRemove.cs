using System;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Gameplay.Utility;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.ContextActions;

[Serializable]
[TypeId("4b884357f3fa446e9541f3e9273e69b9")]
public class ContextActionCriticalEffectsRemove : ContextAction
{
	public ContextValue Amount;

	public BodyPartsSelector BodyParts;

	public override string GetCaption()
	{
		return $"Remove {Amount} critical effects from {BodyParts.GetDescription()}";
	}

	protected override void RunAction()
	{
		PartHealth partHealth = base.Target.Entity?.GetOptional<PartHealth>();
		if (partHealth == null)
		{
			return;
		}
		int amount = Amount.Calculate(base.Context);
		foreach (BlueprintBodyPart bodyPart in BodyParts.GetBodyParts(partHealth.Owner))
		{
			partHealth.RemoveCriticalEffectStages(bodyPart, amount, base.Caster);
		}
	}
}
