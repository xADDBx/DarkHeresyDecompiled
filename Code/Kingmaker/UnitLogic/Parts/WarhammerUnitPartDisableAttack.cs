using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.FlagCountable;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class WarhammerUnitPartDisableAttack : BaseUnitPart, IHashable, IOwlPackable<WarhammerUnitPartDisableAttack>
{
	private readonly CountableFlag m_AttacksDisabled = new CountableFlag();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "WarhammerUnitPartDisableAttack",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public bool AttackDisabled => m_AttacksDisabled;

	public void RetainDisabled(EntityFact source)
	{
		m_AttacksDisabled.Retain();
	}

	public void ReleaseDisabled(EntityFact source)
	{
		m_AttacksDisabled.Release();
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
		WarhammerUnitPartDisableAttack source = new WarhammerUnitPartDisableAttack();
		result = Unsafe.As<WarhammerUnitPartDisableAttack, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<WarhammerUnitPartDisableAttack>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<WarhammerUnitPartDisableAttack>();
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
