using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items.Slots;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Items;

[OwlPackable(OwlPackableMode.Generate)]
public class HandsEquipmentSet : IHashable, IOwlPackable, IOwlPackable<HandsEquipmentSet>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "HandsEquipmentSet",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("PrimaryHand", typeof(HandSlot)),
			new FieldInfo("SecondaryHand", typeof(HandSlot))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public HandSlot PrimaryHand { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public HandSlot SecondaryHand { get; private set; }

	public IEnumerable<HandSlot> Hands
	{
		get
		{
			yield return PrimaryHand;
			yield return SecondaryHand;
		}
	}

	public HandsEquipmentSet(BaseUnitEntity owner)
	{
		PrimaryHand = new HandSlot(owner);
		SecondaryHand = new HandSlot(owner);
	}

	[JsonConstructor]
	[UsedImplicitly]
	private HandsEquipmentSet()
	{
	}

	public bool IsEmpty()
	{
		if (!PrimaryHand.HasItem)
		{
			return !SecondaryHand.HasItem;
		}
		return false;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = ClassHasher<HandSlot>.GetHash128(PrimaryHand);
		result.Append(ref val);
		Hash128 val2 = ClassHasher<HandSlot>.GetHash128(SecondaryHand);
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		HandsEquipmentSet source = new HandsEquipmentSet();
		result = Unsafe.As<HandsEquipmentSet, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<HandsEquipmentSet>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		HandSlot value = PrimaryHand;
		formatter.Field(0, "PrimaryHand", ref value, state);
		HandSlot value2 = SecondaryHand;
		formatter.Field(1, "SecondaryHand", ref value2, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<HandsEquipmentSet>();
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
				PrimaryHand = formatter.ReadPackable<HandSlot>(state);
				break;
			case 1:
				SecondaryHand = formatter.ReadPackable<HandSlot>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
