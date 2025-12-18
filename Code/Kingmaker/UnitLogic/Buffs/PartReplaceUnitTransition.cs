using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs;

[OwlPackable(OwlPackableMode.Generate)]
public class PartReplaceUnitTransition : MechanicEntityPart, IHashable, IOwlPackable<PartReplaceUnitTransition>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityRef<AbstractUnitEntity> m_FromUnit;

	[JsonProperty]
	[OwlPackInclude]
	private EntityRef<AbstractUnitEntity> m_ToUnit;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartReplaceUnitTransition",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_FromUnit", typeof(EntityRef<AbstractUnitEntity>)),
			new FieldInfo("m_ToUnit", typeof(EntityRef<AbstractUnitEntity>))
		}
	};

	public bool IsFinished { get; private set; }

	public AbstractUnitEntity FromUnit => m_FromUnit;

	public AbstractUnitEntity ToUnit => m_ToUnit;

	public bool IsFromOwner => base.Owner == FromUnit;

	public bool IsToOwner => base.Owner == ToUnit;

	public void Setup(AbstractUnitEntity from, AbstractUnitEntity to)
	{
		m_FromUnit = from;
		m_ToUnit = to;
	}

	protected override void OnHoldingStateChanged()
	{
		if (IsToOwner && base.Owner.HoldingState != null)
		{
			IsFinished = true;
			PartReplaceUnitTransition replaceUnitTransitionOptional = FromUnit.GetReplaceUnitTransitionOptional();
			if (replaceUnitTransitionOptional != null)
			{
				replaceUnitTransitionOptional.IsFinished = true;
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		EntityRef<AbstractUnitEntity> obj = m_FromUnit;
		Hash128 val2 = StructHasher<EntityRef<AbstractUnitEntity>>.GetHash128(ref obj);
		result.Append(ref val2);
		EntityRef<AbstractUnitEntity> obj2 = m_ToUnit;
		Hash128 val3 = StructHasher<EntityRef<AbstractUnitEntity>>.GetHash128(ref obj2);
		result.Append(ref val3);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartReplaceUnitTransition source = new PartReplaceUnitTransition();
		result = Unsafe.As<PartReplaceUnitTransition, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartReplaceUnitTransition>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_FromUnit", ref m_FromUnit, state);
		formatter.Field(1, "m_ToUnit", ref m_ToUnit, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartReplaceUnitTransition>();
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
				m_FromUnit = formatter.ReadPackable<EntityRef<AbstractUnitEntity>>(state);
				break;
			case 1:
				m_ToUnit = formatter.ReadPackable<EntityRef<AbstractUnitEntity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
