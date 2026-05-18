using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Experience;

public static class Experience
{
	private static readonly SkillCheckDifficulty[] StandardSkillCheckDifficulties = new SkillCheckDifficulty[4]
	{
		SkillCheckDifficulty.Easy,
		SkillCheckDifficulty.Normal,
		SkillCheckDifficulty.Hard,
		SkillCheckDifficulty.Impossible
	};

	private static ProgressionRoot Settings => ConfigRoot.Instance.Progression;

	private static int CurrentAreaCR => Game.Instance.CurrentlyLoadedArea?.GetCR() ?? 0;

	public static void Gain([NotNull] IExperienceSettings experience, MechanicEntity actor = null)
	{
		Gain(experience.OverrideValue ?? Calculate(experience.Type, experience.OverrideCR), actor);
	}

	public static void TryGain(BlueprintClue blueprint, MechanicEntity actor = null)
	{
		TryGainInternal(blueprint, actor);
	}

	public static void TryGain(BlueprintTrap blueprint, MechanicEntity actor = null)
	{
		TryGainInternal(blueprint, actor);
	}

	public static void GainForEncounter(BlueprintEncounter blueprint)
	{
		int? overrideExperience = blueprint.OverrideExperience;
		int experience;
		if (!overrideExperience.HasValue)
		{
			int? overrideCR = blueprint.CR;
			UnitDifficultyType? difficulty = blueprint.Difficulty;
			experience = Calculate(ExperienceType.Encounter, overrideCR, null, difficulty);
		}
		else
		{
			experience = overrideExperience.GetValueOrDefault();
		}
		Gain(experience, null, isForEncounter: true);
	}

	public static void TryGain(BlueprintQuest blueprint, MechanicEntity actor = null)
	{
		Gain(blueprint.CompletionExperience);
	}

	public static void TryGain(BlueprintQuestObjective blueprint, MechanicEntity actor = null)
	{
		Gain(blueprint.CompletionExperience);
	}

	public static void GainForSkillCheck(SkillCheckDifficulty difficulty, MechanicEntity actor = null)
	{
		Gain(CalculateSkillCheckExperience(difficulty, Game.Instance.Player.Chapter), actor);
	}

	public static void GainForSkillCheck(SkillCheckDifficulty difficulty, int dc, MechanicEntity actor = null, int? overrideCR = null)
	{
		Gain((difficulty == SkillCheckDifficulty.Custom) ? CalculateCustomSkillCheckExperience(dc, overrideCR) : CalculateSkillCheckExperience(difficulty, Game.Instance.Player.Chapter), actor);
	}

	public static int CalculateSkillCheckExperience(SkillCheckDifficulty difficulty, int chapter)
	{
		return Settings.GetSkillCheckExperience(difficulty, chapter);
	}

	public static int CalculateCustomSkillCheckExperience(int dc, int? overrideCR = null)
	{
		return CalculateCustomSkillCheckExperience(dc, Game.Instance.Player.Chapter, overrideCR);
	}

	public static int CalculateCustomSkillCheckExperience(int dc, int chapter, int? overrideCR = null)
	{
		return CalculateSkillCheckExperience(ConvertDCToSkillCheckDifficulty(dc, overrideCR), chapter);
	}

	public static int Calculate([NotNull] IExperienceSettings experience, BaseUnitEntity mob = null)
	{
		ExperienceType type = experience.Type;
		int? overrideCR = experience.OverrideCR;
		UnitDifficultyType? difficulty = mob?.Blueprint.DifficultyType;
		return Calculate(type, overrideCR, null, difficulty);
	}

	public static int Calculate(ExperienceType type, int? overrideCR = null, int? skillCheckDifficulty = null, UnitDifficultyType? difficulty = null)
	{
		int cr = overrideCR ?? CurrentAreaCR;
		int baseCRExperience = Settings.GetBaseCRExperience(cr);
		float experienceFactorForType = Settings.GetExperienceFactorForType(type);
		float num = ((type == ExperienceType.Encounter && difficulty.HasValue) ? Settings.GetExperienceFactorForEncounterDifficulty(difficulty.Value) : 1f);
		float num2 = ((type == ExperienceType.SkillCheck && skillCheckDifficulty.HasValue) ? Settings.GetExperienceFactorSkillCheckDifficulty(skillCheckDifficulty.Value) : 1f);
		float f = (float)baseCRExperience * experienceFactorForType * num * num2;
		return Math.Max(0, Mathf.CeilToInt(f));
	}

	private static void TryGainInternal(BlueprintScriptableObject blueprint, MechanicEntity actor = null)
	{
		ExperienceComponent component = blueprint.GetComponent<ExperienceComponent>();
		if (component != null)
		{
			Gain(component, actor);
		}
	}

	private static SkillCheckDifficulty ConvertDCToSkillCheckDifficulty(int dc, int? overrideCR = null)
	{
		int cr = overrideCR ?? CurrentAreaCR;
		SkillCheckDifficulty result = StandardSkillCheckDifficulties[0];
		int num = int.MaxValue;
		SkillCheckDifficulty[] standardSkillCheckDifficulties = StandardSkillCheckDifficulties;
		foreach (SkillCheckDifficulty skillCheckDifficulty in standardSkillCheckDifficulties)
		{
			int num2 = Math.Abs(ConfigRoot.Instance.SkillCheckRoot.GetSkillCheckDC(skillCheckDifficulty, cr) - dc);
			if (num2 < num)
			{
				num = num2;
				result = skillCheckDifficulty;
			}
		}
		return result;
	}

	private static void Gain(int experience, MechanicEntity actor = null, bool isForEncounter = false)
	{
		Game.Instance.Player.GainPartyExperience(experience, isForEncounter);
	}
}
