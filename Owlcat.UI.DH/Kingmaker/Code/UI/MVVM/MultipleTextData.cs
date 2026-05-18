using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public readonly struct MultipleTextData
{
	public readonly TextEntity Text;

	public readonly Sprite Icon;

	public readonly TextAnchor Alignment;

	public MultipleTextData(TextEntity text, Sprite icon = null, TextAnchor alignment = TextAnchor.MiddleCenter)
	{
		Text = text;
		Icon = icon;
		Alignment = alignment;
	}
}
