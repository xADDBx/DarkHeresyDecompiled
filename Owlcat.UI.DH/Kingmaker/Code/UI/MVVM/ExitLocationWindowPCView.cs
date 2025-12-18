using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ExitLocationWindowPCView : ExitLocationWindowBaseView
{
	[SerializeField]
	protected OwlcatButton m_AcceptButton;

	[SerializeField]
	protected OwlcatButton m_DeclineButton;

	protected override void OnBind()
	{
		base.OnBind();
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.Decline).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_AcceptButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Confirm();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_DeclineButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Decline();
		}).AddTo(this);
	}
}
