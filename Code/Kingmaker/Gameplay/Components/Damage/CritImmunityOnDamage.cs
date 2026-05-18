using System;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Framework.Mechanics.Utility.Damage;
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
[TypeId("398d4e38f8b140b093f1b29661080232")]
public abstract class CritImmunityOnDamage : MechanicEntityFactComponentDelegate
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	[EnumFlagsAsButtons(ColumnCount = 3)]
	public DamageTrigger.FilterFlags Filter = DamageTrigger.FilterFlags.Normal;

	public BoolPropertyChecker.Mode ArmorDamage;

	[ShowIf("ShowArmorCrack")]
	public BoolPropertyChecker.Mode ArmorCrack;

	public BoolPropertyChecker.Mode IsVital;

	private bool ShowArmorCrack => ArmorDamage == BoolPropertyChecker.Mode.True;

	protected void TryApplyImmunity(RulePerformCriticalEffects rule)
	{
		if (IsSuitable(rule) && Restrictions.IsPassed(base.Context, null, null, rule))
		{
			rule.Immunity.Add(base.Runtime);
		}
	}

	private bool IsSuitable(RulePerformCriticalEffects rule)
	{
		RolledDamage damage = rule.Damage;
		if (damage == null)
		{
			return false;
		}
		bool flag = rule.Reason.Fact?.Blueprint is BlueprintBuff blueprintBuff && blueprintBuff.AbilityGroups.Contains(ConfigRoot.Instance.CombatRoot.DamageOverTimeAbilityGroup);
		if ((Filter & DamageTrigger.FilterFlags.DOT) == 0 && flag)
		{
			return false;
		}
		bool flag2 = damage.Type == DamageType.Direct;
		if ((Filter & DamageTrigger.FilterFlags.Direct) == 0 && flag2)
		{
			return false;
		}
		if ((Filter & DamageTrigger.FilterFlags.Normal) == 0 && !flag && !flag2)
		{
			return false;
		}
		if (ArmorDamage.Check(damage.ResultDamageToArmorValue > 0) && IsVital.Check(damage.IsVitalDamage))
		{
			return ArmorCrack.Check(damage.ResultIsArmorCrack);
		}
		return false;
	}
}
