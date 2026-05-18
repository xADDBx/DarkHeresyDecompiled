using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
	public EnemyDifficultyOption EnemyDurability;

	[JsonProperty]
	[OwlPackInclude]
	public EnemyDifficultyOption EnemyDamage;

	[JsonProperty]
	[OwlPackInclude]
	public int SkillCheckModifier;

	[JsonProperty]
	[OwlPackInclude]
	public int EnemyMovementPoints;

	[JsonProperty]
	[OwlPackInclude]
	public int EnemyDamageModifier;

	[JsonProperty]
	[OwlPackInclude]
	public int PartyDamageModifier;

	[JsonProperty]
	[OwlPackInclude]
	public int EnemyDodgeModifier;

	[JsonProperty]
	[OwlPackInclude]
	public int EnemySkillModifier;

	[JsonProperty]
	[OwlPackInclude]
	public int PartyPositiveMoraleChangeModifier;

	[JsonProperty]
	[OwlPackInclude]
	public int PartyNegativeMoraleChangeModifier;

	[JsonProperty]
	[OwlPackInclude]
	public int EnemyPositiveMoraleChangeModifier;

	[JsonProperty]
	[OwlPackInclude]
	public int EnemyNegativeMoraleChangeModifier;

	[JsonProperty]
	[OwlPackInclude]
	public int AllyResolveModifier;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "DifficultyPreset",
		OldNames = null,
		Fields = new FieldInfo[17]
		{
			new FieldInfo("GameDifficulty", typeof(GameDifficultyOption)),
			new FieldInfo("OnlyOneSave", typeof(bool)),
			new FieldInfo("RespecAllowed", typeof(bool)),
			new FieldInfo("CombatEncountersCapacity", typeof(CombatEncountersCapacity)),
			new FieldInfo("EnemyDurability", typeof(EnemyDifficultyOption)),
			new FieldInfo("EnemyDamage", typeof(EnemyDifficultyOption)),
			new FieldInfo("SkillCheckModifier", typeof(int)),
			new FieldInfo("EnemyMovementPoints", typeof(int)),
			new FieldInfo("EnemyDamageModifier", typeof(int)),
			new FieldInfo("PartyDamageModifier", typeof(int)),
			new FieldInfo("EnemyDodgeModifier", typeof(int)),
			new FieldInfo("EnemySkillModifier", typeof(int)),
			new FieldInfo("PartyPositiveMoraleChangeModifier", typeof(int)),
			new FieldInfo("PartyNegativeMoraleChangeModifier", typeof(int)),
			new FieldInfo("EnemyPositiveMoraleChangeModifier", typeof(int)),
			new FieldInfo("EnemyNegativeMoraleChangeModifier", typeof(int)),
			new FieldInfo("AllyResolveModifier", typeof(int))
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
			EnemyDurability = EnemyDurability,
			EnemyDamage = EnemyDamage,
			SkillCheckModifier = SkillCheckModifier,
			EnemyMovementPoints = EnemyMovementPoints,
			EnemyDamageModifier = EnemyDamageModifier,
			PartyDamageModifier = PartyDamageModifier,
			EnemyDodgeModifier = EnemyDodgeModifier,
			EnemySkillModifier = EnemySkillModifier,
			PartyPositiveMoraleChangeModifier = PartyPositiveMoraleChangeModifier,
			PartyNegativeMoraleChangeModifier = PartyNegativeMoraleChangeModifier,
			EnemyPositiveMoraleChangeModifier = EnemyPositiveMoraleChangeModifier,
			EnemyNegativeMoraleChangeModifier = EnemyNegativeMoraleChangeModifier,
			AllyResolveModifier = AllyResolveModifier
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
		int num5 = EnemyDurability.CompareTo(other.EnemyDurability);
		if (num5 < 0)
		{
			return -1;
		}
		int num6 = EnemyDamage.CompareTo(other.EnemyDamage);
		if (num6 < 0)
		{
			return -1;
		}
		int num7 = -SkillCheckModifier.CompareTo(other.SkillCheckModifier);
		if (num7 < 0)
		{
			return -1;
		}
		int num8 = EnemyMovementPoints.CompareTo(other.EnemyMovementPoints);
		if (num8 < 0)
		{
			return -1;
		}
		int num9 = EnemyDamageModifier.CompareTo(other.EnemyDamageModifier);
		if (num9 < 0)
		{
			return -1;
		}
		int num10 = -PartyDamageModifier.CompareTo(other.PartyDamageModifier);
		if (num10 < 0)
		{
			return -1;
		}
		int num11 = EnemyDodgeModifier.CompareTo(other.EnemyDodgeModifier);
		if (num11 < 0)
		{
			return -1;
		}
		int num12 = EnemySkillModifier.CompareTo(other.EnemySkillModifier);
		if (num12 < 0)
		{
			return -1;
		}
		int num13 = -PartyPositiveMoraleChangeModifier.CompareTo(other.PartyPositiveMoraleChangeModifier);
		if (num13 < 0)
		{
			return -1;
		}
		int num14 = -PartyNegativeMoraleChangeModifier.CompareTo(other.PartyNegativeMoraleChangeModifier);
		if (num14 < 0)
		{
			return -1;
		}
		int num15 = EnemyPositiveMoraleChangeModifier.CompareTo(other.EnemyPositiveMoraleChangeModifier);
		if (num15 < 0)
		{
			return -1;
		}
		int num16 = EnemyNegativeMoraleChangeModifier.CompareTo(other.EnemyNegativeMoraleChangeModifier);
		if (num16 < 0)
		{
			return -1;
		}
		int num17 = -AllyResolveModifier.CompareTo(other.AllyResolveModifier);
		if (num17 < 0)
		{
			return -1;
		}
		if (num2 <= 0 && num3 <= 0 && num4 <= 0 && num5 <= 0 && num6 <= 0 && num7 <= 0 && num8 <= 0 && num9 <= 0 && num10 <= 0 && num11 <= 0 && num12 <= 0 && num13 <= 0 && num14 <= 0 && num15 <= 0 && num16 <= 0 && num17 <= 0)
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
		result.Append(ref EnemyDurability);
		result.Append(ref EnemyDamage);
		result.Append(ref SkillCheckModifier);
		result.Append(ref EnemyMovementPoints);
		result.Append(ref EnemyDamageModifier);
		result.Append(ref PartyDamageModifier);
		result.Append(ref EnemyDodgeModifier);
		result.Append(ref EnemySkillModifier);
		result.Append(ref PartyPositiveMoraleChangeModifier);
		result.Append(ref PartyNegativeMoraleChangeModifier);
		result.Append(ref EnemyPositiveMoraleChangeModifier);
		result.Append(ref EnemyNegativeMoraleChangeModifier);
		result.Append(ref AllyResolveModifier);
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
		formatter.EnumField(4, "EnemyDurability", ref EnemyDurability, state);
		formatter.EnumField(5, "EnemyDamage", ref EnemyDamage, state);
		formatter.UnmanagedField(6, "SkillCheckModifier", ref SkillCheckModifier, state);
		formatter.UnmanagedField(7, "EnemyMovementPoints", ref EnemyMovementPoints, state);
		formatter.UnmanagedField(8, "EnemyDamageModifier", ref EnemyDamageModifier, state);
		formatter.UnmanagedField(9, "PartyDamageModifier", ref PartyDamageModifier, state);
		formatter.UnmanagedField(10, "EnemyDodgeModifier", ref EnemyDodgeModifier, state);
		formatter.UnmanagedField(11, "EnemySkillModifier", ref EnemySkillModifier, state);
		formatter.UnmanagedField(12, "PartyPositiveMoraleChangeModifier", ref PartyPositiveMoraleChangeModifier, state);
		formatter.UnmanagedField(13, "PartyNegativeMoraleChangeModifier", ref PartyNegativeMoraleChangeModifier, state);
		formatter.UnmanagedField(14, "EnemyPositiveMoraleChangeModifier", ref EnemyPositiveMoraleChangeModifier, state);
		formatter.UnmanagedField(15, "EnemyNegativeMoraleChangeModifier", ref EnemyNegativeMoraleChangeModifier, state);
		formatter.UnmanagedField(16, "AllyResolveModifier", ref AllyResolveModifier, state);
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
				EnemyDurability = formatter.ReadEnum<EnemyDifficultyOption>(state);
				break;
			case 5:
				EnemyDamage = formatter.ReadEnum<EnemyDifficultyOption>(state);
				break;
			case 6:
				SkillCheckModifier = formatter.ReadUnmanaged<int>(state);
				break;
			case 7:
				EnemyMovementPoints = formatter.ReadUnmanaged<int>(state);
				break;
			case 8:
				EnemyDamageModifier = formatter.ReadUnmanaged<int>(state);
				break;
			case 9:
				PartyDamageModifier = formatter.ReadUnmanaged<int>(state);
				break;
			case 10:
				EnemyDodgeModifier = formatter.ReadUnmanaged<int>(state);
				break;
			case 11:
				EnemySkillModifier = formatter.ReadUnmanaged<int>(state);
				break;
			case 12:
				PartyPositiveMoraleChangeModifier = formatter.ReadUnmanaged<int>(state);
				break;
			case 13:
				PartyNegativeMoraleChangeModifier = formatter.ReadUnmanaged<int>(state);
				break;
			case 14:
				EnemyPositiveMoraleChangeModifier = formatter.ReadUnmanaged<int>(state);
				break;
			case 15:
				EnemyNegativeMoraleChangeModifier = formatter.ReadUnmanaged<int>(state);
				break;
			case 16:
				AllyResolveModifier = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
