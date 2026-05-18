using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ChooseControllerModeContext : ViewModel
{
	private const float MinimumDelayBetweenPrompts = 1f;

	private readonly ReactiveProperty<GamepadConnectDisconnectVM> m_ViewModel;

	private readonly Func<bool> m_CanChangeInput;

	private IDisposable m_ControllerDeclinedSubscription;

	private IDisposable m_InputDisposable;

	private float m_LastInput = float.MinValue;

	private Game.ControllerModeType CurrentControllerMode => Game.Instance.ControllerMode;

	public ChooseControllerModeContext(ReactiveProperty<GamepadConnectDisconnectVM> vm, Func<bool> canChangeInput)
	{
		m_ViewModel = vm;
		m_CanChangeInput = canChangeInput;
		_ = Game.ControllerOverride.HasValue;
	}

	protected override void OnDispose()
	{
		m_ViewModel.CurrentValue?.Dispose();
		m_ControllerDeclinedSubscription?.Dispose();
	}

	private void CreateVM(Game.ControllerModeType requestControllerType)
	{
		if (m_ViewModel.Value == null)
		{
			GamepadConnectDisconnectVM gamepadConnectDisconnectVM = new GamepadConnectDisconnectVM(delegate(Game.ControllerModeType mode)
			{
				Game.Instance.ControllerMode = mode;
			}, requestControllerType);
			m_ControllerDeclinedSubscription = gamepadConnectDisconnectVM.ControllerDeclined.Subscribe(HandleControllerDeclined);
			m_ViewModel.Value = gamepadConnectDisconnectVM;
		}
	}

	private void HandleControllerDeclined()
	{
		DisposeVM();
	}

	private void DisposeVM()
	{
		m_ViewModel.CurrentValue?.Dispose();
		m_ViewModel.Value = null;
		m_ControllerDeclinedSubscription?.Dispose();
	}

	private void OnGamepadConnected()
	{
		TryCreateDialog(Game.ControllerModeType.Gamepad);
	}

	private void OnGamepadDisconnected()
	{
		TryCreateDialog(Game.ControllerModeType.Mouse);
	}

	private void GetInputLayer()
	{
	}

	private void HandleInput()
	{
	}

	private void TryCreateDialog(Game.ControllerModeType requestControllerType)
	{
		if (m_CanChangeInput() && CurrentControllerMode != requestControllerType)
		{
			CreateVM(requestControllerType);
			return;
		}
		if (!m_CanChangeInput())
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ControllerModeTexts.CantChangeInput);
			});
		}
		DisposeVM();
	}
}
