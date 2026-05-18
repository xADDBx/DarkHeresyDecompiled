using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Code.AreaLogic;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Designers.WarhammerSurfaceCombatPrototype;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Framework.ContextContract;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;
using Owlcat.Fmw.Blueprints;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules;

[RuleRoles(Initiator = "warp-channeling unit", Target = "warp-channeling unit (self)")]
public class RulePerformPsychicPhenomena : RulebookEvent
{
	public readonly CompositeModifiersManager ChanceModifiers = new CompositeModifiersManager(0, 100);

	public readonly CompositeModifiersManager PerilsChanceModifiers = new CompositeModifiersManager(0, 100);

	public AbilityExecutionContext AbilityContext { get; }

	[CanBeNull]
	public RuleRollD100 ResultChanceRoll { get; private set; }

	[CanBeNull]
	public RuleRollD100 ResultPerilsChanceRoll { get; private set; }

	[CanBeNull]
	public BlueprintPsykerRoot.PhenomenaData ResultPhenomena { get; private set; }

	public int ResultChance => ChanceModifiers.Value;

	public int ResultPerilsChance => PerilsChanceModifiers.Value;

	public bool ResultPhenomenaIsAppear
	{
		get
		{
			if (ResultChanceRoll != null)
			{
				return (int)ResultChanceRoll <= ResultChance;
			}
			return false;
		}
	}

	public bool ResultIsPerils
	{
		get
		{
			if (ResultPerilsChanceRoll != null)
			{
				return (int)ResultPerilsChanceRoll < ResultPerilsChance;
			}
			return false;
		}
	}

	public RulePerformPsychicPhenomena([NotNull] MechanicEntity initiator, [NotNull] AbilityExecutionContext abilityContext)
		: base(initiator)
	{
		AbilityContext = abilityContext;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		PartVeil veil = Game.Instance.LoadedArea.Veil;
		if (base.Self.GetOptional<PartPsyker>() == null)
		{
			return;
		}
		ChanceModifiers.Add(ModifierType.ValAdd, veil.PsychicPhenomenaChance, this, ModifierDescriptor.BaseValue);
		PerilsChanceModifiers.Add(ModifierType.ValAdd, veil.PerilsOfTheWarpChance, this, ModifierDescriptor.BaseValue);
		ResultChanceRoll = RulebookEvent.RollD100();
		if (ResultPhenomenaIsAppear)
		{
			ResultPerilsChanceRoll = RulebookEvent.RollD100();
			if (ResultIsPerils)
			{
				veil.PhenomenaStreak = 0;
			}
			else
			{
				veil.PhenomenaStreak++;
			}
			List<BlueprintPsykerRoot.PhenomenaData> list = (ResultIsPerils ? veil.GetResolvedPerils() : veil.GetResolvedPhenomena());
			if (list.Count > 0)
			{
				ResultPhenomena = PhenomenaListResolver.SelectWeighted(list, PFStatefulRandom.RuleSystem);
			}
			RunPsychicPhenomenaEffectOnTarget(base.ConcreteInitiator, EvalContext.Current, ResultPhenomena, ResultIsPerils);
			EventBus.RaiseEvent(delegate(IPsychicPerilHandler h)
			{
				h.HandlePsychicPeril(this);
			});
		}
	}

	public static void RunPsychicPhenomenaEffectOnTarget(MechanicEntity target, IEvalContext context, BlueprintPsykerRoot.PhenomenaData phenomenaData, bool isPerils)
	{
		if (target == null || phenomenaData == null)
		{
			return;
		}
		BpRef<BlueprintAbility> ability = phenomenaData.Ability;
		if (ability != null)
		{
			RulePerformAbility rulePerformAbility = new RulePerformAbility(new AbilityData(ability, target), target);
			rulePerformAbility.IgnoreCooldown = true;
			rulePerformAbility.ForceFreeAction = true;
			rulePerformAbility.Context.ExecutionFromPsychicPhenomena = true;
			Rulebook.Trigger(rulePerformAbility);
			rulePerformAbility.Context.RewindActionIndex();
		}
		GameObject gameObject = phenomenaData.OptionalMinorFX.Load();
		if ((object)gameObject != null)
		{
			FxHelper.SpawnFxOnEntity(gameObject, target.View);
		}
		Bark bark = phenomenaData.Bark;
		if (bark == null)
		{
			return;
		}
		UnitAsksManager asks = target.View.Asks;
		if (asks != null)
		{
			phenomenaData.Bark.Chance = 1f;
			phenomenaData.Bark.ShowOnScreen = true;
			new AskWrapper(phenomenaData.Bark, asks).Schedule();
			return;
		}
		AskEntry askEntry = bark.Entries.FirstItem();
		if (askEntry != null)
		{
			LocalizedString text = askEntry.Text;
			if (text != null)
			{
				target.TryGetVoGuid(out var voGuid);
				BarkPlayer.Bark(target, text, VoiceOverType.Bark, voGuid);
			}
		}
	}
}
