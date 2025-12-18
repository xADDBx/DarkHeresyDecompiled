using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Root;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Achievements;

[OwlPackable(OwlPackableMode.Generate)]
public class AchievementEntity : IHashable, IOwlPackable, IOwlPackable<AchievementEntity>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "AchievementEntity",
		OldNames = null,
		Fields = new FieldInfo[4]
		{
			new FieldInfo("Data", typeof(AchievementData)),
			new FieldInfo("IsUnlocked", typeof(bool)),
			new FieldInfo("NeedCommit", typeof(bool)),
			new FieldInfo("Counter", typeof(int))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public AchievementData Data { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool IsUnlocked { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool NeedCommit { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public int Counter { get; private set; }

	public AchievementsManager Manager { get; set; }

	public bool IsDisabled
	{
		get
		{
			if (Data.OnlyMainCampaign && (bool)Game.Instance.Player.Campaign && !Game.Instance.Player.Campaign.IsMainGameContent)
			{
				return true;
			}
			BlueprintCampaign blueprintCampaign = Data.SpecificCampaign?.Get();
			if (!Data.OnlyMainCampaign && blueprintCampaign != null && Game.Instance.Player.Campaign != blueprintCampaign)
			{
				return true;
			}
			if (Game.Instance.Player.ModsUser)
			{
				return true;
			}
			return false;
		}
	}

	public bool HasCounter => Data.EventsCountForUnlock > 1;

	[JsonConstructor]
	public AchievementEntity(AchievementData data)
	{
		Data = data;
	}

	public AchievementEntity()
	{
	}

	public void OnSynchronized(bool unlocked)
	{
		IsUnlocked |= unlocked;
		if (unlocked)
		{
			NeedCommit = false;
			Manager.OnAchievementUnlocked(this);
		}
	}

	public void SynchronizeCounter(int progressValue)
	{
		if (!IsDisabled && !IsUnlocked && HasCounter)
		{
			int counter = Counter;
			Counter = Math.Max(counter, progressValue);
			if (progressValue < counter)
			{
				NeedCommit = true;
				Manager.OnAchievementProgressUpdated(this);
			}
		}
	}

	public void OnCommited()
	{
		NeedCommit = false;
	}

	public void Unlock()
	{
		if (!IsDisabled && !IsUnlocked)
		{
			IsUnlocked = true;
			NeedCommit = true;
			Manager.OnAchievementUnlocked(this);
		}
	}

	public void IncrementCounter()
	{
		if (IsDisabled || IsUnlocked)
		{
			return;
		}
		if (!HasCounter)
		{
			PFLog.Default.Error("Can't increment counter for achievement with EventsCountForUnlock < 2 (use Unlock instead)");
			return;
		}
		Counter++;
		NeedCommit = true;
		Manager.OnAchievementProgressUpdated(this);
		if (Counter >= Data.EventsCountForUnlock)
		{
			Unlock();
		}
	}

	public override string ToString()
	{
		if (!HasCounter)
		{
			return Data.name;
		}
		return $"{Data.name} ({Counter}/{Data.EventsCountForUnlock})";
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = SimpleBlueprintHasher.GetHash128(Data);
		result.Append(ref val);
		bool val2 = IsUnlocked;
		result.Append(ref val2);
		bool val3 = NeedCommit;
		result.Append(ref val3);
		int val4 = Counter;
		result.Append(ref val4);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		AchievementEntity source = new AchievementEntity();
		result = Unsafe.As<AchievementEntity, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AchievementEntity>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		AchievementData value = Data;
		formatter.Field(0, "Data", ref value, state);
		bool value2 = IsUnlocked;
		formatter.UnmanagedField(1, "IsUnlocked", ref value2, state);
		bool value3 = NeedCommit;
		formatter.UnmanagedField(2, "NeedCommit", ref value3, state);
		int value4 = Counter;
		formatter.UnmanagedField(3, "Counter", ref value4, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AchievementEntity>();
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
				Data = formatter.ReadPackable<AchievementData>(state);
				break;
			case 1:
				IsUnlocked = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				NeedCommit = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				Counter = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
