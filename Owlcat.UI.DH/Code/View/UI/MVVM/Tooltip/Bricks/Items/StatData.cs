using Code.View.UI.UIUtils;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using UnityEngine;

namespace Code.View.UI.MVVM.Tooltip.Bricks.Items;

public class StatData
{
	public enum StatHighlight
	{
		Default,
		Positive,
		Negative
	}

	public readonly string Value;

	public readonly string Label;

	public readonly Sprite Icon;

	public readonly ComparisonResult Comparison;

	public readonly StatHighlight Highlight;

	public StatData(string value, string label, Sprite icon, ComparisonResult comparison, StatHighlight highlight)
	{
		Value = value;
		Label = label;
		Icon = icon;
		Comparison = comparison;
		Highlight = highlight;
	}

	public StatData(string value, TooltipElement element, Sprite icon, ComparisonResult comparison, StatHighlight highlight)
		: this(value, UIUtilityTooltip.GetTooltipElementLabel(element), icon, comparison, highlight)
	{
	}
}
