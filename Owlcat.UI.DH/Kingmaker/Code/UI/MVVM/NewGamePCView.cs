using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NewGamePCView : NewGameBaseView
{
	[Header("Views")]
	[SerializeField]
	private NewGamePhasePresetView m_NewGamePhasePresetView;

	[SerializeField]
	private NewGamePhaseStoryPCView m_NewGamePhaseStoryPCView;

	[SerializeField]
	private NewGamePhaseDifficultyPCView m_NewGamePhaseDifficultyPCView;

	[Header("Buttons")]
	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	[SerializeField]
	private OwlcatMultiButton m_BackButton;

	[SerializeField]
	private OwlcatMultiButton m_NextButton;

	[Header("Texts")]
	[SerializeField]
	private TextMeshProUGUI m_BackButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_NextButtonLabel;

	public void Awake()
	{
		base.gameObject.SetActive(value: false);
		m_NewGamePhaseDifficultyPCView.Initialize();
		m_Selector.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.OnClose).AddTo(this);
		SetButtonsSounds();
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnClose();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_BackButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnButtonBack();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_NextButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnButtonNext();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnConfirmClickAsObservable(), delegate
		{
			base.ViewModel.OnClose();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_BackButton.OnConfirmClickAsObservable(), delegate
		{
			base.ViewModel.OnButtonBack();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_NextButton.OnConfirmClickAsObservable(), delegate
		{
			base.ViewModel.OnButtonNext();
		}).AddTo(this);
		base.ViewModel.SelectedMenuEntity.Subscribe(OnPhaseChanged);
		m_BackButtonLabel.text = UIStrings.Instance.ContextMenu.Back;
		m_NextButtonLabel.text = UIStrings.Instance.MainMenu.Continue;
	}

	private void OnPhaseChanged(NewGameMenuEntityVM vm)
	{
		if (vm.NewGamePhaseVM == base.ViewModel.DifficultyVM)
		{
			m_NewGamePhasePresetView.Unbind();
			m_NewGamePhaseStoryPCView.Unbind();
			m_NewGamePhaseDifficultyPCView.Bind(base.ViewModel.DifficultyVM);
		}
	}

	private void SetButtonsSounds()
	{
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_BackButton, ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_NextButton, ButtonSoundsEnum.PlastickSound);
	}
}
