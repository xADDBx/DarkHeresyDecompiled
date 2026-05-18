using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

[ViewFactoryPolicy(ViewFactoryPolicyFlag.DontReparent, null)]
public class EscMenuPCView : EscMenuBaseView
{
	[Header("Common")]
	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	protected override void OnBind()
	{
		base.OnBind();
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, ButtonSoundsEnum.PlastickSound);
		m_CloseButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.OnClose).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.OnClose).AddTo(this);
	}
}
