using Owlcat.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaPageImageBaseView : View<EncyclopediaPageImageVM>
{
	[SerializeField]
	private Image m_Image;

	[SerializeField]
	protected OwlcatButton m_ZoomButton;

	protected override void OnBind()
	{
		m_Image.sprite = base.ViewModel.Image;
	}

	public void OnButtonClick()
	{
		base.ViewModel.HandleZoomClick();
	}
}
