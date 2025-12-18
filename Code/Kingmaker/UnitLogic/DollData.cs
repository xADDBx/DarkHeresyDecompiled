using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Levelup.CharGen;
using Kingmaker.View;
using Kingmaker.Visual.CharacterSystem;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic;

[OwlPackable(OwlPackableMode.Generate)]
public class DollData : IHashable, IOwlPackable, IOwlPackable<DollData>
{
	[JsonProperty]
	[OwlPackInclude]
	public Gender Gender;

	[CanBeNull]
	[JsonProperty]
	[OwlPackInclude]
	public BlueprintRaceVisualPreset RacePreset;

	[NotNull]
	[JsonProperty]
	[OwlPackInclude]
	public List<string> EquipmentEntityIds = new List<string>();

	[NotNull]
	[JsonProperty]
	[OwlPackInclude]
	public Dictionary<string, int> EntityRampIdices = new Dictionary<string, int>();

	[NotNull]
	[JsonProperty]
	[OwlPackInclude]
	public Dictionary<string, int> EntitySecondaryRampIdices = new Dictionary<string, int>();

	[JsonProperty]
	[OwlPackInclude]
	public bool LeftHanded;

	[JsonProperty]
	[OwlPackInclude]
	public int ClothesPrimaryIndex = -1;

	[JsonProperty]
	[OwlPackInclude]
	public int ClothesSecondaryIndex = -1;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "DollData",
		OldNames = null,
		Fields = new FieldInfo[8]
		{
			new FieldInfo("Gender", typeof(Gender)),
			new FieldInfo("RacePreset", typeof(BlueprintRaceVisualPreset)),
			new FieldInfo("EquipmentEntityIds", typeof(List<string>)),
			new FieldInfo("EntityRampIdices", typeof(Dictionary<string, int>)),
			new FieldInfo("EntitySecondaryRampIdices", typeof(Dictionary<string, int>)),
			new FieldInfo("LeftHanded", typeof(bool)),
			new FieldInfo("ClothesPrimaryIndex", typeof(int)),
			new FieldInfo("ClothesSecondaryIndex", typeof(int))
		}
	};

	[NotNull]
	public UnitEntityView CreateUnitView(bool savedEquipment = false)
	{
		BlueprintCharGenRoot charGenRoot = ConfigRoot.Instance.CharGenRoot;
		Character character = ((Gender == Gender.Male) ? charGenRoot.MaleDoll : charGenRoot.FemaleDoll);
		UnitEntityView component = character.GetComponent<UnitEntityView>();
		if (component == null)
		{
			throw new Exception($"Could not create unit view by doll data: invalid prefab {character}");
		}
		UnitEntityView unitEntityView = UnityEngine.Object.Instantiate(component);
		Character component2 = unitEntityView.GetComponent<Character>();
		if (component2 == null)
		{
			return unitEntityView;
		}
		component2.RemoveAllEquipmentEntities(savedEquipment);
		if (RacePreset != null)
		{
			component2.Skeleton = ((Gender == Gender.Male) ? RacePreset.MaleSkeleton : RacePreset.FemaleSkeleton);
			component2.AddEquipmentEntities(RacePreset.Skin.Load(Gender, RacePreset.RaceId), savedEquipment);
		}
		foreach (string equipmentEntityId in EquipmentEntityIds)
		{
			EquipmentEntity ee = ResourcesLibrary.TryGetResource<EquipmentEntity>(equipmentEntityId);
			component2.AddEquipmentEntity(ee, savedEquipment);
		}
		ApplyRampIndices(component2, savedEquipment);
		return unitEntityView;
	}

	public void ApplyRampIndices(Character avatar, bool savedEquipment = false)
	{
		foreach (KeyValuePair<string, int> entityRampIdix in EntityRampIdices)
		{
			EquipmentEntity ee = ResourcesLibrary.TryGetResource<EquipmentEntity>(entityRampIdix.Key);
			int value = entityRampIdix.Value;
			if (value >= 0)
			{
				avatar.SetPrimaryRampIndex(ee, value, savedEquipment);
			}
		}
		foreach (KeyValuePair<string, int> entitySecondaryRampIdix in EntitySecondaryRampIdices)
		{
			EquipmentEntity ee2 = ResourcesLibrary.TryGetResource<EquipmentEntity>(entitySecondaryRampIdix.Key);
			int value2 = entitySecondaryRampIdix.Value;
			if (value2 >= 0)
			{
				avatar.SetSecondaryRampIndex(ee2, value2, savedEquipment);
			}
		}
	}

	public void PreloadEquipmentEntities()
	{
		if (RacePreset != null)
		{
			RacePreset.Skin.Preload(Gender, RacePreset.RaceId);
		}
		foreach (string equipmentEntityId in EquipmentEntityIds)
		{
			ResourcesLibrary.PreloadResource<EquipmentEntity>(equipmentEntityId);
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref Gender);
		Hash128 val = SimpleBlueprintHasher.GetHash128(RacePreset);
		result.Append(ref val);
		List<string> equipmentEntityIds = EquipmentEntityIds;
		if (equipmentEntityIds != null)
		{
			for (int i = 0; i < equipmentEntityIds.Count; i++)
			{
				Hash128 val2 = StringHasher.GetHash128(equipmentEntityIds[i]);
				result.Append(ref val2);
			}
		}
		Dictionary<string, int> entityRampIdices = EntityRampIdices;
		if (entityRampIdices != null)
		{
			int val3 = 0;
			foreach (KeyValuePair<string, int> item in entityRampIdices)
			{
				Hash128 hash = default(Hash128);
				Hash128 val4 = StringHasher.GetHash128(item.Key);
				hash.Append(ref val4);
				int obj = item.Value;
				Hash128 val5 = UnmanagedHasher<int>.GetHash128(ref obj);
				hash.Append(ref val5);
				val3 ^= hash.GetHashCode();
			}
			result.Append(ref val3);
		}
		Dictionary<string, int> entitySecondaryRampIdices = EntitySecondaryRampIdices;
		if (entitySecondaryRampIdices != null)
		{
			int val6 = 0;
			foreach (KeyValuePair<string, int> item2 in entitySecondaryRampIdices)
			{
				Hash128 hash2 = default(Hash128);
				Hash128 val7 = StringHasher.GetHash128(item2.Key);
				hash2.Append(ref val7);
				int obj2 = item2.Value;
				Hash128 val8 = UnmanagedHasher<int>.GetHash128(ref obj2);
				hash2.Append(ref val8);
				val6 ^= hash2.GetHashCode();
			}
			result.Append(ref val6);
		}
		result.Append(ref LeftHanded);
		result.Append(ref ClothesPrimaryIndex);
		result.Append(ref ClothesSecondaryIndex);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		DollData source = new DollData();
		result = Unsafe.As<DollData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<DollData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EnumField(0, "Gender", ref Gender, state);
		formatter.Field(1, "RacePreset", ref RacePreset, state);
		formatter.Field(2, "EquipmentEntityIds", ref EquipmentEntityIds, state);
		formatter.Field(3, "EntityRampIdices", ref EntityRampIdices, state);
		formatter.Field(4, "EntitySecondaryRampIdices", ref EntitySecondaryRampIdices, state);
		formatter.UnmanagedField(5, "LeftHanded", ref LeftHanded, state);
		formatter.UnmanagedField(6, "ClothesPrimaryIndex", ref ClothesPrimaryIndex, state);
		formatter.UnmanagedField(7, "ClothesSecondaryIndex", ref ClothesSecondaryIndex, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DollData>();
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
				Gender = formatter.ReadEnum<Gender>(state);
				break;
			case 1:
				RacePreset = formatter.ReadPackable<BlueprintRaceVisualPreset>(state);
				break;
			case 2:
				EquipmentEntityIds = formatter.ReadPackable<List<string>>(state);
				break;
			case 3:
				EntityRampIdices = formatter.ReadPackable<Dictionary<string, int>>(state);
				break;
			case 4:
				EntitySecondaryRampIdices = formatter.ReadPackable<Dictionary<string, int>>(state);
				break;
			case 5:
				LeftHanded = formatter.ReadUnmanaged<bool>(state);
				break;
			case 6:
				ClothesPrimaryIndex = formatter.ReadUnmanaged<int>(state);
				break;
			case 7:
				ClothesSecondaryIndex = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
