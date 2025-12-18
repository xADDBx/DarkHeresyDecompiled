using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartModifiedSpells : BaseUnitPart, IHashable, IOwlPackable<UnitPartModifiedSpells>
{
	public class ModifiedSpell
	{
		public BlueprintAbility Spell;

		public EntityFact Source;

		public SpellModificationType Modification;
	}

	public List<ModifiedSpell> Entries = new List<ModifiedSpell>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartModifiedSpells",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void AddEntry(BlueprintAbility spell, EntityFact source, SpellModificationType modification)
	{
		ModifiedSpell item = new ModifiedSpell
		{
			Spell = spell,
			Source = source,
			Modification = modification
		};
		Entries.Add(item);
	}

	public void RemoveEntry(EntityFact source)
	{
		Entries.RemoveAll((ModifiedSpell p) => p.Source == source);
	}

	public bool HasEntry(BlueprintAbility spell, SpellModificationType modification)
	{
		return Entries.Any((ModifiedSpell p) => p.Spell == spell && p.Modification == modification);
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
		UnitPartModifiedSpells source = new UnitPartModifiedSpells();
		result = Unsafe.As<UnitPartModifiedSpells, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartModifiedSpells>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartModifiedSpells>();
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
