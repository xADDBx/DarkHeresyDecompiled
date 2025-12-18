using System;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[TypeId("1c8015cd817446dfafe7f9b1421512e6")]
[ClassInfoBox("Модифицирует шанс атаки, защиты и скилл чеков")]
public abstract class RollDifficultyModifier : MechanicEntityFactComponentDelegate
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ModifierDescriptor Descriptor;

	public ContextValueModifierWithType Modifier = new ContextValueModifierWithType
	{
		Enabled = true
	};

	protected void TryApply(RulebookEvent rule)
	{
		if (!Restrictions.IsPassed(base.Context, null, null, rule))
		{
			return;
		}
		if (!(rule is RuleCalculateHitChances ruleCalculateHitChances))
		{
			if (!(rule is RuleCalculateDefence ruleCalculateDefence))
			{
				if (!(rule is RulePerformSkillCheck rulePerformSkillCheck))
				{
					throw new ArgumentOutOfRangeException();
				}
				Modifier.TryApply(rulePerformSkillCheck.DifficultyModifiers, base.Fact, Descriptor);
			}
			else
			{
				Modifier.TryApply(ruleCalculateDefence.DefenceValueModifiers, base.Fact, Descriptor);
			}
		}
		else
		{
			Modifier.TryApply(ruleCalculateHitChances.Modifiers, base.Fact, Descriptor);
		}
	}
}
