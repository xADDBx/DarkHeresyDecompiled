using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class TooltipBrickImageView : TooltipBaseBrickView<TooltipBrickImageVM>
{
	[SerializeField]
	protected Image m_Image;

	[SerializeField]
	protected LayoutElement m_LayoutElement;

	protected override void OnBind()
	{
		base.OnBind();
		m_Image.sprite = base.ViewModel.Sprite;
		m_LayoutElement.preferredWidth = ((base.ViewModel.Size == default(Vector2Int)) ? (-1) : base.ViewModel.Size.x);
		m_LayoutElement.preferredHeight = ((base.ViewModel.Size == default(Vector2Int)) ? (-1) : base.ViewModel.Size.y);
	}
}
