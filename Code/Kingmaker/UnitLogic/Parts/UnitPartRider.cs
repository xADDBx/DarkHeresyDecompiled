using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartRider : BaseUnitPart, IHashable, IOwlPackable<UnitPartRider>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityPartRef<BaseUnitEntity, UnitPartSaddled> m_SaddledUnitRef;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartRider",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_SaddledUnitRef", typeof(EntityPartRef<BaseUnitEntity, UnitPartSaddled>))
		}
	};

	public BaseUnitEntity SaddledUnit => m_SaddledUnitRef.Entity;

	public void Mount([NotNull] BaseUnitEntity target)
	{
	}

	public void Dismount()
	{
	}

	public void DismountForce()
	{
		if (SaddledUnit != null)
		{
			ClearSaddledUnit();
			SetAvoidanceEnabled(enabled: true);
			Clear(toggleAbilityImmediately: true);
		}
	}

	private void ClearSaddledUnit()
	{
		EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitMountHandler>)delegate(IUnitMountHandler h)
		{
			h.HandleUnitDismount(m_SaddledUnitRef.Entity);
		}, isCheckRuntime: true);
		m_SaddledUnitRef.EntityPart?.Clear();
		m_SaddledUnitRef.Entity?.Remove<UnitPartSaddled>();
		m_SaddledUnitRef = default(EntityPartRef<BaseUnitEntity, UnitPartSaddled>);
	}

	private void Clear(bool toggleAbilityImmediately = false)
	{
	}

	private void SetAvoidanceEnabled(bool enabled)
	{
		UnitMovementAgentBase unitMovementAgentBase = base.Owner.View.Or(null)?.MovementAgent;
		if (unitMovementAgentBase != null)
		{
			unitMovementAgentBase.AvoidanceDisabled = !enabled;
		}
	}

	protected override void OnAttachOrPostLoad()
	{
		SetAvoidanceEnabled(enabled: false);
	}

	protected override void OnDetach()
	{
		SetAvoidanceEnabled(enabled: true);
	}

	protected override void OnViewDidAttach()
	{
		SetAvoidanceEnabled(enabled: false);
	}

	protected override void OnViewWillDetach()
	{
		SetAvoidanceEnabled(enabled: true);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		EntityPartRef<BaseUnitEntity, UnitPartSaddled> obj = m_SaddledUnitRef;
		Hash128 val2 = StructHasher<EntityPartRef<BaseUnitEntity, UnitPartSaddled>>.GetHash128(ref obj);
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartRider source = new UnitPartRider();
		result = Unsafe.As<UnitPartRider, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartRider>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_SaddledUnitRef", ref m_SaddledUnitRef, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartRider>();
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
				m_SaddledUnitRef = formatter.ReadPackable<EntityPartRef<BaseUnitEntity, UnitPartSaddled>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
