using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartFadeOutAndDestroy : EntityPart, IGameTimeChangedHandler, ISubscriber, IHashable, IOwlPackable<PartFadeOutAndDestroy>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartFadeOutAndDestroy",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("DestroyTime", typeof(TimeSpan))
		}
	};

	private static LogChannel Logger => EntityDestructionController.Logger;

	[JsonProperty]
	[OwlPackInclude]
	public TimeSpan DestroyTime { get; private set; }

	public void Setup(float destroyDelay)
	{
		base.ConcreteOwner.WillBeDestroyed = true;
		DestroyTime = Game.Instance.Controllers.TimeController.GameTime + destroyDelay.Seconds();
		EventBus.RaiseEvent(base.Owner, delegate(IFadeOutAndDestroyHandler h)
		{
			h.HandleFadeOutAndDestroy();
		});
		Logger.Log($"Fade out and destroy {base.ConcreteOwner}. Destroy time {DestroyTime}");
	}

	public void HandleGameTimeChanged(TimeSpan delta)
	{
		CheckReadyToDestroy();
	}

	private void CheckReadyToDestroy()
	{
		if (!(Game.Instance.Player.GameTime < DestroyTime))
		{
			Game.Instance.Controllers.EntityDestroyer.Destroy(base.ConcreteOwner);
		}
	}

	public void HandleNonGameTimeChanged()
	{
		DestroyTime -= TimeSpan.FromSeconds(Time.deltaTime);
		CheckReadyToDestroy();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		TimeSpan val2 = DestroyTime;
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartFadeOutAndDestroy source = new PartFadeOutAndDestroy();
		result = Unsafe.As<PartFadeOutAndDestroy, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartFadeOutAndDestroy>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		TimeSpan value = DestroyTime;
		formatter.Field(0, "DestroyTime", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartFadeOutAndDestroy>();
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
				DestroyTime = formatter.ReadPackable<TimeSpan>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
