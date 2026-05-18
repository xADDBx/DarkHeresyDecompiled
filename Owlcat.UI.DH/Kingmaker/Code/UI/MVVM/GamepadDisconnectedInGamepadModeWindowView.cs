using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class GamepadDisconnectedInGamepadModeWindowView : View<GamepadConnectDisconnectVM>
{
	[SerializeField]
	private OwlcatButton m_ConfirmButton;

	[SerializeField]
	private OwlcatButton m_DeclineButton;

	[SerializeField]
	private TextMeshProUGUI m_ConfirmLabel;

	[SerializeField]
	private TextMeshProUGUI m_DeclineLabel;

	[SerializeField]
	private WindowAnimator m_WindowAnimator;

	[SerializeField]
	private TextMeshProUGUI m_BodyLabel;

	[SerializeField]
	private TextMeshProUGUI m_HintLabel;

	private readonly ReactiveProperty<bool> m_IsGamepadConnected = new ReactiveProperty<bool>(value: true);

	private bool m_IsOpened;

	private EventSystem m_storedGamepadModeEventSystem;

	private IDisposable m_KeyboardInput;

	private CompositeDisposable m_EscSubscription = new CompositeDisposable();

	private bool m_CursorVisible;

	public void Initialize()
	{
		m_WindowAnimator.Initialize();
	}

	protected override void OnBind()
	{
		if (!base.ViewModel.IsControllerOverride)
		{
			m_BodyLabel.text = UIStrings.Instance.ControllerModeTexts.GamepadDisconnectedHeaderText;
			m_HintLabel.text = UIStrings.Instance.ControllerModeTexts.GamepadDisconnectedText;
			m_KeyboardInput = Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate).Subscribe(OnLateUpdate).AddTo(this);
			m_ConfirmButton.OnLeftClickAsObservable().Subscribe(SwitchControlMode).AddTo(this);
			m_DeclineButton.OnLeftClickAsObservable().Subscribe(GamepadConnected).AddTo(this);
			m_IsGamepadConnected.Subscribe(delegate(bool value)
			{
				m_DeclineButton.Interactable = value;
			}).AddTo(this);
			m_ConfirmLabel.text = UIStrings.Instance.ControllerModeTexts.ConfirmSwitchText.Text;
			m_DeclineLabel.text = UIStrings.Instance.CommonTexts.Cancel.Text;
		}
	}

	protected override void OnUnbind()
	{
		if (m_IsOpened)
		{
			Hide();
		}
		ClearKeyboard();
	}

	private void GamepadConnected()
	{
		if (m_IsOpened)
		{
			Hide();
			DelayedKeyboardSub();
		}
	}

	private void GamepadDisconnected()
	{
		if (!RootUIContext.CanChangeInput())
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ControllerModeTexts.CantChangeInput);
			});
		}
		else
		{
			Show();
		}
	}

	private void Show()
	{
		ClearKeyboard();
		m_storedGamepadModeEventSystem = EventSystem.current;
		m_storedGamepadModeEventSystem.enabled = false;
		base.gameObject.SetActive(value: true);
		m_WindowAnimator.AppearAnimation();
		m_IsOpened = true;
		HandleCurrentState(value: true);
		m_CursorVisible = Cursor.visible;
		Cursor.visible = true;
	}

	private void OnLateUpdate()
	{
		if (!IgnoreKeyPressed() && Input.GetKeyDown(KeyCode.Space))
		{
			GamepadDisconnected();
		}
	}

	private bool IgnoreKeyPressed()
	{
		if (!RootUIContext.Instance.IsChargenShown && !RootUIContext.Instance.IsBugReportOpen && !RootUIContext.Instance.SaveLoadIsShown && RootUIContext.Instance.CurrentServiceWindow != ServiceWindowsType.Inventory)
		{
			return RootUIContext.Instance.CreditsAreShown;
		}
		return true;
	}

	private void Hide()
	{
		m_WindowAnimator.DisappearAnimation(delegate
		{
			base.gameObject.SetActive(value: false);
			m_storedGamepadModeEventSystem.enabled = true;
			m_storedGamepadModeEventSystem = null;
		});
		m_IsOpened = false;
		HandleCurrentState(value: false);
		m_EscSubscription.Clear();
		Cursor.visible = m_CursorVisible;
	}

	private void ClearKeyboard()
	{
		m_KeyboardInput?.Dispose();
	}

	private void HandleCurrentState(bool value)
	{
		Game.Instance.Keyboard.Disabled.SetValue(value);
		Game.Instance.RequestPauseUi(value);
		m_ConfirmButton.Interactable = value;
		m_DeclineButton.Interactable = value;
	}

	private void SwitchControlMode()
	{
		m_BodyLabel.text = UIStrings.Instance.ControllerModeTexts.ChangeInputProcess;
		m_HintLabel.text = string.Empty;
		HandleCurrentState(value: false);
		DelayedSwitch();
	}

	private void DelayedSwitch()
	{
		Observable.NextFrame().Subscribe(base.ViewModel.SwitchControlMode).AddTo(this);
	}

	private void DelayedKeyboardSub()
	{
		Observable.NextFrame().Subscribe(Sub).AddTo(this);
	}

	private void Sub()
	{
		m_KeyboardInput = Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate).Subscribe(OnLateUpdate).AddTo(this);
	}
}
