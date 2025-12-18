using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[ComponentName("Veil/VeilDamageModifier")]
[TypeId("a95d94ff07694b198fdc26830615b28f")]
public class VeilDamageModifier : MechanicEntityFactComponentDelegate, IGlobalRulebookHandler<RuleCalculateVeilDamage>, IRulebookHandler<RuleCalculateVeilDamage>, ISubscriber, IGlobalRulebookSubscriber
{
	[Flags]
	public enum EventType
	{
		CombatRoundStart = 1,
		BeforeAbilityCast = 2,
		AfterAbilityCast = 4
	}

	[EnumFlagsAsDropdown]
	public EventType Event;

	public RestrictionCalculator Restrictions;

	public ContextValueModifierWithType Value = new ContextValueModifierWithType
	{
		Enabled = true
	};

	void IRulebookHandler<RuleCalculateVeilDamage>.OnEventAboutToTrigger(RuleCalculateVeilDamage evt)
	{
		if (IsSuitable(evt))
		{
			Value.TryApply(evt.Modifiers, base.Fact, ModifierDescriptor.None);
		}
	}

	void IRulebookHandler<RuleCalculateVeilDamage>.OnEventDidTrigger(RuleCalculateVeilDamage evt)
	{
	}

	private bool IsSuitable(RuleCalculateVeilDamage evt)
	{
		EventType @event = Event;
		if (@event == EventType.BeforeAbilityCast || @event == EventType.AfterAbilityCast)
		{
			AbilityData maybeAbility = evt.MaybeAbility;
			if ((object)maybeAbility != null)
			{
				BlueprintAbilityWrapper blueprint = maybeAbility.Blueprint;
				if (blueprint != null && !blueprint.IsPsykerAbility)
				{
					return false;
				}
			}
		}
		if (!Restrictions.IsPassed(base.Context, null, null, evt))
		{
			return false;
		}
		if (((Event & EventType.CombatRoundStart) == 0 || evt.Event != UpdateVeilEventType.CombatRoundStart) && ((Event & EventType.BeforeAbilityCast) == 0 || evt.Event != UpdateVeilEventType.BeforeAbilityCast))
		{
			if ((Event & EventType.AfterAbilityCast) != 0)
			{
				return evt.Event == UpdateVeilEventType.AfterAbilityCast;
			}
			return false;
		}
		return true;
	}
}
