using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Enums;
using Kingmaker.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

[OwlPackable(OwlPackableMode.Generate)]
public class PartUnitProficiency : EntityPart, IHashable, IOwlPackable<PartUnitProficiency>
{
	public interface IOwner : IEntityPartOwner<PartUnitProficiency>, IEntityPartOwner
	{
		PartUnitProficiency Proficiencies { get; }
	}

	private readonly MultiSet<ArmorProficiencyGroup> m_ArmorProficiencies = new MultiSet<ArmorProficiencyGroup> { ArmorProficiencyGroup.None };

	private readonly MultiSet<WeaponProficiency> m_WeaponProficiencies = new MultiSet<WeaponProficiency>
	{
		new WeaponProficiency(WeaponCategory.None)
	};

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartUnitProficiency",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public IEnumerable<ArmorProficiencyGroup> ArmorProficiencies => m_ArmorProficiencies;

	public IEnumerable<WeaponProficiency> WeaponProficiencies => m_WeaponProficiencies;

	public void Add(ArmorProficiencyGroup proficiency)
	{
		m_ArmorProficiencies.Add(proficiency);
	}

	public void Add(in WeaponProficiency proficiency)
	{
		m_WeaponProficiencies.Add(proficiency);
	}

	public void Remove(ArmorProficiencyGroup proficiency)
	{
		m_ArmorProficiencies.Remove(proficiency);
	}

	public void Remove(in WeaponProficiency proficiency)
	{
		m_WeaponProficiencies.Remove(proficiency);
	}

	public void Clear()
	{
		m_ArmorProficiencies.Clear();
		m_WeaponProficiencies.Clear();
	}

	public bool Contains(ArmorProficiencyGroup proficiency)
	{
		return m_ArmorProficiencies.Contains(proficiency);
	}

	public bool Contains(WeaponProficiency proficiency)
	{
		return m_WeaponProficiencies.Contains(proficiency);
	}

	public bool Contains(WeaponCategory category, WeaponFamily family)
	{
		if (!m_WeaponProficiencies.Contains(new WeaponProficiency(category, family)))
		{
			return m_WeaponProficiencies.Contains(new WeaponProficiency(category));
		}
		return true;
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
		PartUnitProficiency source = new PartUnitProficiency();
		result = Unsafe.As<PartUnitProficiency, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartUnitProficiency>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartUnitProficiency>();
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
