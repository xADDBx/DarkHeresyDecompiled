using System;
using System.Linq;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[TypeId("48bc013150074ae8b49ce84959124bd3")]
public class ContextActionPerformAttack : ContextAction
{
	public bool UseCurrentWeapon;

	[ShowIf("UseCurrentWeapon")]
	public bool OnlyMeleeWeapon;

	public bool PerformActionsOnDamagePortion;

	public bool PerformActionsOnHit;

	public bool PerformActionsOnKill;

	[ShowIf("PerformActionsOnDamagePortion")]
	public int PercentOfMaxDamageNeededForActions;

	[ShowIf("PerformActionsOnDamagePortion")]
	public ActionList ActionsOnDamagePortion;

	[ShowIf("PerformActionsOnKill")]
	public ActionList ActionsOnKill;

	[ShowIf("PerformActionsOnHit")]
	public ActionList ActionsOnHit;

	protected override void RunAction()
	{
		MechanicEntity caster = base.Context.Caster;
		MechanicEntity entity = base.Target.Entity;
		if (caster == null || entity == null)
		{
			return;
		}
		AbilityData abilityData = base.Context.SourceAbility;
		ItemEntityWeapon itemEntityWeapon = (UseCurrentWeapon ? caster.GetFirstWeapon() : abilityData?.Weapon);
		ItemEntityWeapon itemEntityWeapon2 = (UseCurrentWeapon ? caster.GetSecondWeapon() : abilityData?.Weapon);
		ItemEntityWeapon itemEntityWeapon3 = ((itemEntityWeapon != null && (!OnlyMeleeWeapon || itemEntityWeapon.Blueprint.IsMelee)) ? itemEntityWeapon : itemEntityWeapon2);
		if (UseCurrentWeapon)
		{
			BlueprintAbility blueprintAbility = itemEntityWeapon3?.Blueprint.WeaponAbilities.FirstOrDefault()?.Ability;
			if (blueprintAbility == null)
			{
				return;
			}
			abilityData = new AbilityData(blueprintAbility, caster)
			{
				IsCharge = (abilityData?.IsCharge ?? false),
				OverrideWeapon = itemEntityWeapon3
			};
		}
		if (abilityData == null || itemEntityWeapon3 == null || (OnlyMeleeWeapon && !itemEntityWeapon3.Blueprint.IsMelee))
		{
			return;
		}
		RulePerformAttack rulePerformAttack = new RulePerformAttack(caster, entity, abilityData, 0);
		AbilityExecutionContext abilityContext = base.AbilityContext;
		if (abilityContext != null)
		{
			rulePerformAttack.RollPerformAttackRule.DangerArea.UnionWith(abilityContext.Pattern.Nodes);
		}
		Rulebook.Trigger(rulePerformAttack);
		if (PerformActionsOnHit && rulePerformAttack.ResultIsHit)
		{
			using (base.Context.PushTarget(entity))
			{
				ActionsOnHit.Run();
			}
		}
		RuleDealDamage resultDamageRule = rulePerformAttack.ResultDamageRule;
		if (resultDamageRule == null || resultDamageRule.ResultValue * 100 < itemEntityWeapon3.DamageMax * PercentOfMaxDamageNeededForActions)
		{
			return;
		}
		if (PerformActionsOnDamagePortion)
		{
			using (base.Context.PushTarget(entity))
			{
				ActionsOnDamagePortion.Run();
			}
		}
		if (!PerformActionsOnKill)
		{
			return;
		}
		PartHealth targetHealth = resultDamageRule.TargetHealth;
		if (targetHealth == null || targetHealth.HitPointsLeft > 0 || resultDamageRule.HPBeforeDamage <= 0)
		{
			return;
		}
		using (base.Context.PushTarget(entity))
		{
			ActionsOnKill.Run();
		}
	}

	public override string GetCaption()
	{
		if (!UseCurrentWeapon)
		{
			return "Perform an attack with the weapon that gave this ability";
		}
		return "Perform an attack with the current weapon";
	}
}
