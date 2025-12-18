using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.FlagCountable;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class EntityPartStayOnPlatform : MechanicEntityPart<MechanicEntity>, IHashable, IOwlPackable<EntityPartStayOnPlatform>
{
	private PlatformObjectEntity m_Platform;

	private readonly CountableFlag m_OnPlatform = new CountableFlag();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "EntityPartStayOnPlatform",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void SetOnPlatform(PlatformObjectEntity platform)
	{
		m_OnPlatform.Retain();
		m_Platform = platform;
		m_Platform.AddEntity(base.Owner);
	}

	public void ReleaseFromPlatform()
	{
		m_OnPlatform.Release();
		m_Platform.RemoveEntity(base.Owner);
		m_Platform = null;
		base.Owner.Position = base.Owner.View.ViewTransform.position;
		if (base.Owner is AbstractUnitEntity abstractUnitEntity)
		{
			abstractUnitEntity.View.ForcePlaceAboveGround();
		}
	}

	public bool IsOnPlatform()
	{
		return m_OnPlatform;
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
		EntityPartStayOnPlatform source = new EntityPartStayOnPlatform();
		result = Unsafe.As<EntityPartStayOnPlatform, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<EntityPartStayOnPlatform>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EntityPartStayOnPlatform>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
