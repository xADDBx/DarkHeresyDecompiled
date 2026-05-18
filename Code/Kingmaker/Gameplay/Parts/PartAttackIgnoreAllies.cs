using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem;
using Kingmaker.Framework;
using Kingmaker.Gameplay.Components;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartAttackIgnoreAllies : MechanicEntityPart, IHashable, IOwlPackable<PartAttackIgnoreAllies>
{
	private readonly struct Entry : IEquatable<Entry>
	{
		private readonly EntityFactRef _factRef;

		private readonly BlueprintComponentReference _componentRef;

		public RestrictionCalculator? Restrictions => (_componentRef.Get() as AttackIgnoreAllies)?.Restrictions;

		public MechanicsContext? Context => _factRef.Fact?.MaybeContext;

		public Entry(EntityFact fact, AttackIgnoreAllies component)
		{
			_factRef = fact;
			_componentRef = component;
		}

		public bool Equals(Entry other)
		{
			if (_factRef.Equals(other._factRef))
			{
				return _componentRef.Equals(other._componentRef);
			}
			return false;
		}

		public override bool Equals(object? obj)
		{
			if (obj is Entry other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(_factRef, _componentRef);
		}
	}

	[OwlPackIgnore]
	private readonly HashSet<Entry> _entries = new HashSet<Entry>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartAttackIgnoreAllies",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void Register(EntityFact fact, AttackIgnoreAllies component)
	{
		if (!_entries.Add(new Entry(fact, component)))
		{
			throw new InvalidOperationException("Already registered");
		}
	}

	public void Unregister(EntityFact fact, AttackIgnoreAllies component)
	{
		_entries.Remove(new Entry(fact, component));
	}

	public bool ShouldIgnoreAllies(AbilityData ability)
	{
		foreach (Entry entry in _entries)
		{
			RestrictionCalculator restrictions = entry.Restrictions;
			if (restrictions != null && entry.Context != null && restrictions.IsPassed(EvalContext.Current, base.Owner, null, null, ability))
			{
				return true;
			}
		}
		return false;
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
		PartAttackIgnoreAllies source = new PartAttackIgnoreAllies();
		result = Unsafe.As<PartAttackIgnoreAllies, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartAttackIgnoreAllies>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartAttackIgnoreAllies>();
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
