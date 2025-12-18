using System;
using Kingmaker.Code.View.Bridge.OBSOLETE;
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
	private readonly ReactiveCommand<Unit> m_GamepadConnected = new ReactiveCommand<Unit>();

	private readonly ReactiveCommand<Unit> m_GamepadDisconnected = new ReactiveCommand<Unit>();

	private readonly ReactiveCommand<Unit> m_ControllerDeclined = new ReactiveCommand<Unit>();

	private readonly Action<Game.ControllerModeType> m_ChangeControllerModeAction;

	public Observable<Unit> GamepadConnected => m_GamepadConnected;

	public Observable<Unit> GamepadDisconnected => m_GamepadDisconnected;

	private Game.ControllerModeType? ControllerOverride => Game.ControllerOverride;

	public bool IsControllerOverride => ControllerOverride.HasValue;

	public Observable<Unit> ControllerDeclined => m_ControllerDeclined;

	public GamepadConnectDisconnectVM(Action<Game.ControllerModeType> changeControllerModeAction)
	{
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
		Game.Instance.ControllerMode = ((Game.Instance.ControllerMode != Game.ControllerModeType.Gamepad) ? Game.ControllerModeType.Gamepad : Game.ControllerModeType.Mouse);
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

	public void DeclineController()
	{
		Game.Instance.ControllerMode = Game.ControllerModeType.Mouse;
		Game.DontChangeController = true;
		m_ControllerDeclined.Execute();
	}
}
