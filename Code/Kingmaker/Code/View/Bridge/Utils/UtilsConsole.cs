using Rewired;

namespace Kingmaker.Code.View.Bridge.Utils;

public static class UtilsConsole
{
	public static bool GamepadIsConnected
	{
		get
		{
			if (ReInput.isReady && ReInput.controllers != null)
			{
				return ReInput.controllers.joystickCount > 0;
			}
			return false;
		}
	}
}
