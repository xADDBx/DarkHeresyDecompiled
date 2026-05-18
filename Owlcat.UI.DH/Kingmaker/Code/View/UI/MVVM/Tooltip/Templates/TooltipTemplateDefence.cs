using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.RuleSystem.Rules.Utility;
using Owlcat.UI;

namespace Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;

public class TooltipTemplateDefence : TooltipBaseTemplate
{
	private readonly int m_BaseValue;

	private readonly int m_MaxDefence;

	private readonly BlueprintEncyclopediaGlossaryEntry m_DefenceGlossaryEntry;

	private readonly StatModifiersBreakdownData m_DefenceValueModifiersData;

	public TooltipTemplateDefence(MechanicEntity entity, StatType stat)
	{
		m_DefenceGlossaryEntry = UIUtilityEncyclopedy.GetGlossaryEntry("Defence");
		if (entity != null)
		{
			StatQueryOutput statQueryOutput = new StatQueryOutput();
			m_BaseValue = entity.Actor.GetStat(stat, statQueryOutput, default(StatContext), ".ctor").BaseValue;
			m_MaxDefence = entity.GetMaxDefenceCap();
			StatModifiersBreakdown.AddCompositeModifiersManager(statQueryOutput.Modifiers);
			m_DefenceValueModifiersData = StatModifiersBreakdown.Build();
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new BrickTitleVM(m_DefenceGlossaryEntry?.Title);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		list.Add(new BrickSeparatorVM());
		list.Add(new BrickIconStatValueVM(new TextValueAddElement(UIStrings.Instance.Tooltips.BaseValue, UIUtilityText.AddPercentTo(m_BaseValue))));
		AddDefenceModifiers(list);
		if (m_MaxDefence > 0)
		{
			list.Add(new BrickSeparatorVM());
			list.Add(new BrickIconStatValueVM(new TextValueAddElement(UIStrings.Instance.Tooltips.MaxDefence, UIUtilityText.AddPercentTo(m_MaxDefence))));
		}
		list.Add(new BrickTextVM(UIStrings.Instance.Inspect.UnconditionalModifiers, TooltipTextType.Simple, TooltipTextAlignment.Left));
		list.Add(new BrickTextVM(m_DefenceGlossaryEntry?.GetDescription()));
		return list;
	}

	private void AddDefenceModifiers(List<ITooltipBrick> bricks)
	{
		if (m_DefenceValueModifiersData.HasBonuses)
		{
			List<TooltipBrickVM> list = new List<TooltipBrickVM>();
			AddDefenceModifiers(list, m_DefenceValueModifiersData);
			bricks.Add(new BricksGroupOneColumnVM(list));
		}
	}

	private void AddDefenceModifiers(List<TooltipBrickVM> bricks, StatModifiersBreakdownData breakdownData)
	{
		foreach (StatBonusEntry sortedBonuse in breakdownData.SortedBonuses)
		{
			string text = string.Empty;
			if (sortedBonuse.Descriptor != 0)
			{
				text = ConfigRoot.Instance.LocalizedTexts.Descriptors.GetText(sortedBonuse.Descriptor);
			}
			else if (!string.IsNullOrWhiteSpace(sortedBonuse.Source))
			{
				text = sortedBonuse.Source;
			}
			if (sortedBonuse.Descriptor != 0 && !string.IsNullOrWhiteSpace(sortedBonuse.Source))
			{
				text = text + " [" + sortedBonuse.Source + "]";
			}
			BrickElementPalette type = ((sortedBonuse.Bonus >= 0) ? BrickElementPalette.Positive : BrickElementPalette.Negative);
			string value = UIUtilityText.AddPercentTo(UIUtilityText.AddSign(sortedBonuse.Bonus));
			bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(text, value), null, type));
		}
	}
}
