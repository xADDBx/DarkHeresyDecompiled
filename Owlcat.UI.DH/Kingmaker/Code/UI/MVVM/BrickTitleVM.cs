using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickTitleVM : TooltipBrickVM
{
	public readonly TextEntity Title;

	public readonly TooltipTitleType Type;

	public readonly TextAnchor TextAnchor;

	public BrickTitleVM(TextEntity title, TooltipTitleType type, TextAnchor textAnchor = TextAnchor.MiddleCenter)
	{
		Title = title;
		Type = type;
		TextAnchor = textAnchor;
	}

	public BrickTitleVM(string title, TooltipTitleType type, TextAnchor textAnchor = TextAnchor.MiddleCenter)
		: this(new TextEntity(title, TextFieldParams.Center), type, textAnchor)
	{
	}

	public BrickTitleVM(string title)
		: this(title, TooltipTitleType.H1)
	{
	}
}
