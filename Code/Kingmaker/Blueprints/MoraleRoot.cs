using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.Gameplay.Features.Experience;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[ComponentName("Root/MoraleRoot")]
[TypeId("43c91d03b92e48f7908dbf28220864c6")]
public sealed class MoraleRoot : BlueprintScriptableObject
{
	[Serializable]
	public class DifficultyListItem
	{
		public UnitDifficultyType Type;

		public int Value;
	}

	[Obsolete]
	public enum PowerBalanceModeType
	{
		CurrentHealthPercent = 1,
		CurrentMorale,
		HealthAndMorale
	}

	public int MinValue = -10;

	public int MaxValue = 10;

	public ModifiableByDifficultyParameter RegenAtTurnStart = new ModifiableByDifficultyParameter();

	public ModifiableByDifficultyParameter AllyLeaderDeathPenalty = new ModifiableByDifficultyParameter();

	public ModifiableByDifficultyParameter EnemyLeaderDeathBonus = new ModifiableByDifficultyParameter();

	public ModifiableByDifficultyParameter EnemyLeaderDeadAtTurnStartBonus = new ModifiableByDifficultyParameter();

	public DifficultyTypeToMoraleChange[] UnitDeathMoraleChange = new DifficultyTypeToMoraleChange[0];

	public int MoralLockRoundsLength = 1;

	public int MaxDistanceInCellsToMoraleAffect = 1;

	[Obsolete]
	public PowerBalanceModeType PowerBalanceMode = PowerBalanceModeType.CurrentHealthPercent;

	public int PowerBalanceHealthBase = 50;

	public int PowerBalanceMoraleBase = 20;

	public float ShatteredPowerBalanceMultiplier = 4f;

	public float LosingBattlePowerBalanceMultiplier = 2f;

	public BpRef<BlueprintBuff> LeaderBuff;

	public BpRef<BlueprintBuff> HeroicBuff;

	public BpRef<BlueprintBuff> BrokenBuff;

	public BpRef<BlueprintBuff> LosingBattleBuff;

	public BpRef<BlueprintBuff> ShatteredBuff;

	public BpRef<BlueprintBuff> BetrayalBuff;

	public BpRef<BlueprintBuff> MoralePlusBuff;

	public BpRef<BlueprintUnitFact> MoralePlusFact;

	public BrokenBuffsCollection BrokenBuffsCollection = new BrokenBuffsCollection();

	[SerializeField]
	private List<DifficultyListItem> PowerBalanceCoefficients = new List<DifficultyListItem>();

	private Dictionary<UnitDifficultyType, int> PowerBalanceCoefficientsMap = new Dictionary<UnitDifficultyType, int>();

	public int TraumaStackTriggersMoraleDrop = 3;

	public int MoraleAddOnTraumaStack = -4;

	[InfoBox("Эти экшены выполняются когда юнит получает максимальный уровень крита в любую часть тела. Caster - тот, из-за кого юнит получил крит; Target - юнит, получивший крит.")]
	public ActionList MaxCriticalStageActions = new ActionList();

	public static MoraleRoot Instance => ConfigRoot.Instance.MoraleRoot;

	public int GetUnitDifficulty(UnitDifficultyType type)
	{
		if (PowerBalanceCoefficientsMap.Count == 0)
		{
			PowerBalanceCoefficientsMap = PowerBalanceCoefficients.ToDictionary((DifficultyListItem x) => x.Type, (DifficultyListItem x) => x.Value);
		}
		int value = 0;
		PowerBalanceCoefficientsMap.TryGetValue(type, out value);
		return value;
	}

	public bool TryGetBuffForDifficulty(UnitDifficultyType type, out BpRef<BlueprintBuff> buff)
	{
		return BrokenBuffsCollection.TryGetBuffForDifficulty(type, out buff);
	}

	[CanBeNull]
	public BlueprintBuff GetBuffForDifficulty(UnitDifficultyType type)
	{
		BpRef<BlueprintBuff> buff;
		return BrokenBuffsCollection.TryGetBuffForDifficulty(type, out buff) ? buff : null;
	}

	private int GetUnitDeathMoraleChange(UnitDifficultyType difficultyType, bool isEnemy, bool isLeader)
	{
		return ((!isEnemy) ? UnitDeathMoraleChange.FirstItem((DifficultyTypeToMoraleChange i) => i.DifficultyType == difficultyType)?.AllyDeath : UnitDeathMoraleChange.FirstItem((DifficultyTypeToMoraleChange i) => i.DifficultyType == difficultyType)?.EnemyDeath)?.GetValue() ?? 0;
	}

	public int GetEnemyDeathMoraleChange(UnitDifficultyType difficultyType, bool isLeader)
	{
		return GetUnitDeathMoraleChange(difficultyType, isEnemy: true, isLeader);
	}

	public int GetAllyDeathMoraleChange(UnitDifficultyType difficultyType, bool isLeader)
	{
		return GetUnitDeathMoraleChange(difficultyType, isEnemy: false, isLeader);
	}
}
