using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Interaction;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[OwlPackable(OwlPackableMode.Generate)]
public class LoreXenosRestrictionPart : SkillUseWithoutItemRestrictionPart<LoreXenosRestrictionSettings>, ISkillUseRestrictionWithoutItem, IHashable, IOwlPackable<LoreXenosRestrictionPart>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "LoreXenosRestrictionPart",
		OldNames = null,
		Fields = new FieldInfo[5]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("IsDisabled", typeof(bool)),
			new FieldInfo("DCOverride", typeof(int)),
			new FieldInfo("Unlocked", typeof(bool)),
			new FieldInfo("Failed", typeof(bool))
		}
	};

	public override InteractionActorType Type => InteractionActorType.LoreXenos;

	public int DCOverrideValue => DCOverride;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		LoreXenosRestrictionPart source = new LoreXenosRestrictionPart();
		result = Unsafe.As<LoreXenosRestrictionPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<LoreXenosRestrictionPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		formatter.UnmanagedField(1, "IsDisabled", ref IsDisabled, state);
		formatter.UnmanagedField(2, "DCOverride", ref DCOverride, state);
		formatter.UnmanagedField(3, "Unlocked", ref Unlocked, state);
		formatter.UnmanagedField(4, "Failed", ref Failed, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<LoreXenosRestrictionPart>();
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
			case 2:
				DCOverride = formatter.ReadUnmanaged<int>(state);
				break;
			case 3:
				Unlocked = formatter.ReadUnmanaged<bool>(state);
				break;
			case 4:
				Failed = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
