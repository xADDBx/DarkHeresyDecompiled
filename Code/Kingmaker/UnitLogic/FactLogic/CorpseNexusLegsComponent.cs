using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("f37fc64d5fcc4985a78d679875d4d238")]
public class CorpseNexusLegsComponent : UnitFactComponentDelegate, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, ITargetRulebookSubscriber
{
	public bool Master;

	[HideIf("Master")]
	public CorpseNexusLegType LegType;

	[HideIf("Master")]
	public ActionList ActionsOnKiller;

	[SerializeField]
	[HideIf("Master")]
	private BlueprintBuffReference m_PretendDeadBuff;

	public BlueprintBuff PretendDeadBuff => m_PretendDeadBuff?.Get();

	protected override void OnActivate()
	{
		if (!base.IsReapplying)
		{
			MechanicEntity maybeCaster = base.Fact.Context.MaybeCaster;
			if (!Master)
			{
				maybeCaster?.GetOrCreate<UnitPartCorpseNexusLegs>().NewLeg(base.Owner, LegType);
			}
			base.Owner.GetOrCreate<UnitPartCorpseNexusLegs>();
		}
	}

	protected override void OnDeactivate()
	{
		if (!base.IsReapplying)
		{
			MechanicEntity maybeCaster = base.Fact.Context.MaybeCaster;
			if (!Master)
			{
				base.Owner.Buffs.Remove(PretendDeadBuff);
				maybeCaster?.GetOptional<UnitPartCorpseNexusLegs>()?.RemoveLeg(base.Owner);
			}
		}
	}

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		if (evt.HPBeforeDamage <= 0)
		{
			return;
		}
		MechanicEntity maybeCaster = base.Fact.Context.MaybeCaster;
		UnitPartCorpseNexusLegs unitPartCorpseNexusLegs = maybeCaster?.GetOptional<UnitPartCorpseNexusLegs>();
		BaseUnitEntity baseUnitEntity = unitPartCorpseNexusLegs?.Legs.FirstOrDefault((CorpseNexusLegData p) => p.LegType == CorpseNexusLegType.Herald)?.Unit;
		PartLifeState partLifeState = baseUnitEntity?.GetLifeStateOptional();
		if (Master && partLifeState != null && unitPartCorpseNexusLegs.Legs.Any((CorpseNexusLegData p) => p.LegType == CorpseNexusLegType.Herald && !p.PretendDead))
		{
			Rulebook.Trigger(new RuleDealDamage(evt.Initiator, baseUnitEntity, evt.ResultDamage.CopyAsIntermediateDamage()));
			PartArmor armorOptional = base.Owner.GetArmorOptional();
			DamageStrategy strategy = ((armorOptional != null && armorOptional.DurabilityLeft > 0) ? DamageStrategy.ArmorOnly : DamageStrategy.HealthOnly);
			int num = evt.HPBeforeDamage - base.Owner.Health.HitPointsLeft;
			int num2 = Math.Max(num - evt.ResultValue, 0);
			int num3 = ((num > 0) ? num : evt.ResultValue);
			Rulebook.Trigger(RuleHealDamage.Setup(maybeCaster, maybeCaster).WithMinMax(num3, num3).Base(0)
				.Strategy(strategy)
				.Create());
			if (num2 > 0)
			{
				Rulebook.Trigger(RuleHealDamage.Setup(maybeCaster, maybeCaster).WithMinMax(num2, num2).Base(0)
					.Strategy(DamageStrategy.ArmorOnly)
					.Create());
			}
		}
		PartHealth targetHealth = evt.TargetHealth;
		bool flag = targetHealth != null && targetHealth.HitPointsLeft <= 1;
		if (!Master && unitPartCorpseNexusLegs != null && flag)
		{
			CorpseNexusLegData corpseNexusLegData = unitPartCorpseNexusLegs.Legs.FirstOrDefault((CorpseNexusLegData p) => p.Unit == base.Owner);
			if (corpseNexusLegData != null)
			{
				corpseNexusLegData.PretendDead = true;
				base.Owner.Buffs.Add(PretendDeadBuff, base.Context, null);
				base.Fact.RunActionInContext(ActionsOnKiller, evt.ConcreteInitiator);
			}
		}
		if (!(Master && unitPartCorpseNexusLegs != null && flag))
		{
			return;
		}
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		foreach (CorpseNexusLegData leg in unitPartCorpseNexusLegs.Legs)
		{
			list.Add(leg.Unit);
		}
		foreach (BaseUnitEntity item in list)
		{
			item.Buffs.Remove(PretendDeadBuff);
			item.GetLifeStateOptional().MarkedForDeath = true;
		}
	}
}
