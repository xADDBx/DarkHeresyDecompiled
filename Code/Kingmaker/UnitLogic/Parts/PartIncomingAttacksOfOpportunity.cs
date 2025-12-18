using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartIncomingAttacksOfOpportunity : UnitPart, IHashable, IOwlPackable<PartIncomingAttacksOfOpportunity>
{
	[JsonProperty]
	[OwlPackInclude]
	private List<AttackOfOpportunityData> m_Attacks;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartIncomingAttacksOfOpportunity",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_Attacks", typeof(List<AttackOfOpportunityData>)),
			new FieldInfo("StartPosition", typeof(Vector3))
		}
	};

	[JsonProperty(IsReference = false)]
	[OwlPackInclude]
	public Vector3 StartPosition { get; private set; }

	public IEnumerable<AttackOfOpportunityData> Attacks
	{
		get
		{
			IEnumerable<AttackOfOpportunityData> attacks = m_Attacks;
			return attacks ?? Enumerable.Empty<AttackOfOpportunityData>();
		}
	}

	public AttackOfOpportunityData? NextAttack
	{
		get
		{
			if (!m_Attacks.Empty())
			{
				return m_Attacks[0];
			}
			return null;
		}
	}

	public void SetAttacks(IEnumerable<AttackOfOpportunityData> attacks)
	{
		m_Attacks = attacks.ToList();
		StartPosition = base.Owner.Position;
	}

	public void AcceptNextAttack()
	{
		Accept(NextAttack);
	}

	private void Accept(AttackOfOpportunityData? attack)
	{
		if (attack.HasValue)
		{
			m_Attacks.Remove(attack.Value);
			if (m_Attacks.Empty())
			{
				RemoveSelf();
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<AttackOfOpportunityData> attacks = m_Attacks;
		if (attacks != null)
		{
			for (int i = 0; i < attacks.Count; i++)
			{
				AttackOfOpportunityData obj = attacks[i];
				Hash128 val2 = StructHasher<AttackOfOpportunityData>.GetHash128(ref obj);
				result.Append(ref val2);
			}
		}
		Vector3 val3 = StartPosition;
		result.Append(ref val3);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartIncomingAttacksOfOpportunity source = new PartIncomingAttacksOfOpportunity();
		result = Unsafe.As<PartIncomingAttacksOfOpportunity, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartIncomingAttacksOfOpportunity>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Attacks", ref m_Attacks, state);
		Vector3 value = StartPosition;
		formatter.Field(1, "StartPosition", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartIncomingAttacksOfOpportunity>();
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
				m_Attacks = formatter.ReadPackable<List<AttackOfOpportunityData>>(state);
				break;
			case 1:
				StartPosition = formatter.ReadPackable<Vector3>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
