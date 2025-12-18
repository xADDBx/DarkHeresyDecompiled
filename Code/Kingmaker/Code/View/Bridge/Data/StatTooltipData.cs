using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.Code.View.Bridge.Data;

public class StatTooltipData
{
	public int BaseValue;

	public int? BonusValue;

	public int ModifiedValue;

	public StatGroup Group;

	public StatType? Type;

	public string KeyWord;

	public int AttributeModifier;

	public StatModifiersBreakdownData Breakdown;

	public string TotalValueLabel;

	public StatTooltipData(ModifiableValue stat)
	{
		TotalValueLabel = UIStrings.Instance.Tooltips.TotalValue;
		BaseValue = stat.BaseValue;
		ModifiedValue = stat.ModifiedValue;
		Group = StatGroup.Common;
		Type = stat.Type;
		KeyWord = UtilityStats.GetGlossaryName(stat.Type);
		StatModifiersBreakdown.AddStatModifiers(stat);
		Breakdown = StatModifiersBreakdown.Build();
	}

	public StatTooltipData(ModifiableValueSkill skill)
	{
		TotalValueLabel = UIStrings.Instance.Tooltips.TotalSkillValue;
		BaseValue = skill.BaseValue;
		ModifiedValue = skill.ModifiedValue;
		Group = StatGroup.Skill;
		Type = skill.Type;
		KeyWord = skill.Type.ToString();
		StatModifiersBreakdown.AddStatModifiers(skill);
		Breakdown = StatModifiersBreakdown.Build();
	}

	public StatTooltipData(ModifiableValueAttributeStat attribute)
	{
		TotalValueLabel = UIStrings.Instance.Tooltips.TotalAttributeValue;
		BonusValue = attribute.Bonus;
		if (attribute.Enabled)
		{
			BaseValue = attribute.BaseValue;
			ModifiedValue = attribute.ModifiedValue;
			AttributeModifier = attribute.WarhammerBonus;
		}
		else
		{
			BaseValue = 0;
			ModifiedValue = 0;
			AttributeModifier = 0;
		}
		Group = StatGroup.Attribute;
		Type = attribute.Type;
		KeyWord = attribute.Type.ToString();
		StatModifiersBreakdown.AddStatModifiers(attribute);
		Breakdown = StatModifiersBreakdown.Build();
	}
}
