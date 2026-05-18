using UnityEngine.InputSystem;

namespace Kingmaker.Code.View.Bridge.Utils;

public static class UtilsConsole
{
	public static bool HasAnyGamepad => Gamepad.all.Count > 0;
}
