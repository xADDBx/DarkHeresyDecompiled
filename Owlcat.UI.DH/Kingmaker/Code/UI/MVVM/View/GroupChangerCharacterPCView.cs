using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.View;

public class GroupChangerCharacterPCView : GroupChangerCharacterBaseView
{
	protected override void OnBind()
	{
		base.OnBind();
		ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnClick();
		}).AddTo(this);
	}
}
