using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickPicture : ITooltipBrick
{
	private readonly Sprite m_Picture;

	private readonly Color m_PictureColor;

	public TooltipBrickPicture(Sprite picture, Color pictureColor = default(Color))
	{
		m_Picture = picture;
		m_PictureColor = ((pictureColor == default(Color)) ? Color.white : pictureColor);
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickPictureVM(m_Picture, m_PictureColor);
	}
}
