using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class RespecWindowPCView : RespecWindowCommonView
{
	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	[SerializeField]
	private OwlcatButton m_AcceptButton;

	protected override void OnBind()
	{
		base.OnBind();
		EscHotkeyManager.Instance.Subscribe(base.CloseWindow).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			CloseWindow();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_AcceptButton.OnLeftClickAsObservable(), delegate
		{
			OnConfirm();
		}).AddTo(this);
		base.ViewModel.CanRespec.Subscribe(m_AcceptButton.SetInteractable).AddTo(this);
	}
}
