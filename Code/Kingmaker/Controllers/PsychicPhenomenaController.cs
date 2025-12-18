using Core.Cheats;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.WarhammerSurfaceCombatPrototype;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.GlobalEffectSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Controllers;

public class PsychicPhenomenaController : IControllerEnable, IController, IControllerDisable, ITurnBasedModeHandler, ISubscriber, IRoundStartHandler
{
	public void TryTriggerPsychicPhenomenaBeforeCast(AbilityExecutionContext context)
	{
		BlueprintAbilityWrapper blueprint = context.Ability.Blueprint;
		MechanicEntity caster = context.Caster;
		if (blueprint.IsPsykerAbility && caster.GetPsykerOptional() != null)
		{
			Game.Instance.LoadedArea.Veil.UpdateDamage(caster, UpdateVeilEventType.BeforeAbilityCast, context.Ability);
		}
	}

	public void TryTriggerPsychicPhenomenaAfterCast(AbilityExecutionContext context)
	{
		if (context.Ability.Blueprint.IsPsykerAbility && context.MaybeCaster?.GetPsykerOptional() != null)
		{
			Rulebook.Trigger(new RulePerformPsychicPhenomena(context.Caster, context));
			Game.Instance.LoadedArea.Veil.UpdateDamage(context.Caster, UpdateVeilEventType.AfterAbilityCast, context.Ability);
		}
	}

	void IControllerEnable.OnEnable()
	{
		GlobalEffectDirector.Shared.SetWeightFromCode(ConfigRoot.Instance.PsykerRoot.VeilDamageGlobalEffect, delegate
		{
			int maxVeilDamage = ConfigRoot.Instance.PsykerRoot.MaxVeilDamage;
			return (float)Game.Instance.LoadedArea.Veil.Damage / (float)maxVeilDamage;
		});
		EventBus.RaiseEvent(delegate(IVeilDamageHandler h)
		{
			h.HandleVeilDamageChanged(0, Game.Instance.LoadedArea.Veil.Damage);
		});
	}

	void IControllerDisable.OnDisable()
	{
		GlobalEffectDirector.Shared.RemoveWeightFromCode(ConfigRoot.Instance.PsykerRoot.VeilDamageGlobalEffect);
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			Game.Instance.LoadedArea.Veil.UpdateDamage(Game.Instance.LoadedArea, UpdateVeilEventType.CombatEnd);
		}
	}

	void IRoundStartHandler.HandleRoundStart(bool isTurnBased)
	{
		if (isTurnBased)
		{
			Game.Instance.LoadedArea.Veil.UpdateDamage(Game.Instance.LoadedArea, UpdateVeilEventType.CombatRoundStart);
		}
	}

	public static void TriggerPsychicPhenomenaForced(MechanicEntity target, MechanicsContext context, BlueprintPsykerRoot.PhenomenaData phenomena, bool isPerils)
	{
		RulePerformPsychicPhenomena.RunPsychicPhenomenaEffectOnTarget(target, context, phenomena, isPerils);
		EventBus.RaiseEvent(delegate(IForcedPsychicPhenomenaHandler h)
		{
			h.HandleForcedPsychicPhenomena(isPerils);
		});
	}

	[Cheat(Name = "veil_heal", Description = "Heal veil by specified value (default = 1)")]
	public static void HealVeil(int value = 1)
	{
		Game.Instance.LoadedArea.Veil.UpdateDamage(Game.Instance.LoadedArea, UpdateVeilEventType.Custom, null, -value);
	}

	[Cheat(Name = "veil_damage", Description = "Damage veil by specified value (default = 1)")]
	public static void DamageVeil(int value = 1)
	{
		Game.Instance.LoadedArea.Veil.UpdateDamage(Game.Instance.LoadedArea, UpdateVeilEventType.Custom, null, value);
	}

	[Cheat(Name = "veil_heal_all", Description = "Heal all veil damage")]
	public static void HealVeilAll()
	{
		Game.Instance.LoadedArea.Veil.UpdateDamage(Game.Instance.LoadedArea, UpdateVeilEventType.Custom, null, -Game.Instance.LoadedArea.Veil.Damage);
	}
}
