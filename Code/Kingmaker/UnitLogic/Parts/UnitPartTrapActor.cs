using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.MapObjects.Traps;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartTrapActor : BaseUnitPart, IHashable, IOwlPackable<UnitPartTrapActor>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityRef<TrapObjectData> m_TrapRef;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartTrapActor",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_TrapRef", typeof(EntityRef<TrapObjectData>))
		}
	};

	public TrapObjectData Trap => m_TrapRef;

	public void Setup(TrapObjectData trap)
	{
		m_TrapRef = trap;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		EntityRef<TrapObjectData> obj = m_TrapRef;
		Hash128 val2 = StructHasher<EntityRef<TrapObjectData>>.GetHash128(ref obj);
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartTrapActor source = new UnitPartTrapActor();
		result = Unsafe.As<UnitPartTrapActor, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartTrapActor>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_TrapRef", ref m_TrapRef, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartTrapActor>();
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
				m_TrapRef = formatter.ReadPackable<EntityRef<TrapObjectData>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
