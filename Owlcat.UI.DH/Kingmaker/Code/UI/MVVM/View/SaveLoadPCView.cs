using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

[ViewFactoryPolicy(ViewFactoryPolicyFlag.DontReparent, null)]
public class SaveLoadPCView : SaveLoadBaseView
{
	[Header("PC")]
	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	protected override void OnBind()
	{
		base.OnBind();
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, ButtonSoundsEnum.PlastickSound);
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnClose();
		}).AddTo(this);
		UIKeybindGeneralSettings uIKeybindGeneralSettings = UISettingsRoot.Instance.UIKeybindGeneralSettings;
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.PrevTab.name, base.SelectPrev).AddTo(this);
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.NextTab.name, base.SelectNext).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.OnClose).AddTo(this);
	}
}
