using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

[OwlPackable(OwlPackableMode.Generate)]
public class AreaEffectUnboundPart : EntityPart<AreaEffectEntity>, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IInGameHandler<EntitySubscriber>, IInGameHandler, ISubscriber<IEntity>, IEventTag<IInGameHandler, EntitySubscriber>, AreaEffectEntity.IEntityWithinBoundsHandler, IHashable, IOwlPackable<AreaEffectUnboundPart>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "AreaEffectUnboundPart",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public HashSet<EntityRef<MechanicEntity>> Entered { get; } = new HashSet<EntityRef<MechanicEntity>>();


	public HashSet<EntityRef<MechanicEntity>> Exited { get; } = new HashSet<EntityRef<MechanicEntity>>();


	public void ClearDelta()
	{
		Entered.Clear();
		Exited.Clear();
	}

	public void HandleUnitSpawned()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null)
		{
			Entered.Add(baseUnitEntity);
		}
	}

	public void HandleUnitDestroyed()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null)
		{
			Exited.Add(baseUnitEntity);
		}
	}

	public void HandleUnitDeath()
	{
	}

	public void HandleObjectInGameChanged()
	{
		if (base.Owner.IsInGame)
		{
			AddAll();
		}
		else
		{
			RemoveAll();
		}
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		AddAll();
	}

	protected override void OnViewWillDetach()
	{
		base.OnViewWillDetach();
		RemoveAll();
	}

	private void AddAll()
	{
		Exited.Clear();
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.EntityPools.AllBaseUnits)
		{
			Entered.Add(allBaseUnit);
		}
	}

	private void RemoveAll()
	{
		Entered.Clear();
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.EntityPools.AllBaseUnits)
		{
			Exited.Add(allBaseUnit);
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
		AreaEffectUnboundPart source = new AreaEffectUnboundPart();
		result = Unsafe.As<AreaEffectUnboundPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AreaEffectUnboundPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AreaEffectUnboundPart>();
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
