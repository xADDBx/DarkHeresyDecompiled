using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickImageVM : TooltipBaseBrickVM
{
	public readonly Sprite Sprite;

	public readonly Vector2Int Size;

	public TooltipBrickImageVM(Sprite sprite, Vector2Int size)
	{
		Sprite = sprite;
		Size = size;
	}
}
