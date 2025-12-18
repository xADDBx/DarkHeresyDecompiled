using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[Obsolete("Duplicating functionality of BlueprintAbilityModifier")]
[OwlPackable(OwlPackableMode.Generate)]
public class PartAbilitySettings : UnitPart, IInterruptTurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IInterruptTurnEndHandler, IHashable, IOwlPackable<PartAbilitySettings>
{
	public RestrictionCalculator InterruptionAbilityRestrictions;

	private readonly List<(EntityFactComponent Runtime, OverrideAbilityThreatenedAreaSetting Component)> m_ThreatenedAreaEntries = new List<(EntityFactComponent, OverrideAbilityThreatenedAreaSetting)>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartAbilitySettings",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void Add(OverrideAbilityThreatenedAreaSetting component)
	{
		m_ThreatenedAreaEntries.Add((component.Runtime, component));
	}

	public void Remove(OverrideAbilityThreatenedAreaSetting component)
	{
		m_ThreatenedAreaEntries.Remove((component.Runtime, component));
		RemoveSelfIfEmpty();
	}

	private void RemoveSelfIfEmpty()
	{
		if (m_ThreatenedAreaEntries.Empty())
		{
			RemoveSelf();
		}
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		if (EventInvokerExtensions.MechanicEntity == base.Owner)
		{
			InterruptionAbilityRestrictions = interruptionData.RestrictionsOnInterrupt;
		}
	}

	public void HandleUnitEndInterruptTurn()
	{
		if (EventInvokerExtensions.MechanicEntity == base.Owner)
		{
			InterruptionAbilityRestrictions = null;
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
		PartAbilitySettings source = new PartAbilitySettings();
		result = Unsafe.As<PartAbilitySettings, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartAbilitySettings>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartAbilitySettings>();
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
