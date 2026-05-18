using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickPictureVM : TooltipBrickVM
{
	public readonly Sprite Picture;

	public readonly Color PictureColor;

	public BrickPictureVM(Sprite picture, Color pictureColor = default(Color))
	{
		Picture = picture;
		PictureColor = ((pictureColor == default(Color)) ? Color.white : pictureColor);
	}
}
