using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Transitions;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[ViewFactoryPolicy(ViewFactoryPolicyFlag.DontPool, null)]
public class GamepadConnectedInKeyboardModeWindowView : View<GamepadConnectDisconnectVM>, ITransitable
{
	[SerializeField]
	private ConsoleHint m_ConfirmHint;

	[SerializeField]
	private ConsoleHint m_DeclineHint;

	[SerializeField]
	private WindowAnimator m_WindowAnimator;

	[SerializeField]
	public TextMeshProUGUI m_HeaderLabel;

	[SerializeField]
	public TextMeshProUGUI m_HintLabel;

	private UIAnimatorTransition m_ShowTransition;

	private UIAnimatorTransition m_HideTransition;

	private readonly ReactiveProperty<bool> m_CanDecline = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanConfirm = new ReactiveProperty<bool>();

	private void Awake()
	{
		m_WindowAnimator.Initialize();
		m_ShowTransition = new UIAnimatorShowTransition(m_WindowAnimator);
		m_HideTransition = new UIAnimatorHideTransition(m_WindowAnimator);
		CreateNavigation();
	}

	protected override void OnBind()
	{
		m_HeaderLabel.text = UIStrings.Instance.ControllerModeTexts.GamepadConnectedHeaderText;
		m_HintLabel.text = UIStrings.Instance.ControllerModeTexts.GamepadConnectedText;
		m_ConfirmHint.SetLabel(UIStrings.Instance.ControllerModeTexts.ConfirmSwitchText);
		m_DeclineHint.SetLabel(UIStrings.Instance.CommonTexts.Cancel);
		base.gameObject.SetActive(value: true);
		CreateNavigation();
		HandleCurrentState(value: true);
	}

	Transition ITransitable.Show()
	{
		return m_ShowTransition.Run();
	}

	Transition ITransitable.Hide()
	{
		return m_HideTransition.Run(delegate
		{
			base.gameObject.SetActive(value: false);
			HandleCurrentState(value: false);
		});
	}

	private void CreateNavigation()
	{
		InputLayer inputLayer = new InputLayer
		{
			ContextName = "GamepadConnectedInKeyboardModeWindowView"
		};
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			SwitchControlMode();
		}, 8, m_CanConfirm);
		m_ConfirmHint.Bind(inputBindStruct).AddTo(this);
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			Decline();
		}, 9, m_CanDecline);
		m_DeclineHint.Bind(inputBindStruct2).AddTo(this);
		GamePad.Instance.PushLayer(inputLayer).AddTo(this);
	}

	private void Decline()
	{
		base.ViewModel.DeclineController();
	}

	private void SwitchControlMode()
	{
		m_HeaderLabel.text = UIStrings.Instance.ControllerModeTexts.ChangeInputProcess;
		m_HintLabel.text = string.Empty;
		HandleCurrentState(value: false);
		DelayedSwitch();
	}

	private void DelayedSwitch()
	{
		Observable.NextFrame().Subscribe(base.ViewModel.SwitchControlMode).AddTo(this);
	}

	private void HandleCurrentState(bool value)
	{
		Game.Instance.Keyboard.Disabled.SetValue(value);
		Game.Instance.RequestPauseUi(value);
		m_CanConfirm.Value = value;
		m_CanDecline.Value = value;
	}
}
