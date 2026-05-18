using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Transitions;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[ViewFactoryPolicy(ViewFactoryPolicyFlag.DontPool, null)]
public class GamepadConnectedDisconnectedWindowView : View<GamepadConnectDisconnectVM>, ITransitable
{
	[SerializeField]
	private WindowAnimator m_WindowAnimator;

	[SerializeField]
	public TextMeshProUGUI m_HeaderLabel;

	[SerializeField]
	public TextMeshProUGUI m_HintLabel;

	[Header("Console Hints Block")]
	[SerializeField]
	private GameObject m_ConsoleHintsBlock;

	[SerializeField]
	private HintView m_ConfirmHint;

	[SerializeField]
	private HintView m_DeclineHint;

	[Header("Buttons Block")]
	[SerializeField]
	private GameObject m_ButtonsBlock;

	[SerializeField]
	private OwlcatMultiButton m_ConfirmButton;

	[SerializeField]
	private OwlcatMultiButton m_DeclineButton;

	[SerializeField]
	private TextMeshProUGUI m_ConfirmLabel;

	[SerializeField]
	private TextMeshProUGUI m_DeclineLabel;

	private UIAnimatorTransition m_ShowTransition;

	private UIAnimatorTransition m_HideTransition;

	private void Awake()
	{
		m_WindowAnimator.Initialize();
		m_ShowTransition = new UIAnimatorShowTransition(m_WindowAnimator);
		m_HideTransition = new UIAnimatorHideTransition(m_WindowAnimator);
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_ConsoleHintsBlock.gameObject.SetActive(base.ViewModel.RequestControllerType == Game.ControllerModeType.Gamepad);
		m_ButtonsBlock.gameObject.SetActive(base.ViewModel.RequestControllerType == Game.ControllerModeType.Mouse);
		if (base.ViewModel.RequestControllerType == Game.ControllerModeType.Gamepad)
		{
			m_HeaderLabel.text = UIStrings.Instance.ControllerModeTexts.GamepadConnectedHeaderText;
			m_HintLabel.text = UIStrings.Instance.ControllerModeTexts.GamepadConnectedText;
			HandleCurrentState(value: true);
			return;
		}
		m_HeaderLabel.text = UIStrings.Instance.ControllerModeTexts.GamepadDisconnectedHeaderText;
		m_HintLabel.text = UIStrings.Instance.ControllerModeTexts.GamepadDisconnectedText;
		m_ConfirmButton.OnLeftClickAsObservable().Subscribe(SwitchControlMode).AddTo(this);
		m_DeclineButton.OnLeftClickAsObservable().Subscribe(Decline).AddTo(this);
		m_ConfirmLabel.text = UIStrings.Instance.ControllerModeTexts.ConfirmSwitchText.Text;
		m_DeclineLabel.text = UIStrings.Instance.CommonTexts.Cancel.Text;
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

	private void Decline()
	{
		base.ViewModel.DeclineControllerSwitch();
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
	}
}
