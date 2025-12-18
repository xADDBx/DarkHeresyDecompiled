using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartDamageGrace : BaseUnitPart, IHashable, IOwlPackable<UnitPartDamageGrace>
{
	public class DamageGraceEntry
	{
		public WeaponCategory Category;

		public EntityFact Source;
	}

	public List<DamageGraceEntry> Weapons = new List<DamageGraceEntry>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartDamageGrace",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void AddEntry(WeaponCategory? category, EntityFact source)
	{
		if (category.HasValue)
		{
			DamageGraceEntry item = new DamageGraceEntry
			{
				Category = category.Value,
				Source = source
			};
			Weapons.Add(item);
		}
	}

	public void RemoveEntry(EntityFact source)
	{
		Weapons.RemoveAll((DamageGraceEntry p) => p.Source == source);
	}

	public bool HasEntry(WeaponCategory category)
	{
		return Weapons.Any((DamageGraceEntry p) => p.Category == category);
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
		UnitPartDamageGrace source = new UnitPartDamageGrace();
		result = Unsafe.As<UnitPartDamageGrace, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartDamageGrace>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartDamageGrace>();
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
