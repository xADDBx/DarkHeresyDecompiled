using System;
using Rewired;

namespace Owlcat.UI.Commands.Rewired;

public class RewiredInputSource : IDisposable
{
	private readonly Player m_Player;

	private readonly CommandLayerStack m_Stack;

	private readonly Keyboard m_Keyboard;

	private readonly RewiredAxis2D m_LeftStick;

	private readonly RewiredAxis2D m_RightStick;

	private bool m_Disposed;

	public RewiredInputSource(Player player, CommandLayerStack commandLayerStack)
	{
		m_Player = player;
		m_Player.controllers.hasKeyboard = true;
		m_Player.AddInputEventDelegate(OnInputEventDelegate, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed);
		m_Player.AddInputEventDelegate(OnInputEventDelegate, UpdateLoopType.Update, InputActionEventType.ButtonRepeating);
		m_Player.AddInputEventDelegate(OnInputEventDelegate, UpdateLoopType.Update, InputActionEventType.ButtonPressed);
		m_Player.AddInputEventDelegate(OnInputEventDelegate, UpdateLoopType.Update, InputActionEventType.ButtonJustLongPressed);
		m_Player.AddInputEventDelegate(OnInputEventDelegate, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased);
		m_Player.AddInputEventDelegate(OnAxisActiveOrJustInactive, UpdateLoopType.Update, InputActionEventType.AxisActiveOrJustInactive);
		m_Stack = commandLayerStack;
		m_Keyboard = m_Player.controllers.Keyboard;
		m_LeftStick = new RewiredAxis2D("leftStick", 0, 1);
		m_RightStick = new RewiredAxis2D("rightStick", 2, 3);
	}

	public void Dispose()
	{
		m_Disposed = true;
		m_Player.RemoveInputEventDelegate(OnInputEventDelegate);
		m_Player.RemoveInputEventDelegate(OnAxisActiveOrJustInactive);
	}

	private void OnInputEventDelegate(InputActionEventData inputActionEventData)
	{
		if (RewiredInputEvent.TryCreate(inputActionEventData, out var result))
		{
			m_Stack.Consume(result);
		}
	}

	private void OnAxisActiveOrJustInactive(InputActionEventData inputActionEventData)
	{
		m_LeftStick.OnAxisActiveOrJustInactive(inputActionEventData);
		m_RightStick.OnAxisActiveOrJustInactive(inputActionEventData);
	}

	public void Update()
	{
		if (m_Disposed)
		{
			throw new InvalidOperationException("RewiredInputSource was disposed");
		}
		for (int i = 0; i < m_Keyboard.buttonCount; i++)
		{
			if (m_Keyboard.GetButtonDown(i) && RewiredInputEvent.TryCreate(m_Keyboard, m_Keyboard.GetKeyCodeByButtonIndex(i), out var result))
			{
				m_Stack.Consume(result);
			}
		}
		m_LeftStick.Update(m_Stack);
		m_RightStick.Update(m_Stack);
	}
}
