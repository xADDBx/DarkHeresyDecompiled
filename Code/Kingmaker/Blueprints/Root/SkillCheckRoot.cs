using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[ComponentName("Root/SkillCheckRoot")]
[TypeId("eb0f154cbe024b708d154713d974f2f0")]
public class SkillCheckRoot : BlueprintScriptableObject
{
	[Serializable]
	public class SkillCheckDifficultyEntry
	{
		[SkillCheckActualDifficulty]
		public SkillCheckDifficulty Difficulty;

		public int[] CR2DC = new int[0];
	}

	[Serializable]
	public class CRAndDamagePair
	{
		[ArrayElementNameProvider]
		public int CR;

		[SerializeField]
		public ContextValue Damage;
	}

	public SkillCheckDifficultyEntry[] SkillCheckDifficulty = new SkillCheckDifficultyEntry[4]
	{
		new SkillCheckDifficultyEntry
		{
			Difficulty = Kingmaker.View.MapObjects.SkillCheckDifficulty.Easy
		},
		new SkillCheckDifficultyEntry
		{
			Difficulty = Kingmaker.View.MapObjects.SkillCheckDifficulty.Normal
		},
		new SkillCheckDifficultyEntry
		{
			Difficulty = Kingmaker.View.MapObjects.SkillCheckDifficulty.Hard
		},
		new SkillCheckDifficultyEntry
		{
			Difficulty = Kingmaker.View.MapObjects.SkillCheckDifficulty.Impossible
		}
	};

	public Dictionary<StatType, BodyPartTags> SkillToBodyPartCritMap = new Dictionary<StatType, BodyPartTags>
	{
		{
			StatType.SkillAthletics,
			BodyPartTags.Arms
		},
		{
			StatType.SkillMobility,
			BodyPartTags.Legs
		},
		{
			StatType.SkillResistance,
			BodyPartTags.Torso
		}
	};

	[ValidateNotEmpty]
	public CRAndDamagePair[] CRAndDamage = new CRAndDamagePair[0];

	[SerializeField]
	public BpRef<BlueprintBuff> Fatigued;

	[SerializeField]
	public BpRef<BlueprintBuff> Disturbed;

	[SerializeField]
	public BpRef<BlueprintBuff> Perplexed;

	[SerializeField]
	public BpRef<BlueprintBuff> MentalCriticalEffectBuff;

	public int GetSkillCheckDC(SkillCheckDifficulty difficulty, int cr)
	{
		if (difficulty == Kingmaker.View.MapObjects.SkillCheckDifficulty.Custom)
		{
			PFLog.Default.Error("Difficulty is Custom");
			return 0;
		}
		SkillCheckDifficultyEntry skillCheckDifficultyEntry = SkillCheckDifficulty.FirstItem((SkillCheckDifficultyEntry i) => i.Difficulty == difficulty);
		if (skillCheckDifficultyEntry == null || skillCheckDifficultyEntry.CR2DC.Empty())
		{
			PFLog.Default.Error($"Settings is missing for Difficulty == {difficulty}");
			return 0;
		}
		cr = Math.Clamp(cr, 0, skillCheckDifficultyEntry.CR2DC.Length - 1);
		return skillCheckDifficultyEntry.CR2DC[cr];
	}

	public void SetDebuffForFailedSkillCheck([CanBeNull] BaseUnitEntity user, StatType skill, bool isCriticalFail)
	{
		BlueprintBuff blueprintBuff = null;
		switch (skill)
		{
		case StatType.Strength:
		case StatType.Toughness:
		case StatType.Agility:
		case StatType.SkillAthletics:
		case StatType.SkillTenacity:
		case StatType.SkillMobility:
		case StatType.SkillResistance:
		case StatType.SkillDemolition:
		case StatType.SkillReflexes:
			blueprintBuff = Fatigued;
			break;
		case StatType.Willpower:
		case StatType.Fellowship:
		case StatType.SkillInterrogation:
		case StatType.SkillMettle:
		case StatType.SkillIntimidation:
		case StatType.SkillDiplomacy:
			blueprintBuff = Disturbed;
			break;
		case StatType.BallisticSkill:
		case StatType.WeaponSkill:
		case StatType.Intelligence:
		case StatType.Perception:
		case StatType.SkillSleightOfHand:
		case StatType.SkillLoreHeresy:
		case StatType.SkillLoreXenos:
		case StatType.SkillLoreWarp:
		case StatType.SkillTechUse:
		case StatType.SkillAwareness:
		case StatType.SkillWits:
		case StatType.SkillMedicae:
			blueprintBuff = Perplexed;
			break;
		}
		if (blueprintBuff != null && user != null)
		{
			user.Buffs.Add(blueprintBuff, user);
			if (isCriticalFail)
			{
				user.Buffs.Add(blueprintBuff, user);
			}
		}
	}
}
