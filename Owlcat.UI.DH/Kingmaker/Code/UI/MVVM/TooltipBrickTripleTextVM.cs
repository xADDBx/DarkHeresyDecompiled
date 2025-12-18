using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickTripleTextVM : TooltipBrickDoubleTextVM
{
	public readonly string MiddleLine;

	public readonly Sprite LeftIcon;

	public readonly Sprite MiddleIcon;

	public readonly Sprite RightIcon;

	public readonly TextFieldParams LeftParams;

	public readonly TextFieldParams MiddleParams;

	public readonly TextFieldParams RightParams;

	public TooltipBrickTripleTextVM(string leftLine, string middleLine, string rightLine, Sprite leftIcon = null, Sprite middleIcon = null, Sprite rightIcon = null, TextFieldParams leftLineParams = null, TextFieldParams middleLineParams = null, TextFieldParams rightLineParams = null)
		: base(leftLine, rightLine)
	{
		MiddleLine = middleLine;
		LeftIcon = leftIcon;
		MiddleIcon = middleIcon;
		RightIcon = rightIcon;
		LeftParams = leftLineParams;
		MiddleParams = middleLineParams;
		RightParams = rightLineParams;
	}
}
