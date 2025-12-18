using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaPageImagePCView : EncyclopediaPageImageBaseView
{
	protected override void OnBind()
	{
		base.OnBind();
		m_ZoomButton.gameObject.SetActive(base.ViewModel.IsZoomAllowed);
		if (base.ViewModel.IsZoomAllowed)
		{
			ObservableSubscribeExtensions.Subscribe(m_ZoomButton.OnLeftClickAsObservable(), delegate
			{
				OnButtonClick();
			}).AddTo(this);
		}
	}
}
