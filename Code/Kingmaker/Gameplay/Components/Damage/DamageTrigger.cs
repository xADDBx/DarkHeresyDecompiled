using System;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Framework.Mechanics;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components.Damage;

[Serializable]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[AllowMultipleComponents]
[TypeId("c09907b2996642dfb931743d34a1dbff")]
public abstract class DamageTrigger : MechanicEntityFactComponentDelegate
{
	[Flags]
	public enum FilterFlags
	{
		Normal = 1,
		DOT = 2,
		Direct = 4
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	[EnumFlagsAsButtons(ColumnCount = 3)]
	public FilterFlags Filter = FilterFlags.Normal;

	[HideIf("TriggerBefore")]
	public BoolPropertyChecker.Mode ArmorDamage;

	[ShowIf("ShowArmorCrack")]
	public BoolPropertyChecker.Mode ArmorCrack;

	[HideIf("TriggerBefore")]
	public BoolPropertyChecker.Mode TargetDies;

	[HideIf("TriggerBefore")]
	public BoolPropertyChecker.Mode IsCritical;

	[HideIf("TriggerBefore")]
	public BoolPropertyChecker.Mode IsVital;

	public ActionList ActionOnTarget;

	public ActionList ActionOnCaster;

	public bool TriggerBefore;

	private bool ShowArmorCrack
	{
		get
		{
			if (ArmorDamage == BoolPropertyChecker.Mode.True)
			{
				return !TriggerBefore;
			}
			return false;
		}
	}

	protected void TryTrigger(RuleDealDamage rule, bool before)
	{
		if (TriggerBefore == before && IsSuitable(rule) && Restrictions.IsPassed(base.Context, null, null, rule))
		{
			ActionOnTarget.RunWithTarget(rule.Target);
			ActionOnCaster.RunWithTarget(rule.Initiator);
		}
	}

	private bool IsSuitable(RuleDealDamage rule)
	{
		if (rule.IsFake)
		{
			return false;
		}
		bool flag = rule.Reason.Fact?.Blueprint is BlueprintBuff blueprintBuff && blueprintBuff.AbilityGroups.Contains(ConfigRoot.Instance.CombatRoot.DamageOverTimeAbilityGroup);
		if ((Filter & FilterFlags.DOT) == 0 && flag)
		{
			return false;
		}
		bool flag2 = rule.IntermediateDamage.Type == DamageType.Direct;
		if ((Filter & FilterFlags.Direct) == 0 && flag2)
		{
			return false;
		}
		if ((Filter & FilterFlags.Normal) == 0 && !flag && !flag2)
		{
			return false;
		}
		if (!TriggerBefore)
		{
			if (ArmorDamage.Check(rule.ResultDamage.ResultDamageToArmorValue > 0) && IsVital.Check(rule.ResultDamage.IsVitalDamage) && ArmorCrack.Check(rule.ResultArmorCrack) && IsCritical.Check(rule.ResultIsCritical))
			{
				return TargetDies.Check(rule.ResultUnitDied);
			}
			return false;
		}
		return true;
	}
}
