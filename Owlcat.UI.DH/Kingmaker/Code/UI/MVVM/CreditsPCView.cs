using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CreditsPCView : CreditsBaseView
{
	[Header("PC Input")]
	[SerializeField]
	protected OwlcatMultiButton m_CloseButton;

	private InputLayer m_InputLayer;

	protected override void OnBind()
	{
		m_CloseButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.CloseCredits).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(delegate
		{
			base.ViewModel.CloseCredits();
		}).AddTo(this);
		base.OnBind();
		m_PlayMultiButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.TogglePause).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ButtonLeft.OnLeftClickAsObservable(), delegate
		{
			OnPrevPage();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ButtonRight.OnLeftClickAsObservable(), delegate
		{
			OnNextPage();
		}).AddTo(this);
		m_SearchButton.OnLeftClickAsObservable().Subscribe(base.OnFind).AddTo(this);
		CreateInput();
		UIKeybindGeneralSettings uIKeybindGeneralSettings = UISettingsRoot.Instance.UIKeybindGeneralSettings;
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.PrevTab.name, delegate
		{
			ChangeTab(direction: false);
		}).AddTo(this);
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.NextTab.name, delegate
		{
			ChangeTab(direction: true);
		}).AddTo(this);
	}

	private void CreateInput()
	{
		m_InputLayer = new InputLayer
		{
			ContextName = "CreditsView"
		};
		m_InputLayer.AddButton(delegate
		{
			OnFind();
		}, 8);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}
}
