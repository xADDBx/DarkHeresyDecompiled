using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Owlcat.UI.Commands.InputSystem;

internal class UnityInputSystemControlHelper
{
	private KeyControl m_KeyControl;

	private ButtonControl m_ButtonControl;

	private Gamepad m_Gamepad;

	private Keyboard m_Keyboard;

	private float m_Timer;

	private const float LongPressTime = 1.4f;

	private const float RepeatPressTime = 2f;

	public UnityInputSystemControlHelper(KeyControl keyControl, Keyboard keyboard)
	{
		m_KeyControl = keyControl;
		m_ButtonControl = keyControl;
		m_Keyboard = keyboard;
	}

	public UnityInputSystemControlHelper(ButtonControl buttonControl, Gamepad gamepad)
	{
		m_ButtonControl = buttonControl;
		m_Gamepad = gamepad;
	}

	public UnityInputSystemControlHelper(ButtonControl buttonControl, Keyboard keyboard)
	{
		m_ButtonControl = buttonControl;
		m_Keyboard = keyboard;
	}

	public void Update(CommandLayerStack stack)
	{
		if (m_ButtonControl.wasPressedThisFrame)
		{
			TryConsumeEvent(stack, "");
			m_Timer = 0f;
		}
		else if (m_ButtonControl.isPressed)
		{
			float unscaledDeltaTime = Time.unscaledDeltaTime;
			TryConsumeEvent(stack, "hold");
			if (m_Timer > 1.4f)
			{
				TryConsumeEvent(stack, "longpress", 0f);
			}
			if (m_Timer < 1.4f)
			{
				TryConsumeEvent(stack, "longpress", Mathf.Clamp01(m_Timer + unscaledDeltaTime / 1.4f));
			}
			if ((int)((m_Timer + unscaledDeltaTime) / 2f) > (int)(m_Timer / 2f))
			{
				TryConsumeEvent(stack, "repeat");
			}
			m_Timer += unscaledDeltaTime;
		}
		else if (m_ButtonControl.wasReleasedThisFrame)
		{
			m_Timer = 0f;
			TryConsumeEvent(stack, "release");
			TryConsumeEvent(stack, "longpress", 0f);
		}
	}

	private void TryConsumeEvent(CommandLayerStack stack, string interaction, float progress = 1f)
	{
		if (m_Keyboard != null && m_KeyControl != null)
		{
			TryConsumeEvent(stack, m_Keyboard, m_KeyControl, interaction, progress);
		}
		else if (m_Keyboard != null)
		{
			TryConsumeEvent(stack, m_Keyboard, m_ButtonControl, interaction, progress);
		}
		else if (m_Gamepad != null)
		{
			TryConsumeEvent(stack, m_Gamepad, m_ButtonControl, interaction, progress);
		}
	}

	private void TryConsumeEvent(CommandLayerStack stack, Keyboard keyboard, KeyControl keyControl, string interaction, float progress)
	{
		if (UnityInputSystemEvent.TryCreate(keyboard, keyControl, interaction, progress, out var inputEvent))
		{
			stack.Consume(inputEvent);
		}
	}

	private void TryConsumeEvent(CommandLayerStack stack, Gamepad gamepad, ButtonControl buttonControl, string interaction, float progress)
	{
		if (UnityInputSystemEvent.TryCreate(gamepad, buttonControl, interaction, progress, out var inputEvent))
		{
			stack.Consume(inputEvent);
		}
	}

	private void TryConsumeEvent(CommandLayerStack stack, Keyboard keyboard, ButtonControl buttonControl, string interaction, float progress)
	{
		if (UnityInputSystemEvent.TryCreate(keyboard, buttonControl, interaction, progress, out var inputEvent))
		{
			stack.Consume(inputEvent);
		}
	}
}
