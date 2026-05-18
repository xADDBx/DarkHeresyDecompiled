using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[Obsolete("WH")]
[OwlPackable(OwlPackableMode.Generate)]
[OwlPackOldName("Kingmaker.View.MapObjects.DroppedLoot+EntityPartBreathOfMoney")]
public class EntityPartBreathOfMoney : EntityPart<DroppedLootEntity>, IAreaHandler, ISubscriber, IHashable, IOwlPackable<EntityPartBreathOfMoney>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "EntityPartBreathOfMoney",
		OldNames = new string[1] { "Kingmaker.View.MapObjects.DroppedLoot+EntityPartBreathOfMoney" },
		Fields = new FieldInfo[0]
	};

	public void OnAreaBeginUnloading()
	{
		Game.Instance.Controllers.EntityDestroyer.Destroy(base.Owner);
	}

	public void OnAreaDidLoad()
	{
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
		EntityPartBreathOfMoney source = new EntityPartBreathOfMoney();
		result = Unsafe.As<EntityPartBreathOfMoney, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<EntityPartBreathOfMoney>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EntityPartBreathOfMoney>();
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
