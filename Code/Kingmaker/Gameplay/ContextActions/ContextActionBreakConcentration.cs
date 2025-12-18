using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Gameplay.Features.Concentration;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.ContextActions;

[Serializable]
[KDB("Сбивает концентрацию с цели")]
[TypeId("69711474968d4367ace25519d4275b2e")]
public class ContextActionBreakConcentration : ContextAction
{
	public bool BreakEvenSteadyConcentration;

	public override string GetCaption()
	{
		return "Break concentration on target";
	}

	protected override void RunAction()
	{
		if (base.Target.Entity == null || (!BreakEvenSteadyConcentration && (bool)base.Target.Entity.Features.SteadyConcentration))
		{
			return;
		}
		AbilityExecutionContext abilityContext = base.AbilityContext;
		if (abilityContext != null && abilityContext.Ability.IsPrecise)
		{
			EventBus.RaiseEvent((IMechanicEntity)base.Target.Entity, (Action<IBodyPartHitAdditionalEffect>)delegate(IBodyPartHitAdditionalEffect h)
			{
				h.HandleBodyPartHitBreakConcentration(base.AbilityContext?.Ability.PreciseBodyPart, base.Target.Entity?.GetOptional<PartConcentration>()?.Buff);
			}, isCheckRuntime: true);
		}
		base.Target.Entity?.GetOptional<PartConcentration>()?.Break(base.Caster);
	}
}
