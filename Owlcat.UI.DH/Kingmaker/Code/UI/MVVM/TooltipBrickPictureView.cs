using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickPictureView : TooltipBaseBrickView<TooltipBrickPictureVM>
{
	[SerializeField]
	private Image m_Image;

	protected override void OnBind()
	{
		m_Image.sprite = base.ViewModel.Picture;
		m_Image.color = base.ViewModel.PictureColor;
	}
}
