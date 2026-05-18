using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;

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

	public static StatTooltipData FromActor(MechanicEntity entity, StatType stat)
	{
		StatQueryOutput statQueryOutput = new StatQueryOutput();
		StatResult stat2 = entity.Actor.GetStat(stat, statQueryOutput, default(StatContext), "FromActor");
		StatTooltipData statTooltipData = new StatTooltipData();
		statTooltipData.Type = stat;
		statTooltipData.KeyWord = UtilityStats.GetGlossaryName(stat);
		statTooltipData.BaseValue = stat2.BaseValue;
		statTooltipData.ModifiedValue = stat2.ModifiedValue;
		if (stat.IsAttribute())
		{
			statTooltipData.TotalValueLabel = UIStrings.Instance.Tooltips.TotalAttributeValue;
			statTooltipData.Group = StatGroup.Attribute;
			statTooltipData.BonusValue = stat2.ModifiedValue / 10;
			statTooltipData.AttributeModifier = stat2.ModifiedValue / 10;
		}
		else if (stat.IsSkill())
		{
			statTooltipData.TotalValueLabel = UIStrings.Instance.Tooltips.TotalSkillValue;
			statTooltipData.Group = StatGroup.Skill;
		}
		else
		{
			statTooltipData.TotalValueLabel = UIStrings.Instance.Tooltips.TotalValue;
			statTooltipData.Group = StatGroup.Common;
		}
		StatModifiersBreakdown.AddStatModifiers(statQueryOutput.Modifiers);
		statTooltipData.Breakdown = StatModifiersBreakdown.Build();
		return statTooltipData;
	}

	private StatTooltipData()
	{
	}
}
