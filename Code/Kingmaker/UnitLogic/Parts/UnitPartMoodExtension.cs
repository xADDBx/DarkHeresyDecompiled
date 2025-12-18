using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class UnitPartMoodExtension
{
	public static Mood GetMood(this BaseUnitEntity entity)
	{
		return entity.GetOptional<UnitPartMood>()?.Mood ?? Mood.Neutral;
	}

	public static void SetMood(this BaseUnitEntity entity, Mood mood)
	{
		if (entity.GetMood() != mood)
		{
			entity.GetOrCreate<UnitPartMood>().Mood = mood;
		}
	}
}
