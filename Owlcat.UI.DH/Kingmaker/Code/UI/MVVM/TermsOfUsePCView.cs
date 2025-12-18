using JetBrains.Annotations;
using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TermsOfUsePCView : TermsOfUseBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_OkLabel;

	[SerializeField]
	protected OwlcatMultiButton m_ButtonOk;

	[SerializeField]
	protected OwlcatMultiButton m_ButtonClose;

	protected override void OnBind()
	{
		base.OnBind();
		m_OkLabel.text = base.ViewModel.TermsOfUseTexts.AcceptBtn;
		EscHotkeyManager.Instance.Subscribe(delegate
		{
			base.ViewModel.TryCloseTermOfUse();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ButtonOk.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.TermsOfUseAccept();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ButtonClose.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.TryCloseTermOfUse();
		}).AddTo(this);
	}
}
