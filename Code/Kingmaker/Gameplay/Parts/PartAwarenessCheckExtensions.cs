using JetBrains.Annotations;

namespace Kingmaker.Gameplay.Parts;

public static class PartAwarenessCheckExtensions
{
	public static bool IsPassed([CanBeNull] this PartAwarenessCheck part)
	{
		return part?.IsPassed ?? true;
	}

	public static void SetPassed([CanBeNull] this PartAwarenessCheck part, bool value)
	{
		if (part != null)
		{
			part.IsPassed = value;
		}
	}
}
