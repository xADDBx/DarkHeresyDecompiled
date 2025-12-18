using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Enums;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Settings.Difficulty;

[Serializable]
[OwlPackable(OwlPackableMode.Generate)]
public class DifficultyPreset : IComparable<DifficultyPreset>, IHashable, IOwlPackable, IOwlPackable<DifficultyPreset>
{
	[JsonProperty]
	[OwlPackInclude]
	public GameDifficultyOption GameDifficulty;

	[JsonProperty]
	[OwlPackInclude]
	public bool OnlyOneSave;

	[JsonProperty]
	[OwlPackInclude]
	public bool RespecAllowed;

	[JsonProperty]
	[OwlPackInclude]
	public CombatEncountersCapacity CombatEncountersCapacity;

	[JsonProperty]
	[OwlPackInclude]
	public int EnemyDodgePercentModifier;

	[JsonProperty]
	[OwlPackInclude]
	public int MinPartyDamage;

	[JsonProperty]
	[OwlPackInclude]
	public int MinPartyDamageFraction;

	[JsonProperty]
	[OwlPackInclude]
	public int NPCAttributesBaseValuePercentModifier;

	[JsonProperty]
	[OwlPackInclude]
	public HardCrowdControlDurationLimit HardCrowdControlOnPartyMaxDurationRounds = HardCrowdControlDurationLimit.Unlimited;

	[JsonProperty]
	[OwlPackInclude]
	public int SkillCheckModifier;

	[JsonProperty]
	[OwlPackInclude]
	public int EnemyHitPointsPercentModifier;

	[JsonProperty]
	[OwlPackInclude]
	public int AllyResolveModifier;

	[JsonProperty]
	[OwlPackInclude]
	public int PartyDamageDealtAfterArmorReductionPercentModifier;

	[JsonProperty]
	[OwlPackInclude]
	public int EnemyMovementPoints;

	[JsonProperty]
	[OwlPackInclude]
	public int AvoidableDamagePercentModifier;

	[JsonProperty]
	[OwlPackInclude]
	public NPCDifficultyOption NPCDifficulty;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "DifficultyPreset",
		OldNames = null,
		Fields = new FieldInfo[16]
		{
			new FieldInfo("GameDifficulty", typeof(GameDifficultyOption)),
			new FieldInfo("OnlyOneSave", typeof(bool)),
			new FieldInfo("RespecAllowed", typeof(bool)),
			new FieldInfo("CombatEncountersCapacity", typeof(CombatEncountersCapacity)),
			new FieldInfo("EnemyDodgePercentModifier", typeof(int)),
			new FieldInfo("MinPartyDamage", typeof(int)),
			new FieldInfo("MinPartyDamageFraction", typeof(int)),
			new FieldInfo("NPCAttributesBaseValuePercentModifier", typeof(int)),
			new FieldInfo("HardCrowdControlOnPartyMaxDurationRounds", typeof(HardCrowdControlDurationLimit)),
			new FieldInfo("SkillCheckModifier", typeof(int)),
			new FieldInfo("EnemyHitPointsPercentModifier", typeof(int)),
			new FieldInfo("AllyResolveModifier", typeof(int)),
			new FieldInfo("PartyDamageDealtAfterArmorReductionPercentModifier", typeof(int)),
			new FieldInfo("EnemyMovementPoints", typeof(int)),
			new FieldInfo("AvoidableDamagePercentModifier", typeof(int)),
			new FieldInfo("NPCDifficulty", typeof(NPCDifficultyOption))
		}
	};

	public DifficultyPreset Copy()
	{
		return new DifficultyPreset
		{
			GameDifficulty = GameDifficulty,
			OnlyOneSave = OnlyOneSave,
			RespecAllowed = RespecAllowed,
			CombatEncountersCapacity = CombatEncountersCapacity,
			EnemyDodgePercentModifier = EnemyDodgePercentModifier,
			MinPartyDamage = MinPartyDamage,
			MinPartyDamageFraction = MinPartyDamageFraction,
			NPCAttributesBaseValuePercentModifier = NPCAttributesBaseValuePercentModifier,
			HardCrowdControlOnPartyMaxDurationRounds = HardCrowdControlOnPartyMaxDurationRounds,
			SkillCheckModifier = SkillCheckModifier,
			EnemyHitPointsPercentModifier = EnemyHitPointsPercentModifier,
			AllyResolveModifier = AllyResolveModifier,
			PartyDamageDealtAfterArmorReductionPercentModifier = PartyDamageDealtAfterArmorReductionPercentModifier,
			EnemyMovementPoints = EnemyMovementPoints,
			AvoidableDamagePercentModifier = AvoidableDamagePercentModifier,
			NPCDifficulty = NPCDifficulty
		};
	}

	public int CompareTo(DifficultyPreset other)
	{
		if (this == other)
		{
			return 0;
		}
		if (other == null)
		{
			return 1;
		}
		if (other.GameDifficulty != GameDifficultyOption.Custom && GameDifficulty != GameDifficultyOption.Custom)
		{
			int num = GameDifficulty.CompareTo(other.GameDifficulty);
			if (num != 0)
			{
				return num;
			}
		}
		int num2 = -OnlyOneSave.CompareTo(other.OnlyOneSave);
		if (num2 < 0)
		{
			return -1;
		}
		int num3 = CombatEncountersCapacity.CompareTo(other.CombatEncountersCapacity);
		if (num3 < 0)
		{
			return -1;
		}
		int num4 = -RespecAllowed.CompareTo(other.RespecAllowed);
		if (num4 < 0)
		{
			return -1;
		}
		int num5 = EnemyDodgePercentModifier.CompareTo(other.EnemyDodgePercentModifier);
		if (num5 < 0)
		{
			return -1;
		}
		int num6 = -MinPartyDamage.CompareTo(other.MinPartyDamage);
		if (num6 < 0)
		{
			return -1;
		}
		int num7 = -MinPartyDamageFraction.CompareTo(other.MinPartyDamageFraction);
		if (num7 < 0)
		{
			return -1;
		}
		int num8 = NPCAttributesBaseValuePercentModifier.CompareTo(other.NPCAttributesBaseValuePercentModifier);
		if (num8 < 0)
		{
			return -1;
		}
		int num9 = HardCrowdControlOnPartyMaxDurationRounds.CompareTo(other.HardCrowdControlOnPartyMaxDurationRounds);
		if (num9 < 0)
		{
			return -1;
		}
		int num10 = SkillCheckModifier.CompareTo(other.SkillCheckModifier);
		if (num10 < 0)
		{
			return -1;
		}
		int num11 = -EnemyHitPointsPercentModifier.CompareTo(other.EnemyHitPointsPercentModifier);
		if (num11 < 0)
		{
			return -1;
		}
		int num12 = AllyResolveModifier.CompareTo(other.AllyResolveModifier);
		if (num12 < 0)
		{
			return -1;
		}
		int num13 = -PartyDamageDealtAfterArmorReductionPercentModifier.CompareTo(other.PartyDamageDealtAfterArmorReductionPercentModifier);
		if (num13 < 0)
		{
			return -1;
		}
		int num14 = AvoidableDamagePercentModifier.CompareTo(other.AvoidableDamagePercentModifier);
		if (num14 < 0)
		{
			return -1;
		}
		int num15 = EnemyMovementPoints.CompareTo(other.EnemyMovementPoints);
		if (num15 < 0)
		{
			return -1;
		}
		int num16 = NPCDifficulty.CompareTo(other.NPCDifficulty);
		if (num16 < 0)
		{
			return -1;
		}
		if (num2 <= 0 && num3 <= 0 && num4 <= 0 && num5 <= 0 && num6 <= 0 && num7 <= 0 && num8 <= 0 && num9 <= 0 && num10 <= 0 && num11 <= 0 && num12 <= 0 && num13 <= 0 && num14 <= 0 && num15 <= 0 && num16 <= 0)
		{
			return 0;
		}
		return 1;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref GameDifficulty);
		result.Append(ref OnlyOneSave);
		result.Append(ref RespecAllowed);
		result.Append(ref CombatEncountersCapacity);
		result.Append(ref EnemyDodgePercentModifier);
		result.Append(ref MinPartyDamage);
		result.Append(ref MinPartyDamageFraction);
		result.Append(ref NPCAttributesBaseValuePercentModifier);
		result.Append(ref HardCrowdControlOnPartyMaxDurationRounds);
		result.Append(ref SkillCheckModifier);
		result.Append(ref EnemyHitPointsPercentModifier);
		result.Append(ref AllyResolveModifier);
		result.Append(ref PartyDamageDealtAfterArmorReductionPercentModifier);
		result.Append(ref EnemyMovementPoints);
		result.Append(ref AvoidableDamagePercentModifier);
		result.Append(ref NPCDifficulty);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		DifficultyPreset source = new DifficultyPreset();
		result = Unsafe.As<DifficultyPreset, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<DifficultyPreset>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EnumField(0, "GameDifficulty", ref GameDifficulty, state);
		formatter.UnmanagedField(1, "OnlyOneSave", ref OnlyOneSave, state);
		formatter.UnmanagedField(2, "RespecAllowed", ref RespecAllowed, state);
		formatter.EnumField(3, "CombatEncountersCapacity", ref CombatEncountersCapacity, state);
		formatter.UnmanagedField(4, "EnemyDodgePercentModifier", ref EnemyDodgePercentModifier, state);
		formatter.UnmanagedField(5, "MinPartyDamage", ref MinPartyDamage, state);
		formatter.UnmanagedField(6, "MinPartyDamageFraction", ref MinPartyDamageFraction, state);
		formatter.UnmanagedField(7, "NPCAttributesBaseValuePercentModifier", ref NPCAttributesBaseValuePercentModifier, state);
		formatter.EnumField(8, "HardCrowdControlOnPartyMaxDurationRounds", ref HardCrowdControlOnPartyMaxDurationRounds, state);
		formatter.UnmanagedField(9, "SkillCheckModifier", ref SkillCheckModifier, state);
		formatter.UnmanagedField(10, "EnemyHitPointsPercentModifier", ref EnemyHitPointsPercentModifier, state);
		formatter.UnmanagedField(11, "AllyResolveModifier", ref AllyResolveModifier, state);
		formatter.UnmanagedField(12, "PartyDamageDealtAfterArmorReductionPercentModifier", ref PartyDamageDealtAfterArmorReductionPercentModifier, state);
		formatter.UnmanagedField(13, "EnemyMovementPoints", ref EnemyMovementPoints, state);
		formatter.UnmanagedField(14, "AvoidableDamagePercentModifier", ref AvoidableDamagePercentModifier, state);
		formatter.EnumField(15, "NPCDifficulty", ref NPCDifficulty, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DifficultyPreset>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				GameDifficulty = formatter.ReadEnum<GameDifficultyOption>(state);
				break;
			case 1:
				OnlyOneSave = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				RespecAllowed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				CombatEncountersCapacity = formatter.ReadEnum<CombatEncountersCapacity>(state);
				break;
			case 4:
				EnemyDodgePercentModifier = formatter.ReadUnmanaged<int>(state);
				break;
			case 5:
				MinPartyDamage = formatter.ReadUnmanaged<int>(state);
				break;
			case 6:
				MinPartyDamageFraction = formatter.ReadUnmanaged<int>(state);
				break;
			case 7:
				NPCAttributesBaseValuePercentModifier = formatter.ReadUnmanaged<int>(state);
				break;
			case 8:
				HardCrowdControlOnPartyMaxDurationRounds = formatter.ReadEnum<HardCrowdControlDurationLimit>(state);
				break;
			case 9:
				SkillCheckModifier = formatter.ReadUnmanaged<int>(state);
				break;
			case 10:
				EnemyHitPointsPercentModifier = formatter.ReadUnmanaged<int>(state);
				break;
			case 11:
				AllyResolveModifier = formatter.ReadUnmanaged<int>(state);
				break;
			case 12:
				PartyDamageDealtAfterArmorReductionPercentModifier = formatter.ReadUnmanaged<int>(state);
				break;
			case 13:
				EnemyMovementPoints = formatter.ReadUnmanaged<int>(state);
				break;
			case 14:
				AvoidableDamagePercentModifier = formatter.ReadUnmanaged<int>(state);
				break;
			case 15:
				NPCDifficulty = formatter.ReadEnum<NPCDifficultyOption>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
