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
public class UnitPartUnlimitedSpells : BaseUnitPart, IHashable, IOwlPackable<UnitPartUnlimitedSpells>
{
	public class UnlimitedEntry
	{
		public BlueprintAbility Ability;

		public EntityFact Source;
	}

	public List<UnlimitedEntry> Entries = new List<UnlimitedEntry>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartUnlimitedSpells",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void AddEntry(BlueprintAbility ability, EntityFact source)
	{
		UnlimitedEntry item = new UnlimitedEntry
		{
			Ability = ability,
			Source = source
		};
		Entries.Add(item);
	}

	public void RemoveUnlimitedEntry(EntityFact source)
	{
		Entries.RemoveAll((UnlimitedEntry p) => p.Source == source);
	}

	public bool CheckUnlimitedEntry(BlueprintAbility ability)
	{
		return Entries.Any((UnlimitedEntry p) => p.Ability == ability);
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
		UnitPartUnlimitedSpells source = new UnitPartUnlimitedSpells();
		result = Unsafe.As<UnitPartUnlimitedSpells, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartUnlimitedSpells>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartUnlimitedSpells>();
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
