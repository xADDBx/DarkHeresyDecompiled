using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ConvictionBarVM : CharInfoComponentVM
{
	private const float EdgeValue = 1000f;

	private readonly ReactiveProperty<float> m_CurrentRelativeValue = new ReactiveProperty<float>();

	public readonly TooltipBaseTemplate RadicalTooltip;

	public readonly TooltipBaseTemplate PuritanTooltip;

	public readonly TooltipBaseTemplate CurrentTooltip;

	public ReadOnlyReactiveProperty<float> CurrentRelativeValue => m_CurrentRelativeValue;

	public ConvictionBarVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
		UITextAlignment alignment = UIStrings.Instance.Alignment;
		RadicalTooltip = new TooltipTemplateSimple(alignment.RadicalTitle, alignment.RadicalDescription);
		PuritanTooltip = new TooltipTemplateSimple(alignment.PuritanTitle, alignment.PuritanDescription);
		CurrentTooltip = new TooltipTemplateSimple(alignment.CurrentConvictionTitle, alignment.CurrentConvictionDescription);
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		AlignmentTooltipExtensions.GetAlignmentInfo(AlignmentAxis.Torian, Unit.CurrentValue, out var rankThresholds, out var maxValue, out var currentValue, out var currentTier);
		AlignmentTooltipExtensions.GetAlignmentInfo(AlignmentAxis.Monodominance, Unit.CurrentValue, out rankThresholds, out currentTier, out var currentValue2, out maxValue);
		AlignmentTooltipExtensions.GetAlignmentInfo(AlignmentAxis.Xanthite, Unit.CurrentValue, out rankThresholds, out maxValue, out var currentValue3, out currentTier);
		AlignmentTooltipExtensions.GetAlignmentInfo(AlignmentAxis.Xenophilia, Unit.CurrentValue, out rankThresholds, out currentTier, out var currentValue4, out maxValue);
		float value = (float)(currentValue2 + currentValue - currentValue4 - currentValue3) / 1000f;
		value = Mathf.Clamp(value, -1f, 1f);
		m_CurrentRelativeValue.Value = value;
	}
}
