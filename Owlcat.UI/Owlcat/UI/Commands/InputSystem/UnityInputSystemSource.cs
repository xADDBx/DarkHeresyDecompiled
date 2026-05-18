using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace Owlcat.UI.Commands.InputSystem;

public class UnityInputSystemSource
{
	private GamepadButton[] m_AllGamepadButtons;

	private Dictionary<string, UnityInputSystemControlHelper> m_CachedControls = new Dictionary<string, UnityInputSystemControlHelper>();

	private readonly CommandLayerStack m_Stack;

	private void UpdateImpl()
	{
		CacheKeyboardControls();
		CacheMouseControls();
		CacheGamepadControls();
		UpdateCachedControls();
	}

	private void CacheKeyboardControls()
	{
		Keyboard current = Keyboard.current;
		if (current == null)
		{
			return;
		}
		ReadOnlyArray<KeyControl> allKeys = current.allKeys;
		for (int i = 0; i < allKeys.Count; i++)
		{
			KeyControl keyControl = allKeys[i];
			if (keyControl != null && !m_CachedControls.ContainsKey(keyControl.name) && keyControl.wasPressedThisFrame)
			{
				AddToCache(keyControl, current);
			}
		}
	}

	private void CacheMouseControls()
	{
		Keyboard current = Keyboard.current;
		if (current == null)
		{
			return;
		}
		Mouse current2 = Mouse.current;
		if (current2 == null)
		{
			return;
		}
		ReadOnlyArray<InputControl> allControls = current2.allControls;
		for (int i = 0; i < allControls.Count; i++)
		{
			InputControl inputControl = allControls[i];
			if (inputControl != null && !m_CachedControls.ContainsKey(inputControl.name) && inputControl is ButtonControl buttonControl && !m_CachedControls.ContainsKey(buttonControl.name) && buttonControl.wasPressedThisFrame)
			{
				AddToCache(buttonControl, current);
			}
		}
	}

	private void CacheGamepadControls()
	{
		Gamepad current = Gamepad.current;
		if (current == null)
		{
			return;
		}
		if (m_AllGamepadButtons == null)
		{
			m_AllGamepadButtons = Enum.GetValues(typeof(GamepadButton)).Cast<GamepadButton>().Distinct()
				.ToArray();
		}
		GamepadButton[] allGamepadButtons = m_AllGamepadButtons;
		foreach (GamepadButton button in allGamepadButtons)
		{
			ButtonControl buttonControl = current[button];
			if (buttonControl != null && !m_CachedControls.ContainsKey(buttonControl.name) && buttonControl.wasPressedThisFrame)
			{
				AddToCache(buttonControl, current);
			}
		}
	}

	private void UpdateCachedControls()
	{
		foreach (UnityInputSystemControlHelper value in m_CachedControls.Values)
		{
			value.Update(m_Stack);
		}
	}

	private void AddToCache(KeyControl keyControl, Keyboard keyboard)
	{
		m_CachedControls.Add(keyControl.name, new UnityInputSystemControlHelper(keyControl, keyboard));
	}

	private void AddToCache(ButtonControl buttonControl, Gamepad gamepad)
	{
		m_CachedControls.Add(buttonControl.name, new UnityInputSystemControlHelper(buttonControl, gamepad));
	}

	private void AddToCache(ButtonControl buttonControl, Keyboard keyboard)
	{
		m_CachedControls.Add(buttonControl.name, new UnityInputSystemControlHelper(buttonControl, keyboard));
	}

	public static bool IsSupported()
	{
		return true;
	}

	public UnityInputSystemSource(CommandLayerStack stack)
	{
		m_Stack = stack;
	}

	public void Update()
	{
		UpdateImpl();
	}
}
