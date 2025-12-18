using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.RuleSystem.Enum;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartAttackTypeRestriction : BaseUnitPart, IHashable, IOwlPackable<UnitPartAttackTypeRestriction>
{
	[JsonProperty]
	[OwlPackInclude]
	private sbyte[] m_DisabledTypes;

	[JsonProperty]
	[OwlPackInclude]
	private AttackTypeFlag m_DisabledTypesMask;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartAttackTypeRestriction",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_DisabledTypes", typeof(sbyte[])),
			new FieldInfo("m_DisabledTypesMask", typeof(AttackTypeFlag))
		}
	};

	public UnitPartAttackTypeRestriction()
	{
		m_DisabledTypes = new sbyte[EnumUtils.GetMaxValuePlusOne<AttackType>()];
	}

	public void DisableAttackTypes(AttackTypeFlag attackTypes)
	{
		for (int i = 0; i < m_DisabledTypes.Length; i++)
		{
			if (attackTypes.Contains((AttackType)i))
			{
				m_DisabledTypes[i]++;
			}
		}
		RecalculateMask();
	}

	public void EnableAttackTypes(AttackTypeFlag attackTypes)
	{
		for (int i = 0; i < m_DisabledTypes.Length; i++)
		{
			if (attackTypes.Contains((AttackType)i) && m_DisabledTypes[i] > 0)
			{
				m_DisabledTypes[i]--;
			}
		}
		RecalculateMask();
	}

	public bool CanAttack(AttackType attackType)
	{
		return !m_DisabledTypesMask.Contains(attackType);
	}

	private void RecalculateMask()
	{
		m_DisabledTypesMask = (AttackTypeFlag)0;
		for (int i = 0; i < m_DisabledTypes.Length; i++)
		{
			if (m_DisabledTypes[i] > 0)
			{
				m_DisabledTypesMask |= ((AttackType)i).ToFlag();
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(m_DisabledTypes);
		result.Append(ref m_DisabledTypesMask);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartAttackTypeRestriction source = new UnitPartAttackTypeRestriction();
		result = Unsafe.As<UnitPartAttackTypeRestriction, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartAttackTypeRestriction>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_DisabledTypes", ref m_DisabledTypes, state);
		formatter.EnumField(1, "m_DisabledTypesMask", ref m_DisabledTypesMask, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartAttackTypeRestriction>();
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
				m_DisabledTypes = formatter.ReadPackable<sbyte[]>(state);
				break;
			case 1:
				m_DisabledTypesMask = formatter.ReadEnum<AttackTypeFlag>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
