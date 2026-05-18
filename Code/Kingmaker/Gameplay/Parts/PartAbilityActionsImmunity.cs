using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem;
using Kingmaker.Framework;
using Kingmaker.Framework.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartAbilityActionsImmunity : MechanicEntityPart, IHashable, IOwlPackable<PartAbilityActionsImmunity>
{
	private readonly struct Entry : IEquatable<Entry>
	{
		private readonly EntityFactRef _factRef;

		private readonly BlueprintComponentReference _componentRef;

		public EntityFact? Fact => _factRef;

		public AbilityActionsImmunity? Component => _componentRef.Get() as AbilityActionsImmunity;

		public Entry(EntityFact fact, AbilityActionsImmunity component)
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
		Name = "PartAbilityActionsImmunity",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void Register(EntityFact fact, AbilityActionsImmunity component)
	{
		if (!_entries.Add(new Entry(fact, component)))
		{
			throw new InvalidOperationException("Already registered");
		}
	}

	public void Unregister(EntityFact fact, AbilityActionsImmunity component)
	{
		_entries.Remove(new Entry(fact, component));
	}

	public bool IsImmune(IEvalContext context)
	{
		foreach (Entry entry in _entries)
		{
			AbilityActionsImmunity component = entry.Component;
			if (component != null)
			{
				RestrictionCalculator restrictions = component.Restrictions;
				if (restrictions != null && restrictions.IsPassed(context, base.Owner, base.Owner))
				{
					return true;
				}
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
		PartAbilityActionsImmunity source = new PartAbilityActionsImmunity();
		result = Unsafe.As<PartAbilityActionsImmunity, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartAbilityActionsImmunity>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartAbilityActionsImmunity>();
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
