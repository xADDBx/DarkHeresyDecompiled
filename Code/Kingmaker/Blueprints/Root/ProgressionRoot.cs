using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Gameplay.Features.Experience;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[ComponentName("Root/ProgressionRoot")]
[TypeId("d938496aac277e841a349f8b552449fb")]
public class ProgressionRoot : BlueprintScriptableObject
{
	[Serializable]
	private sealed class TypeToExperienceFactor
	{
		public ExperienceType Type;

		public float ExperienceFactor;

		public TypeToExperienceFactor(ExperienceType type, float experienceFactor)
		{
			Type = type;
			ExperienceFactor = experienceFactor;
		}
	}

	[Serializable]
	private sealed class EncounterDifficultyToExperienceFactor
	{
		public UnitDifficultyType Type;

		public float ExperienceFactor;

		public EncounterDifficultyToExperienceFactor(UnitDifficultyType type, float experienceFactor)
		{
			Type = type;
			ExperienceFactor = experienceFactor;
		}
	}

	[Serializable]
	private sealed class SkillCheckDifficultyToExperienceFactor
	{
		[Range(-60f, 60f)]
		public int Difficulty;

		public float ExperienceFactor;

		public SkillCheckDifficultyToExperienceFactor(int difficulty, float experienceFactor)
		{
			Difficulty = difficulty;
			ExperienceFactor = experienceFactor;
		}
	}

	[SerializeField]
	[ValidateNotNull]
	private BpRef<BlueprintCareerPath>[] _careerPaths = new BpRef<BlueprintCareerPath>[0];

	[Tooltip("Индекс - Level; значение - экспа, требуемая для получения уровня")]
	[SerializeField]
	[ValidateNotNull]
	private BpRef<BlueprintStatProgression> _experienceTable = new BpRef<BlueprintStatProgression>();

	[Tooltip("Индекс - CR; значение - базовая экспа за CR")]
	[SerializeField]
	[ValidateNotNull]
	private BpRef<BlueprintStatProgression> _crTable = new BpRef<BlueprintStatProgression>();

	[SerializeField]
	private TypeToExperienceFactor[] _typeFactors = new TypeToExperienceFactor[5]
	{
		new TypeToExperienceFactor(ExperienceType.Encounter, 1f),
		new TypeToExperienceFactor(ExperienceType.SkillCheck, 1f),
		new TypeToExperienceFactor(ExperienceType.Quest, 1f),
		new TypeToExperienceFactor(ExperienceType.MainQuest, 1f),
		new TypeToExperienceFactor(ExperienceType.Investigation, 1f)
	};

	[SerializeField]
	private EncounterDifficultyToExperienceFactor[] _encounterDifficultyFactors = new EncounterDifficultyToExperienceFactor[4]
	{
		new EncounterDifficultyToExperienceFactor(UnitDifficultyType.Swarm, 1f),
		new EncounterDifficultyToExperienceFactor(UnitDifficultyType.Common, 1f),
		new EncounterDifficultyToExperienceFactor(UnitDifficultyType.Elite, 1f),
		new EncounterDifficultyToExperienceFactor(UnitDifficultyType.Boss, 1f)
	};

	[SerializeField]
	private SkillCheckDifficultyToExperienceFactor[] _skillCheckDifficultyFactors = new SkillCheckDifficultyToExperienceFactor[13]
	{
		new SkillCheckDifficultyToExperienceFactor(60, 1f),
		new SkillCheckDifficultyToExperienceFactor(50, 1f),
		new SkillCheckDifficultyToExperienceFactor(40, 1f),
		new SkillCheckDifficultyToExperienceFactor(30, 1f),
		new SkillCheckDifficultyToExperienceFactor(20, 1f),
		new SkillCheckDifficultyToExperienceFactor(10, 1f),
		new SkillCheckDifficultyToExperienceFactor(0, 1f),
		new SkillCheckDifficultyToExperienceFactor(-10, 1f),
		new SkillCheckDifficultyToExperienceFactor(-20, 1f),
		new SkillCheckDifficultyToExperienceFactor(-30, 1f),
		new SkillCheckDifficultyToExperienceFactor(-40, 1f),
		new SkillCheckDifficultyToExperienceFactor(-50, 1f),
		new SkillCheckDifficultyToExperienceFactor(-60, 1f)
	};

	public static ProgressionRoot Instance => ConfigRoot.Instance.Progression;

	public IEnumerable<BlueprintCareerPath> CareerPaths => from i in _careerPaths.Dereference()
		where i?.IsAvailable ?? false
		select i;

	public BlueprintStatProgression ExperienceTable => _experienceTable;

	public BlueprintStatProgression CRTable => _crTable;

	public int GetBaseCRExperience(int cr)
	{
		return CRTable.GetBonus(cr);
	}

	public float GetExperienceFactorForType(ExperienceType type)
	{
		return _typeFactors.FirstItem((TypeToExperienceFactor f) => f.Type == type)?.ExperienceFactor ?? 1f;
	}

	public float GetExperienceFactorForEncounterDifficulty(UnitDifficultyType type)
	{
		return _encounterDifficultyFactors.FirstItem((EncounterDifficultyToExperienceFactor f) => f.Type == type)?.ExperienceFactor ?? 1f;
	}

	public float GetExperienceFactorSkillCheckDifficulty(int difficulty)
	{
		return _skillCheckDifficultyFactors.FirstItem((SkillCheckDifficultyToExperienceFactor f) => f.Difficulty <= difficulty)?.ExperienceFactor ?? _skillCheckDifficultyFactors.LastItem()?.ExperienceFactor ?? 1f;
	}
}
