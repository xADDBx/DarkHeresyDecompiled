using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[OwlPackable(OwlPackableMode.Generate)]
public class ScriptRestrictionPart : InteractionRestrictionPart, IHashable, IOwlPackable<ScriptRestrictionPart>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ScriptRestrictionPart",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("IsDisabled", typeof(bool))
		}
	};

	public override int GetUserPriority(BaseUnitEntity user)
	{
		return -1;
	}

	public override bool CheckRestriction(BaseUnitEntity user)
	{
		return false;
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
		ScriptRestrictionPart source = new ScriptRestrictionPart();
		result = Unsafe.As<ScriptRestrictionPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ScriptRestrictionPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		formatter.UnmanagedField(1, "IsDisabled", ref IsDisabled, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ScriptRestrictionPart>();
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
				IsDisabled = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
