using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic;

[OwlPackable(OwlPackableMode.Generate)]
public struct AttackOfOpportunityData : IHashable, IOwlPackable, IOwlPackable<AttackOfOpportunityData>
{
	[JsonProperty]
	[OwlPackInclude]
	public readonly BaseUnitEntity Attacker;

	[JsonProperty(IsReference = false)]
	[OwlPackInclude]
	public readonly Vector3 Position;

	[JsonProperty]
	[CanBeNull]
	[OwlPackInclude]
	public readonly BlueprintAbility Reason;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "AttackOfOpportunityData",
		Fields = new FieldInfo[3]
		{
			new FieldInfo("Attacker", typeof(BaseUnitEntity)),
			new FieldInfo("Position", typeof(Vector3)),
			new FieldInfo("Reason", typeof(BlueprintAbility))
		}
	};

	public AttackOfOpportunityData([NotNull] BaseUnitEntity attacker, Vector3 position)
	{
		Attacker = attacker ?? throw new ArgumentNullException("attacker");
		Position = position;
		Reason = null;
	}

	public AttackOfOpportunityData([NotNull] BaseUnitEntity attacker, Vector3 position, [CanBeNull] BlueprintAbility reason)
	{
		Attacker = attacker ?? throw new ArgumentNullException("attacker");
		Position = position;
		Reason = reason;
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = ClassHasher<BaseUnitEntity>.GetHash128(Attacker);
		result.Append(ref val);
		Vector3 val2 = Position;
		result.Append(ref val2);
		Hash128 val3 = SimpleBlueprintHasher.GetHash128(Reason);
		result.Append(ref val3);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		AttackOfOpportunityData source = default(AttackOfOpportunityData);
		result = Unsafe.As<AttackOfOpportunityData, TPossiblyBase>(ref source);
	}

	public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<AttackOfOpportunityData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BaseUnitEntity value = Attacker;
		formatter.Field(0, "Attacker", ref value, state);
		Vector3 value2 = Position;
		formatter.Field(1, "Position", ref value2, state);
		BlueprintAbility value3 = Reason;
		formatter.Field(2, "Reason", ref value3, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AttackOfOpportunityData>();
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
				Unsafe.AsRef(in Attacker) = formatter.ReadPackable<BaseUnitEntity>(state);
				break;
			case 1:
				Unsafe.AsRef(in Position) = formatter.ReadPackable<Vector3>(state);
				break;
			case 2:
				Unsafe.AsRef(in Reason) = formatter.ReadPackable<BlueprintAbility>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
