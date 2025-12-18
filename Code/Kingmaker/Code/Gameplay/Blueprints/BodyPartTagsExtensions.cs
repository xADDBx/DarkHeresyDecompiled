namespace Kingmaker.Code.Gameplay.Blueprints;

public static class BodyPartTagsExtensions
{
	public static bool HasAnyFlag(this BodyPartTags value, BodyPartTags flag)
	{
		return (value & flag) != 0;
	}
}
