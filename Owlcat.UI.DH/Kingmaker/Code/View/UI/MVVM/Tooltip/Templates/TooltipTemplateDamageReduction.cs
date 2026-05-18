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
using Owlcat.UI;

namespace Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;

public class TooltipTemplateDamageReduction : TooltipBaseTemplate
{
	private readonly int m_BaseValue;

	private readonly BlueprintEncyclopediaGlossaryEntry m_DamageReductionGlossaryEntry;

	private readonly StatModifiersBreakdownData m_DamageReductionValueModifiersData;

	public TooltipTemplateDamageReduction(MechanicEntity entity, StatType stat)
	{
		m_DamageReductionGlossaryEntry = UIUtilityEncyclopedy.GetGlossaryEntry("DamageReduction");
		if (entity != null)
		{
			StatQueryOutput statQueryOutput = new StatQueryOutput();
			m_BaseValue = entity.Actor.GetStat(stat, statQueryOutput, default(StatContext), ".ctor").BaseValue;
			StatModifiersBreakdown.AddCompositeModifiersManager(statQueryOutput.Modifiers);
			m_DamageReductionValueModifiersData = StatModifiersBreakdown.Build();
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new BrickTitleVM(m_DamageReductionGlossaryEntry?.Title);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		list.Add(new BrickSeparatorVM());
		list.Add(new BrickIconStatValueVM(new TextValueAddElement(UIStrings.Instance.Tooltips.BaseValue, UIUtilityText.AddPercentTo(m_BaseValue))));
		AddDamageReductionModifiers(list);
		list.Add(new BrickTextVM(UIStrings.Instance.Inspect.UnconditionalModifiers, TooltipTextType.Simple, TooltipTextAlignment.Left));
		list.Add(new BrickTextVM(m_DamageReductionGlossaryEntry?.GetDescription()));
		return list;
	}

	private void AddDamageReductionModifiers(List<ITooltipBrick> bricks)
	{
		if (m_DamageReductionValueModifiersData.HasBonuses)
		{
			List<TooltipBrickVM> list = new List<TooltipBrickVM>();
			AddDamageReductionModifiers(list, m_DamageReductionValueModifiersData);
			bricks.Add(new BricksGroupOneColumnVM(list));
		}
	}

	private void AddDamageReductionModifiers(List<TooltipBrickVM> bricks, StatModifiersBreakdownData breakdownData)
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
