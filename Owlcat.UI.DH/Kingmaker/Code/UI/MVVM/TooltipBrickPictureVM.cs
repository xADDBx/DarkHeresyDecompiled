using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickPictureVM : TooltipBaseBrickVM
{
	public readonly Sprite Picture;

	public readonly Color PictureColor;

	public TooltipBrickPictureVM(Sprite picture, Color pictureColor)
	{
		Picture = picture;
		PictureColor = pictureColor;
	}
}
