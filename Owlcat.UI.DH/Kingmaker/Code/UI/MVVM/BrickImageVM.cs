using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickImageVM : TooltipBrickVM
{
	public readonly Sprite Sprite;

	public BrickImageVM(Sprite sprite)
	{
		Sprite = sprite;
	}
}
