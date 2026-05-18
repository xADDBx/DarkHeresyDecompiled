using JetBrains.Annotations;

namespace Kingmaker.Gameplay.Parts;

public static class PartAwarenessCheckExtensions
{
	public static bool GetPassed([CanBeNull] this PartAwarenessCheck part)
	{
		if (part == null)
		{
			return true;
		}
		if (part.Settings.HiddenInDarkness && !Game.Instance.Player.Flashlight.FlashlightInUse)
		{
			return true;
		}
		return part.GetIsPassed;
	}

	public static void SetPassed([CanBeNull] this PartAwarenessCheck part, bool value)
	{
		part?.SetIsPassed(value);
	}
}
