using System;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class GamepadConnectDisconnectVM : ViewModel
{
	private readonly ReactiveCommand<Unit> m_ControllerDeclined = new ReactiveCommand<Unit>();

	private readonly Action<Game.ControllerModeType> m_ChangeControllerModeAction;

	private readonly Game.ControllerModeType m_RequestControllerType;

	private Game.ControllerModeType? ControllerOverride => Game.ControllerOverride;

	public bool IsControllerOverride => ControllerOverride.HasValue;

	public Observable<Unit> ControllerDeclined => m_ControllerDeclined;

	public Game.ControllerModeType RequestControllerType => m_RequestControllerType;

	public GamepadConnectDisconnectVM(Action<Game.ControllerModeType> changeControllerModeAction, Game.ControllerModeType requestType)
	{
		m_RequestControllerType = requestType;
		m_ChangeControllerModeAction = changeControllerModeAction;
	}

	public void SetGamepadMode()
	{
		if (EventSystem.current != null)
		{
			UnityEngine.Object.Destroy(EventSystem.current.gameObject);
		}
		EventSystem.current = null;
		m_ChangeControllerModeAction?.Invoke(Game.ControllerModeType.Gamepad);
	}

	public void SetKeyboardMode()
	{
		m_ChangeControllerModeAction?.Invoke(Game.ControllerModeType.Mouse);
	}

	public void SwitchControlMode()
	{
		Game.Instance.ControllerMode = m_RequestControllerType;
		Game.DontChangeController = true;
		NetGame.State currentState = PhotonManager.NetGame.CurrentState;
		Action onComplete = ((currentState == NetGame.State.NetInitialized || currentState == NetGame.State.InLobby) ? ((Action)delegate
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				EventBus.RaiseEvent(delegate(INetLobbyRequest h)
				{
					h.HandleNetLobbyRequest();
				});
			}, 5);
		}) : null);
		Game.Instance.RootUIContext.ResetUI(onComplete);
	}

	public void DeclineControllerSwitch()
	{
		Game.DontChangeController = true;
		m_ControllerDeclined.Execute(Unit.Default);
	}
}
