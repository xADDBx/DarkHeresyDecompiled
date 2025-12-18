using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Items.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartItemRemoveAtTheEndOfCombat : EntityPart<ItemEntity>, IPartyCombatHandler, ISubscriber, IHashable, IOwlPackable<PartItemRemoveAtTheEndOfCombat>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartItemRemoveAtTheEndOfCombat",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	void IPartyCombatHandler.HandlePartyCombatStateChanged(bool inCombat)
	{
		if (!inCombat)
		{
			using (ContextData<ItemSlot.IgnoreLock>.Request())
			{
				base.Owner.Collection?.Remove(base.Owner);
			}
		}
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
		PartItemRemoveAtTheEndOfCombat source = new PartItemRemoveAtTheEndOfCombat();
		result = Unsafe.As<PartItemRemoveAtTheEndOfCombat, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartItemRemoveAtTheEndOfCombat>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartItemRemoveAtTheEndOfCombat>();
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
