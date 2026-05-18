using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[Obsolete("Unused mechanic")]
[OwlPackable(OwlPackableMode.Generate)]
public class DestructionPart : EntityPartWithConfig<DestructionSettings>, IHashable, IOwlPackable<DestructionPart>
{
	[JsonProperty]
	[OwlPackInclude]
	private bool m_AlreadyDestroyed;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "DestructionPart",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("m_AlreadyDestroyed", typeof(bool))
		}
	};

	public bool AlreadyDestroyed => m_AlreadyDestroyed;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_AlreadyDestroyed);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		DestructionPart source = new DestructionPart();
		result = Unsafe.As<DestructionPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<DestructionPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		formatter.UnmanagedField(1, "m_AlreadyDestroyed", ref m_AlreadyDestroyed, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DestructionPart>();
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
			case 1:
				m_AlreadyDestroyed = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
