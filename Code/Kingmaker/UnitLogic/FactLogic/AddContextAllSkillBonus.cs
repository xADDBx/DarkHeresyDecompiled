using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete("WH2-11514")]
[ComponentName("Add bonus to all skills")]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("b18305c0363e40d4a7f0f5743a583bc5")]
public class AddContextAllSkillBonus : UnitFactComponentDelegate
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ModifierDescriptor Descriptor;

	public int Multiplier = 1;

	public ContextValue Value;

	protected override void OnActivateOrPostLoad()
	{
		if (Restrictions.IsPassed(base.Context))
		{
			StatType[] skills = StatTypeHelper.Skills;
			foreach (StatType statType in skills)
			{
				int value = CalculateValue(base.Context, statType);
				base.Owner.Stats.GetStatOptional(statType)?.AddModifier(value, base.Runtime, Descriptor);
			}
		}
	}

	protected override void OnDeactivate()
	{
		StatType[] skills = StatTypeHelper.Skills;
		foreach (StatType type in skills)
		{
			base.Owner.Stats.GetStatOptional(type)?.RemoveModifiersFrom(base.Runtime);
		}
	}

	public int CalculateValue(MechanicsContext context, StatType stat)
	{
		return Value.Calculate(context) * Multiplier;
	}
}
