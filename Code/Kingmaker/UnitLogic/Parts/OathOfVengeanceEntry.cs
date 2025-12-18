using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class OathOfVengeanceEntry : IHashable, IOwlPackable, IOwlPackable<OathOfVengeanceEntry>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "OathOfVengeanceEntry",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("Ally", typeof(EntityRef<UnitEntity>)),
			new FieldInfo("Enemy", typeof(EntityRef<UnitEntity>))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public EntityRef<UnitEntity> Ally { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public EntityRef<UnitEntity> Enemy { get; set; }

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef<UnitEntity> obj = Ally;
		Hash128 val = StructHasher<EntityRef<UnitEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		EntityRef<UnitEntity> obj2 = Enemy;
		Hash128 val2 = StructHasher<EntityRef<UnitEntity>>.GetHash128(ref obj2);
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		OathOfVengeanceEntry source = new OathOfVengeanceEntry();
		result = Unsafe.As<OathOfVengeanceEntry, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<OathOfVengeanceEntry>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityRef<UnitEntity> value = Ally;
		formatter.Field(0, "Ally", ref value, state);
		EntityRef<UnitEntity> value2 = Enemy;
		formatter.Field(1, "Enemy", ref value2, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<OathOfVengeanceEntry>();
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
				Ally = formatter.ReadPackable<EntityRef<UnitEntity>>(state);
				break;
			case 1:
				Enemy = formatter.ReadPackable<EntityRef<UnitEntity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
