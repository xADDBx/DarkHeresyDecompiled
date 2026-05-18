using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[OwlPackable(OwlPackableMode.Generate)]
public class SpawnerMakePassivePart : EntityPartWithConfig, IUnitInitializer, IHashable, IOwlPackable<SpawnerMakePassivePart>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SpawnerMakePassivePart",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("SourceType", typeof(string))
		}
	};

	public void OnSpawn(AbstractUnitEntity unit)
	{
	}

	public void OnInitialize(AbstractUnitEntity unit)
	{
		unit.Passive.Retain();
	}

	public void OnDispose(AbstractUnitEntity unit)
	{
		unit.Passive.Release();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SpawnerMakePassivePart source = new SpawnerMakePassivePart();
		result = Unsafe.As<SpawnerMakePassivePart, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<SpawnerMakePassivePart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SpawnerMakePassivePart>();
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
				base.SourceType = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
