using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaFullScreenPageImagePCView : EncyclopediaFullScreenPageImageBaseView
{
	protected override void OnBind()
	{
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			Close();
		}).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(base.Close).AddTo(this);
		base.OnBind();
	}
}
