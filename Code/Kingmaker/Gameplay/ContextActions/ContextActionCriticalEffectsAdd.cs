using System;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Utility;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.ContextActions;

[Serializable]
[TypeId("c226f1d46f95482ab030440238c4c10f")]
public class ContextActionCriticalEffectsAdd : ContextAction
{
	public ContextValue Amount;

	public BodyPartsSelector BodyParts;

	public bool ResistanceCheck;

	public override string GetCaption()
	{
		return string.Format("Add {0} critical effects to {1} {2}", Amount, BodyParts.GetDescription(), ResistanceCheck ? "(with resistance check)" : "");
	}

	protected override void RunAction()
	{
		MechanicEntity caster = base.Caster;
		MechanicEntity entity = base.Target.Entity;
		if (entity?.GetOptional<PartHealth>() == null)
		{
			return;
		}
		int amount = Amount.Calculate(base.Context);
		foreach (BlueprintBodyPart bodyPart in BodyParts.GetBodyParts(entity))
		{
			Rulebook.Trigger(new RulePerformCriticalEffects(caster, entity, bodyPart, amount)
			{
				DisableResistanceCheck = !ResistanceCheck
			});
		}
	}
}
