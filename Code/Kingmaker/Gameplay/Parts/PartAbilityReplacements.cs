using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartAbilityReplacements : BaseUnitPart, IHashable, IOwlPackable<PartAbilityReplacements>
{
	private sealed class Entry
	{
		public readonly EntityFactRef Source;

		public readonly BlueprintAbility Target;

		public readonly Ability Replacement;

		public Entry(EntityFact source, BlueprintAbility target, Ability replacement)
		{
			Source = source;
			Target = target;
			Replacement = replacement;
		}
	}

	private readonly List<Entry> _entries = new List<Entry>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartAbilityReplacements",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	[JsonConstructor]
	public PartAbilityReplacements()
	{
	}

	public void Add(EntityFact source, BlueprintAbility target, Ability replacement)
	{
		EntityFact source = source;
		BlueprintAbility target = target;
		Ability replacement = replacement;
		if (!_entries.HasItem<Entry>((Entry i) => i.Source == source && i.Target == target && i.Replacement == replacement))
		{
			_entries.Add(new Entry(source, target, replacement));
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IAbilityReplacementsUpdatedHandler>)delegate(IAbilityReplacementsUpdatedHandler h)
			{
				h.HandleAbilityReplacementsUpdated(target);
			}, isCheckRuntime: true);
		}
	}

	public void Remove(EntityFact source, BlueprintAbility target)
	{
		EntityFact source = source;
		BlueprintAbility target = target;
		_entries.RemoveAll((Entry i) => i.Source == source && i.Target == target);
		EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IAbilityReplacementsUpdatedHandler>)delegate(IAbilityReplacementsUpdatedHandler h)
		{
			h.HandleAbilityReplacementsUpdated(target);
		}, isCheckRuntime: true);
		if (_entries.Empty())
		{
			RemoveSelf();
		}
	}

	public Ability? Get(BlueprintAbility? target)
	{
		BlueprintAbility target = target;
		return _entries.LastItem<Entry>((Entry i) => i.Target == target)?.Replacement;
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
		PartAbilityReplacements source = new PartAbilityReplacements();
		result = Unsafe.As<PartAbilityReplacements, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartAbilityReplacements>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartAbilityReplacements>();
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
