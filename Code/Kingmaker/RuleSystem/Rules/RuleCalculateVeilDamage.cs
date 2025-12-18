using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.AreaLogic;
using Kingmaker.Designers.WarhammerSurfaceCombatPrototype;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateVeilDamage : RulebookEvent
{
	public readonly CompositeModifiersManager Modifiers = new CompositeModifiersManager();

	public readonly ValueModifiersManager MinVeilDamageModifiers = new ValueModifiersManager();

	public AbilityData Ability { get; }

	public UpdateVeilEventType Event { get; }

	public int CustomDamageDelta { get; }

	public int ResultDamage { get; private set; }

	public int ResultDamageDelta { get; private set; }

	public int ResultMinDamage => MinVeilDamageModifiers.Value;

	public override AbilityData MaybeAbility => Ability;

	public RuleCalculateVeilDamage([NotNull] MechanicEntity initiator, UpdateVeilEventType @event, [CanBeNull] AbilityData ability = null, int customDamageDelta = 0)
		: base(initiator)
	{
		MinVeilDamageModifiers.Add(Game.Instance.LoadedAreaState.Blueprint.StartVeilDamage, this, ModifierDescriptor.UntypedUnstackable);
		Event = @event;
		Ability = ability;
		CustomDamageDelta = customDamageDelta;
		UpdateVeilEventType event2 = Event;
		if ((event2 == UpdateVeilEventType.AfterAbilityCast || event2 == UpdateVeilEventType.BeforeAbilityCast) && Ability == null)
		{
			throw new ArgumentException(string.Format("{0} is required for event {1}", "ability", Event));
		}
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		PartVeil veil = Game.Instance.LoadedArea.Veil;
		BlueprintPsykerRoot psykerRoot = ConfigRoot.Instance.PsykerRoot;
		int resultMinDamage = ResultMinDamage;
		int damage = veil.Damage;
		int maxVeilDamage = psykerRoot.MaxVeilDamage;
		int value = Event switch
		{
			UpdateVeilEventType.Custom => CustomDamageDelta, 
			UpdateVeilEventType.CombatEnd => -damage, 
			UpdateVeilEventType.CombatRoundStart => -psykerRoot.EveryRoundVeilHealing, 
			UpdateVeilEventType.BeforeAbilityCast => GetAbilityVeilDelta(afterCast: false), 
			UpdateVeilEventType.AfterAbilityCast => GetAbilityVeilDelta(afterCast: true), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		ResultDamage = Math.Clamp(damage + Modifiers.Apply(value), resultMinDamage, maxVeilDamage);
		ResultDamageDelta = ResultDamage - damage;
	}

	private int GetAbilityVeilDelta(bool afterCast)
	{
		AbilityData ability = Ability;
		if ((object)ability != null)
		{
			BlueprintAbilityWrapper blueprint = ability.Blueprint;
			if (blueprint != null && blueprint.IsPsykerAbility)
			{
				BlueprintPsykerRoot psykerRoot = ConfigRoot.Instance.PsykerRoot;
				if (afterCast)
				{
					return Math.Max(1, psykerRoot.DefaultVeilDamageFromPower + blueprint.GetVeilDamage());
				}
				if ((bool)base.Initiator.Features.PsykerPush)
				{
					return psykerRoot.PushVeilDamage;
				}
				if ((bool)base.Initiator.Features.PsykerFetter)
				{
					return -psykerRoot.FetterVeilHealing;
				}
				return 0;
			}
		}
		return 0;
	}
}
