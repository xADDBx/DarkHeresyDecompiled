using Kingmaker.GameModes;

namespace Kingmaker.Code.UI.MVVM.SignalDevice;

public static class SignalDeviceUtils
{
	public static bool CanPulse()
	{
		if (RootUIContext.Instance.GameUIState.IsInCombat.Value || RootUIContext.Instance.GameUIState.GameMode.Value != GameModeType.Default)
		{
			return false;
		}
		return true;
	}
}
