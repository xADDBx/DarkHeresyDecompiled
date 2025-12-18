using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Items.Slots;

[OwlPackable(OwlPackableMode.Generate)]
public class AdditionalLimbSlot : WeaponSlot, IHashable, IOwlPackable<AdditionalLimbSlot>
{
	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "AdditionalLimbSlot",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("m_ItemRef", typeof(EntityRef<ItemEntity>)),
			new FieldInfo("m_Active", typeof(bool)),
			new FieldInfo("KeepInPolymorph", typeof(bool))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public bool KeepInPolymorph { get; set; }

	public AdditionalLimbSlot(BaseUnitEntity owner)
		: base(owner)
	{
	}

	public AdditionalLimbSlot(JsonConstructorMark _)
		: base(_)
	{
	}

	protected AdditionalLimbSlot()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = KeepInPolymorph;
		result.Append(ref val2);
		return result;
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		AdditionalLimbSlot source = new AdditionalLimbSlot();
		result = Unsafe.As<AdditionalLimbSlot, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AdditionalLimbSlot>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_ItemRef", ref m_ItemRef, state);
		formatter.UnmanagedField(1, "m_Active", ref m_Active, state);
		bool value = KeepInPolymorph;
		formatter.UnmanagedField(2, "KeepInPolymorph", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AdditionalLimbSlot>();
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
				m_ItemRef = formatter.ReadPackable<EntityRef<ItemEntity>>(state);
				break;
			case 1:
				m_Active = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				KeepInPolymorph = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
