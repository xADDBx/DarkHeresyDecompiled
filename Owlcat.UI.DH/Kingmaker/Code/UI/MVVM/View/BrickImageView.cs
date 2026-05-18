using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class BrickImageView : BrickBaseView<BrickImageVM>
{
	[SerializeField]
	protected Image m_Image;

	protected override void OnBind()
	{
		base.OnBind();
		m_Image.sprite = base.ViewModel.Sprite;
	}
}
