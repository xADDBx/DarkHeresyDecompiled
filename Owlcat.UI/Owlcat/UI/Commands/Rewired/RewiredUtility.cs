using System.Collections.Generic;
using System.Text;
using Rewired;
using UnityEngine;

namespace Owlcat.UI.Commands.Rewired;

internal static class RewiredUtility
{
	private static readonly Dictionary<(bool ctrl, bool alt, bool shift, bool command, KeyCode code), string> sCacheKeys = new Dictionary<(bool, bool, bool, bool, KeyCode), string>();

	private static readonly Dictionary<(InputActionEventType, int), string> sCacheActions = new Dictionary<(InputActionEventType, int), string>();

	public static string GetBindingNonAlloc(Keyboard keyboard, KeyCode keyCode)
	{
		if (Keyboard.IsModifierKey(keyCode))
		{
			return null;
		}
		bool modifierKey = keyboard.GetModifierKey(ModifierKey.Control);
		bool modifierKey2 = keyboard.GetModifierKey(ModifierKey.Alt);
		bool modifierKey3 = keyboard.GetModifierKey(ModifierKey.Shift);
		bool modifierKey4 = keyboard.GetModifierKey(ModifierKey.Command);
		(bool, bool, bool, bool, KeyCode) key = (modifierKey, modifierKey2, modifierKey3, modifierKey4, keyCode);
		if (sCacheKeys.TryGetValue(key, out var value))
		{
			return value;
		}
		StringBuilder stringBuilder = new StringBuilder("keyboard:");
		if (modifierKey)
		{
			stringBuilder.Append("ctrl+");
		}
		if (modifierKey2)
		{
			stringBuilder.Append("alt+");
		}
		if (modifierKey3)
		{
			stringBuilder.Append("shift+");
		}
		if (modifierKey4)
		{
			stringBuilder.Append("cmd+");
		}
		stringBuilder.Append(ConvertToCamelCase(Keyboard.GetKeyName(keyCode).ToLowerInvariant()));
		value = stringBuilder.ToString();
		sCacheKeys.Add(key, value);
		return value;
	}

	private static string ConvertToCamelCase(string input)
	{
		string[] array = input.Split(' ');
		if (array.Length != 0)
		{
			array[0] = char.ToLower(array[0][0]) + array[0].Substring(1);
		}
		for (int i = 1; i < array.Length; i++)
		{
			array[i] = char.ToUpper(array[i][0]) + array[i].Substring(1);
		}
		return string.Join("", array);
	}

	public static string GetBindingNonAlloc(InputActionEventData actionEventData)
	{
		(InputActionEventType, int) key = (actionEventData.eventType, actionEventData.actionId);
		if (sCacheActions.TryGetValue(key, out var value))
		{
			return value;
		}
		int actionId = actionEventData.actionId;
		if (actionId == 0 || actionId == 1 || actionId == 2 || actionId == 3)
		{
			sCacheActions[key] = null;
			return null;
		}
		string name = GetName(actionEventData.actionId);
		string interaction = GetInteraction(actionEventData.eventType);
		if (name == null || interaction == null)
		{
			sCacheActions[key] = null;
			return null;
		}
		return sCacheActions[key] = "gamepad:" + name + ((interaction == string.Empty) ? string.Empty : ("#" + interaction));
	}

	public static string GetInteraction(InputActionEventType eventType)
	{
		return eventType switch
		{
			InputActionEventType.ButtonJustPressed => "", 
			InputActionEventType.ButtonJustLongPressed => "longpress", 
			InputActionEventType.ButtonJustReleased => "release", 
			InputActionEventType.ButtonPressed => "hold", 
			InputActionEventType.ButtonRepeating => "repeat", 
			_ => null, 
		};
	}

	public static string GetName(int actionId)
	{
		return actionId switch
		{
			8 => "buttonSouth", 
			9 => "buttonEast", 
			10 => "buttonWest", 
			11 => "buttonNorth", 
			6 => "dpadUp", 
			5 => "dpadRight", 
			4 => "dpadLeft", 
			7 => "dpadDown", 
			0 => "leftStickX", 
			1 => "leftStickY", 
			18 => "leftStickButton", 
			2 => "rightStickX", 
			3 => "rightStickY", 
			19 => "rightStickButton", 
			12 => "leftBottom", 
			14 => "leftTop", 
			13 => "rightBottom", 
			15 => "rightTop", 
			16 => "options", 
			17 => "start", 
			_ => null, 
		};
	}
}
