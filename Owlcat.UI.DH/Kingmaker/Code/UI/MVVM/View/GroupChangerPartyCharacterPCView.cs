using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.View;

public class GroupChangerPartyCharacterPCView : GroupChangerCharacterBaseView
{
	protected override void OnBind()
	{
		base.OnBind();
		ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnClick();
		}).AddTo(this);
	}

	protected override void SetState(bool isInParty, bool isLock)
	{
		base.gameObject.SetActive(isInParty || isLock);
	}
}
