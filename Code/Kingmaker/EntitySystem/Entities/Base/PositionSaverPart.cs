using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities.Base;

[OwlPackable(OwlPackableMode.Generate)]
public class PositionSaverPart : EntityPartWithConfig, IHashable, IOwlPackable<PositionSaverPart>
{
	[JsonProperty]
	[OwlPackInclude]
	private Vector3? m_Position;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PositionSaverPart",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("m_Position", typeof(Vector3?))
		}
	};

	protected override void OnPreSave()
	{
		base.OnPreSave();
		m_Position = base.Owner.Position;
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		if (m_Position.HasValue)
		{
			base.Owner.Position = m_Position.Value;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		if (m_Position.HasValue)
		{
			Vector3 val2 = m_Position.Value;
			result.Append(ref val2);
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PositionSaverPart source = new PositionSaverPart();
		result = Unsafe.As<PositionSaverPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PositionSaverPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		formatter.NullableField(1, "m_Position", ref m_Position, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PositionSaverPart>();
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
				m_Position = formatter.ReadNullablePackable<Vector3>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
