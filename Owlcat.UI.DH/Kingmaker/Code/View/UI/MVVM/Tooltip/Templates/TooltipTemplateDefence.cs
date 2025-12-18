using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Settings;
using Owlcat.UI;

namespace Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;

public class TooltipTemplateDefence : TooltipBaseTemplate
{
	private readonly ModifiableValue m_Defence;

	private readonly BlueprintEncyclopediaGlossaryEntry m_DefenceGlossaryEntry;

	private readonly StatModifiersBreakdownData m_DefenceValueModifiersData;

	public TooltipTemplateDefence(ModifiableValue defence)
	{
		m_Defence = defence;
		m_DefenceGlossaryEntry = UIUtilityEncyclopedy.GetGlossaryEntry("Defence");
		if (m_Defence != null)
		{
			StatModifiersBreakdown.AddModifiersManager(m_Defence.Modifiers);
			m_DefenceValueModifiersData = StatModifiersBreakdown.Build();
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(m_DefenceGlossaryEntry?.Title);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		int value = m_Defence?.BaseValue ?? 0;
		list.Add(new TooltipBrickSeparator());
		list.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Tooltips.BaseValue, UIUtilityText.AddPercentTo(value)));
		AddDefenceModifiers(list);
		list.Add(new TooltipBrickText(UIStrings.Instance.Inspect.UnconditionalModifiers, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Left));
		list.Add(new TooltipBrickText(m_DefenceGlossaryEntry?.GetDescription()));
		return list;
	}

	private void AddDefenceModifiers(List<ITooltipBrick> bricks)
	{
		if (m_DefenceValueModifiersData.HasBonuses)
		{
			bricks.Add(new TooltipBricksGroupStart());
			AddDefenceModifiers(bricks, m_DefenceValueModifiersData);
			bricks.Add(new TooltipBricksGroupEnd());
		}
	}

	private void AddDefenceModifiers(List<ITooltipBrick> bricks, StatModifiersBreakdownData breakdownData)
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
			if (sortedBonuse.Descriptor == ModifierDescriptor.Difficulty && SettingsHelper.CalculateCRModifier() < 1f)
			{
				text = text + " (" + UIStrings.Instance.Tooltips.DifficultyReduceDescription.Text + ")";
			}
			TooltipBrickIconStatValueType type = ((sortedBonuse.Bonus >= 0) ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
			string value = UIUtilityText.AddPercentTo(UIUtilityText.AddSign(sortedBonuse.Bonus));
			bricks.Add(new TooltipBrickIconStatValue(text, value, null, null, type));
		}
	}
}
