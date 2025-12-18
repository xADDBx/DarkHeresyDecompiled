using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartSaddled : BaseUnitPart, IHashable, IOwlPackable<UnitPartSaddled>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityPartRef<BaseUnitEntity, UnitPartRider> m_RiderRef;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartSaddled",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_RiderRef", typeof(EntityPartRef<BaseUnitEntity, UnitPartRider>))
		}
	};

	public BaseUnitEntity Rider => m_RiderRef.Entity;

	public void Initialize([NotNull] UnitPartRider rider)
	{
		m_RiderRef = rider;
	}

	public void Clear()
	{
		m_RiderRef = default(EntityPartRef<BaseUnitEntity, UnitPartRider>);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		EntityPartRef<BaseUnitEntity, UnitPartRider> obj = m_RiderRef;
		Hash128 val2 = StructHasher<EntityPartRef<BaseUnitEntity, UnitPartRider>>.GetHash128(ref obj);
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartSaddled source = new UnitPartSaddled();
		result = Unsafe.As<UnitPartSaddled, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartSaddled>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_RiderRef", ref m_RiderRef, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartSaddled>();
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
				m_RiderRef = formatter.ReadPackable<EntityPartRef<BaseUnitEntity, UnitPartRider>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
