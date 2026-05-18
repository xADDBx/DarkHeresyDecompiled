using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class StatData
{
	public enum StatHighlight
	{
		Default,
		Positive,
		Negative
	}

	public readonly TextValueElement Text;

	public readonly Sprite Icon;

	public readonly ComparisonResult Comparison;

	public readonly StatHighlight Highlight;

	public StatData(TextValueElement text, Sprite icon, ComparisonResult comparison, StatHighlight highlight)
	{
		Text = text;
		Icon = icon;
		Comparison = comparison;
		Highlight = highlight;
	}
}
