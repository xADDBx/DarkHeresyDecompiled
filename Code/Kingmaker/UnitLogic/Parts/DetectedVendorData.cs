using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class DetectedVendorData : IHashable, IOwlPackable, IOwlPackable<DetectedVendorData>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "DetectedVendorData",
		OldNames = null,
		Fields = new FieldInfo[6]
		{
			new FieldInfo("Entity", typeof(EntityRef<MechanicEntity>)),
			new FieldInfo("EntityBlueprint", typeof(BlueprintScriptableObject)),
			new FieldInfo("Faction", typeof(BlueprintVendorFaction)),
			new FieldInfo("Area", typeof(BlueprintAreaReference)),
			new FieldInfo("AreaPart", typeof(BlueprintAreaPartReference)),
			new FieldInfo("Chapter", typeof(int))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public EntityRef<MechanicEntity> Entity { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public BlueprintScriptableObject EntityBlueprint { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public BlueprintVendorFaction Faction { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public BlueprintAreaReference Area { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public BlueprintAreaPartReference AreaPart { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public int Chapter { get; private set; }

	public string VendorName
	{
		get
		{
			object obj = (EntityBlueprint as BlueprintUnit)?.CharacterName;
			if (obj == null)
			{
				BlueprintMechanicEntityFact obj2 = EntityBlueprint as BlueprintMechanicEntityFact;
				if (obj2 == null)
				{
					return null;
				}
				obj = obj2.Name;
			}
			return (string)obj;
		}
	}

	public DetectedVendorData(MechanicEntity entity, [CanBeNull] BlueprintArea area, [CanBeNull] BlueprintAreaPart areaPart, int chapter)
	{
		Entity = entity;
		EntityBlueprint = entity.Blueprint;
		Faction = entity.GetRequired<PartVendor>().Faction;
		Area = area.ToReference<BlueprintAreaReference>();
		AreaPart = areaPart.ToReference<BlueprintAreaPartReference>();
		Chapter = chapter;
	}

	[JsonConstructor]
	private DetectedVendorData()
	{
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef<MechanicEntity> obj = Entity;
		Hash128 val = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		Hash128 val2 = SimpleBlueprintHasher.GetHash128(EntityBlueprint);
		result.Append(ref val2);
		Hash128 val3 = SimpleBlueprintHasher.GetHash128(Faction);
		result.Append(ref val3);
		Hash128 val4 = BlueprintReferenceHasher.GetHash128(Area);
		result.Append(ref val4);
		Hash128 val5 = BlueprintReferenceHasher.GetHash128(AreaPart);
		result.Append(ref val5);
		int val6 = Chapter;
		result.Append(ref val6);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		DetectedVendorData source = new DetectedVendorData();
		result = Unsafe.As<DetectedVendorData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<DetectedVendorData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityRef<MechanicEntity> value = Entity;
		formatter.Field(0, "Entity", ref value, state);
		BlueprintScriptableObject value2 = EntityBlueprint;
		formatter.Field(1, "EntityBlueprint", ref value2, state);
		BlueprintVendorFaction value3 = Faction;
		formatter.Field(2, "Faction", ref value3, state);
		BlueprintAreaReference value4 = Area;
		formatter.Field(3, "Area", ref value4, state);
		BlueprintAreaPartReference value5 = AreaPart;
		formatter.Field(4, "AreaPart", ref value5, state);
		int value6 = Chapter;
		formatter.UnmanagedField(5, "Chapter", ref value6, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DetectedVendorData>();
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
				Entity = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
				break;
			case 1:
				EntityBlueprint = formatter.ReadPackable<BlueprintScriptableObject>(state);
				break;
			case 2:
				Faction = formatter.ReadPackable<BlueprintVendorFaction>(state);
				break;
			case 3:
				Area = formatter.ReadPackable<BlueprintAreaReference>(state);
				break;
			case 4:
				AreaPart = formatter.ReadPackable<BlueprintAreaPartReference>(state);
				break;
			case 5:
				Chapter = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
