using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ChooseControllerModeContext : ViewModel
{
	private const float MinimumDelayBetweenPrompts = 1f;

	private readonly ReactiveProperty<GamepadConnectDisconnectVM> m_ViewModel;

	private readonly Func<bool> m_CanChangeInput;

	private readonly InputLayer m_InputLayer;

	private IDisposable m_ControllerDeclinedSubscription;

	private IDisposable m_InputDisposable;

	private float m_LastInput = float.MinValue;

	public ChooseControllerModeContext(ReactiveProperty<GamepadConnectDisconnectVM> vm, Func<bool> canChangeInput)
	{
		m_ViewModel = vm;
		m_CanChangeInput = canChangeInput;
		if (!Game.ControllerOverride.HasValue)
		{
			m_InputLayer = GetInputLayer();
			ReInput.ControllerConnectedEvent += OnGamepadConnected;
			ReInput.ControllerDisconnectedEvent += OnGamepadDisconnected;
			if (ReInput.controllers.Joysticks.Count > 0 && Game.Instance.ControllerMode != Game.ControllerModeType.Gamepad)
			{
				PushInputLayer();
			}
		}
	}

	protected override void OnDispose()
	{
		ReInput.ControllerConnectedEvent -= OnGamepadConnected;
		ReInput.ControllerDisconnectedEvent -= OnGamepadDisconnected;
		m_ViewModel.CurrentValue?.Dispose();
		m_ControllerDeclinedSubscription?.Dispose();
		try
		{
			PopInputLayer();
		}
		catch (NullReferenceException)
		{
		}
	}

	private void CreateVM()
	{
		if (m_ViewModel.Value == null)
		{
			GamepadConnectDisconnectVM gamepadConnectDisconnectVM = new GamepadConnectDisconnectVM(delegate(Game.ControllerModeType mode)
			{
				Game.Instance.ControllerMode = mode;
			});
			m_ControllerDeclinedSubscription = gamepadConnectDisconnectVM.ControllerDeclined.Subscribe(HandleControllerDeclined);
			m_ViewModel.Value = gamepadConnectDisconnectVM;
		}
	}

	private void HandleControllerDeclined()
	{
		DisposeVM();
		PushInputLayer();
	}

	private void DisposeVM()
	{
		m_ViewModel.CurrentValue?.Dispose();
		m_ViewModel.Value = null;
		m_ControllerDeclinedSubscription?.Dispose();
	}

	private void OnGamepadConnected(ControllerStatusChangedEventArgs obj)
	{
		HandleGamepadConnected();
	}

	private void OnGamepadDisconnected(ControllerStatusChangedEventArgs obj)
	{
		HandleGamepadDisconnected();
	}

	private InputLayer GetInputLayer()
	{
		InputLayer inputLayer = new InputLayer();
		inputLayer.ContextName = "ChooseControllerModeWindowView";
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			HandleInput(eventData);
		}, 8);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			HandleInput(eventData);
		}, 9);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			HandleInput(eventData);
		}, 10);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			HandleInput(eventData);
		}, 11);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			HandleInput(eventData);
		}, 16);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			HandleInput(eventData);
		}, 17);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			HandleInput(eventData);
		}, 12);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			HandleInput(eventData);
		}, 14);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			HandleInput(eventData);
		}, 13);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			HandleInput(eventData);
		}, 15);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			HandleInput(eventData);
		}, 7);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			HandleInput(eventData);
		}, 4);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			HandleInput(eventData);
		}, 5);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			HandleInput(eventData);
		}, 6);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			HandleInput(eventData);
		}, 18);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			HandleInput(eventData);
		}, 19);
		inputLayer.AddAxis(delegate(InputActionEventData eventData, float _)
		{
			HandleInput(eventData);
		}, 0);
		inputLayer.AddAxis(delegate(InputActionEventData eventData, float _)
		{
			HandleInput(eventData);
		}, 1);
		inputLayer.AddAxis(delegate(InputActionEventData eventData, float _)
		{
			HandleInput(eventData);
		}, 2);
		inputLayer.AddAxis(delegate(InputActionEventData eventData, float _)
		{
			HandleInput(eventData);
		}, 3);
		return inputLayer;
	}

	private void HandleInput(InputActionEventData? eventData)
	{
		if (!IsKeyboardInput(eventData))
		{
			if (Time.realtimeSinceStartup > m_LastInput + 1f)
			{
				HandleGamepadConnected();
			}
			m_LastInput = Time.realtimeSinceStartup;
		}
		static bool IsKeyboardInput(InputActionEventData? evt)
		{
			if (!evt.HasValue)
			{
				return false;
			}
			foreach (InputActionSourceData currentInputSource in evt.Value.GetCurrentInputSources())
			{
				if (currentInputSource.controllerType == ControllerType.Keyboard)
				{
					return true;
				}
			}
			return false;
		}
	}

	private void HandleGamepadConnected()
	{
		if (!m_CanChangeInput())
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ControllerModeTexts.CantChangeInput);
			});
		}
		else
		{
			PushInputLayer();
			CreateVM();
		}
	}

	private void HandleGamepadDisconnected()
	{
		PopInputLayer();
		DisposeVM();
	}

	private void PushInputLayer()
	{
		m_InputDisposable?.Dispose();
		GamePad.Instance.PushLayer(m_InputLayer);
	}

	private void PopInputLayer()
	{
		GamePad.Instance.PopLayer(m_InputLayer);
		m_InputDisposable?.Dispose();
		m_InputDisposable = null;
	}
}
