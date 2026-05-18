using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenPCView : CharGenView
{
	[SerializeField]
	private OwlcatButton m_VisualSettingsViewButton;

	[SerializeField]
	private CharacterVisualSettingsPCView m_VisualSettingsPCView;

	[Header("Buttons")]
	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	[SerializeField]
	private OwlcatMultiButton m_NextButton;

	[SerializeField]
	private TextMeshProUGUI m_NextButtonLabel;

	[SerializeField]
	private OwlcatMultiButton m_BackButton;

	[SerializeField]
	private TextMeshProUGUI m_BackButtonLabel;

	[SerializeField]
	private OwlcatMultiButton m_ToFirstButton;

	[SerializeField]
	private OwlcatMultiButton m_ToLastButton;

	[SerializeField]
	private OwlcatMultiButton m_ToCharInfoButton;

	[SerializeField]
	private TextMeshProUGUI m_ToCharInfoLabel;

	private readonly ReactiveProperty<string> m_NextButtonHint = new ReactiveProperty<string>(string.Empty);

	private readonly CompositeDisposable m_CurrentPhaseDisposable = new CompositeDisposable();

	private bool m_IsShowed;

	protected override void OnBind()
	{
		base.OnBind();
		SetButtonsSounds();
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			CloseCharGen();
		}).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(CloseCharGen).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_NextButton.OnLeftClickAsObservable(), delegate
		{
			NextPressed();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_BackButton.OnLeftClickAsObservable(), delegate
		{
			BackPressed();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ToFirstButton.OnLeftClickAsObservable(), delegate
		{
			FirstPressed();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ToLastButton.OnLeftClickAsObservable(), delegate
		{
			LastPressed();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ToCharInfoButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ToCharInfo();
		}).AddTo(this);
		base.ViewModel.CurrentPhaseVM.Subscribe(delegate(CharGenPhaseBaseVM phase)
		{
			m_NextButtonLabel.text = (base.CurrentPhaseIsLast ? UIStrings.Instance.CharGen.Complete : UIStrings.Instance.CharGen.Next);
			m_CurrentPhaseDisposable.Clear();
			m_CurrentPhaseDisposable.Add(phase.PhaseNextHint.Subscribe(delegate(string value)
			{
				m_NextButtonHint.Value = value;
			}));
		}).AddTo(this);
		m_CanGoNext.Subscribe(SetActiveNextPhaseButton).AddTo(this);
		m_CanGoBack.Subscribe(SetActiveBackPhaseButton).AddTo(this);
		m_NextButton.SetHint(m_NextButtonHint).AddTo(this);
		m_NextButtonLabel.text = UIStrings.Instance.CharGen.Next;
		m_BackButtonLabel.text = UIStrings.Instance.CharGen.Back;
		m_ToCharInfoLabel.text = UIStrings.Instance.CharGen.ToCharacterInfo;
		CheckCoopButtons(base.ViewModel.IsMainCharacter.CurrentValue);
		base.ViewModel.CheckCoopControls.Subscribe(CheckCoopButtons).AddTo(this);
		Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.PrevTab.name, BackPressed).AddTo(this);
		Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.NextTab.name, NextPressed).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_CurrentPhaseDisposable.Dispose();
	}

	private void CheckCoopButtons(bool isMainCharacter)
	{
		m_CloseButton.gameObject.SetActive(isMainCharacter);
		m_NextButton.gameObject.SetActive(isMainCharacter);
		m_BackButton.gameObject.SetActive(isMainCharacter);
	}

	private void SetActiveNextPhaseButton(bool active)
	{
		m_NextButton.Interactable = active;
		m_ToLastButton.Interactable = active && !base.CurrentPhaseIsLast;
	}

	private void SetActiveBackPhaseButton(bool active)
	{
		m_BackButton.Interactable = active;
		m_ToFirstButton.Interactable = active;
	}

	private void SetButtonsSounds()
	{
		UISounds.Instance.SetClickAndHoverSound(m_NextButton, ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_BackButton, ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, ButtonSoundsEnum.PlastickSound);
	}
}
