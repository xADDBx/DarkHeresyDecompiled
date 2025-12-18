using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartDestroyAfterDelay : AbstractUnitPart, IGameTimeChangedHandler, ISubscriber, IHashable, IOwlPackable<PartDestroyAfterDelay>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartDestroyAfterDelay",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Delay", typeof(float))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public float Delay { get; private set; }

	public void Setup(float delay)
	{
		Delay = delay;
	}

	public void HandleGameTimeChanged(TimeSpan delta)
	{
		Delay -= (float)delta.TotalSeconds;
		if (!(Delay > 0f))
		{
			if ((bool)base.Owner.GetOptional<UnitPartSummonedMonster>())
			{
				Game.Instance.Controllers.EntityDestroyer.Destroy(base.Owner);
			}
			else if (!base.Owner.IsPlayerFaction)
			{
				base.Owner.IsInGame = false;
			}
			RemoveSelf();
		}
	}

	public void HandleNonGameTimeChanged()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		float val2 = Delay;
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartDestroyAfterDelay source = new PartDestroyAfterDelay();
		result = Unsafe.As<PartDestroyAfterDelay, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartDestroyAfterDelay>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		float value = Delay;
		formatter.UnmanagedField(0, "Delay", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartDestroyAfterDelay>();
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
				Delay = formatter.ReadUnmanaged<float>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
