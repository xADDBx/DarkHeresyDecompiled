using System.Collections.Generic;
using System.Text;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace Owlcat.UI.Commands.InputSystem;

internal class UnityInputSystemEvent : InputEvent
{
	private static readonly Dictionary<(bool ctrl, bool alt, bool shift, Key code, string interaction), string> sCacheKeys = new Dictionary<(bool, bool, bool, Key, string), string>();

	private static readonly Dictionary<(string name, string interaction), string> sCacheGamepadButtons = new Dictionary<(string, string), string>();

	private static readonly Dictionary<(bool ctrl, bool alt, bool shift, string name, string interaction), string> sCacheMouseKeys = new Dictionary<(bool, bool, bool, string, string), string>();

	private static readonly UnityInputSystemEvent sInstance = new UnityInputSystemEvent();

	private string m_Binding;

	public static bool TryCreate(Keyboard keyboard, KeyControl keyControl, string interaction, float progress, out InputEvent inputEvent)
	{
		if (IsModifierKey(keyControl.keyCode))
		{
			inputEvent = null;
			return false;
		}
		bool flag = keyboard.ctrlKey.isPressed || keyboard.leftCommandKey.isPressed || keyboard.rightCommandKey.isPressed;
		bool isPressed = keyboard.altKey.isPressed;
		bool isPressed2 = keyboard.shiftKey.isPressed;
		(bool, bool, bool, Key, string) key = (flag, isPressed, isPressed2, keyControl.keyCode, interaction);
		if (sCacheKeys.TryGetValue(key, out var value))
		{
			inputEvent = sInstance.Reset(value, progress);
			return true;
		}
		StringBuilder stringBuilder = new StringBuilder("keyboard:");
		if (flag)
		{
			stringBuilder.Append("ctrl+");
		}
		if (isPressed)
		{
			stringBuilder.Append("alt+");
		}
		if (isPressed2)
		{
			stringBuilder.Append("shift+");
		}
		stringBuilder.Append(GetKeyName(keyControl.keyCode));
		stringBuilder.Append(GetInteractionString(interaction));
		value = stringBuilder.ToString();
		sCacheKeys.Add(key, value);
		inputEvent = sInstance.Reset(value, progress);
		return true;
	}

	public static bool TryCreate(Keyboard keyboard, ButtonControl button, string interaction, float progress, out InputEvent inputEvent)
	{
		inputEvent = null;
		bool flag = keyboard.ctrlKey.isPressed || keyboard.leftCommandKey.isPressed || keyboard.rightCommandKey.isPressed;
		bool isPressed = keyboard.altKey.isPressed;
		bool isPressed2 = keyboard.shiftKey.isPressed;
		(bool, bool, bool, string, string) key = (flag, isPressed, isPressed2, button.name, interaction);
		if (!sCacheMouseKeys.TryGetValue(key, out var value))
		{
			string text = string.Empty;
			if (button.name == "leftButton")
			{
				text = "mouse0";
			}
			else if (button.name == "rightButton")
			{
				text = "mouse1";
			}
			else if (button.name == "middleButton")
			{
				text = "mouse2";
			}
			if (text == string.Empty)
			{
				return false;
			}
			StringBuilder stringBuilder = new StringBuilder("keyboard:");
			if (flag)
			{
				stringBuilder.Append("ctrl+");
			}
			if (isPressed)
			{
				stringBuilder.Append("alt+");
			}
			if (isPressed2)
			{
				stringBuilder.Append("shift+");
			}
			stringBuilder.Append(text);
			stringBuilder.Append(GetInteractionString(interaction));
			value = stringBuilder.ToString();
			sCacheMouseKeys.Add(key, value);
		}
		if (value != string.Empty)
		{
			inputEvent = sInstance.Reset(value, progress);
		}
		return inputEvent != null;
	}

	public static bool TryCreate(Gamepad gamepad, ButtonControl button, string interaction, float progress, out InputEvent inputEvent)
	{
		(string, string) key = (button.name, interaction);
		if (!sCacheGamepadButtons.TryGetValue(key, out var value))
		{
			string text = ((gamepad[GamepadButton.Start] == button) ? "options" : button.name);
			value = "gamepad:" + text + GetInteractionString(interaction);
			sCacheGamepadButtons.Add(key, value);
		}
		inputEvent = sInstance.Reset(value, progress);
		return inputEvent != null;
	}

	private static bool IsModifierKey(Key key)
	{
		if ((uint)(key - 51) <= 7u)
		{
			return true;
		}
		return false;
	}

	private static string GetKeyName(Key keyCode)
	{
		StringBuilder stringBuilder = new StringBuilder(ConvertToCamelCase(keyCode.ToString()));
		stringBuilder.Replace("escape", "esc");
		stringBuilder.Replace("enter", "return");
		stringBuilder.Replace("digit", string.Empty);
		return stringBuilder.ToString();
	}

	private static string ConvertToCamelCase(string input)
	{
		char[] array = input.ToCharArray();
		for (int i = 0; i < array.Length && char.IsLetter(array[i]) && char.IsUpper(array[i]); i++)
		{
			array[i] = char.ToLowerInvariant(array[i]);
		}
		return new string(array);
	}

	private static string GetInteractionString(string interaction)
	{
		if (!(interaction == string.Empty))
		{
			return "#" + interaction;
		}
		return string.Empty;
	}

	private UnityInputSystemEvent()
	{
	}

	private UnityInputSystemEvent Reset(string binding, float progress)
	{
		m_Binding = binding;
		Progress = progress;
		return this;
	}

	public override bool IsTrigger(string binding)
	{
		return InputEvent.Contains(m_Binding, binding);
	}
}
