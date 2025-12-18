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

public class TooltipTemplateDamageReduction : TooltipBaseTemplate
{
	private readonly ModifiableValue m_DamageReduction;

	private readonly BlueprintEncyclopediaGlossaryEntry m_DamageReductionGlossaryEntry;

	private readonly StatModifiersBreakdownData m_DamageReductionValueModifiersData;

	public TooltipTemplateDamageReduction(ModifiableValue damageReduction)
	{
		m_DamageReduction = damageReduction;
		m_DamageReductionGlossaryEntry = UIUtilityEncyclopedy.GetGlossaryEntry("DamageReduction");
		if (m_DamageReduction != null)
		{
			StatModifiersBreakdown.AddModifiersManager(m_DamageReduction.Modifiers);
			m_DamageReductionValueModifiersData = StatModifiersBreakdown.Build();
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(m_DamageReductionGlossaryEntry?.Title);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		int value = m_DamageReduction?.BaseValue ?? 0;
		list.Add(new TooltipBrickSeparator());
		list.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Tooltips.BaseValue, UIUtilityText.AddPercentTo(value)));
		AddDamageReductionModifiers(list);
		list.Add(new TooltipBrickText(UIStrings.Instance.Inspect.UnconditionalModifiers, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Left));
		list.Add(new TooltipBrickText(m_DamageReductionGlossaryEntry?.GetDescription()));
		return list;
	}

	private void AddDamageReductionModifiers(List<ITooltipBrick> bricks)
	{
		if (m_DamageReductionValueModifiersData.HasBonuses)
		{
			bricks.Add(new TooltipBricksGroupStart());
			AddDamageReductionModifiers(bricks, m_DamageReductionValueModifiersData);
			bricks.Add(new TooltipBricksGroupEnd());
		}
	}

	private void AddDamageReductionModifiers(List<ITooltipBrick> bricks, StatModifiersBreakdownData breakdownData)
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
