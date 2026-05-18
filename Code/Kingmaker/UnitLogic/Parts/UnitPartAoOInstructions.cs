using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Components;
using Kingmaker.UnitLogic.Mechanics;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class UnitPartAoOInstructions : UnitPart, IHashable, IOwlPackable<UnitPartAoOInstructions>
{
	private readonly struct Entry : IEquatable<Entry>
	{
		private readonly EntityFactRef _factRef;

		private readonly BlueprintComponentReference _componentRef;

		public readonly RestrictionCalculator AllowingCondition;

		public EntityFact? Fact => _factRef;

		public Entry(EntityFact fact, AoORestriction component)
		{
			_factRef = fact;
			_componentRef = component;
			AllowingCondition = component.AllowingCondition;
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
		Name = "UnitPartAoOInstructions",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void Register(EntityFact fact, AoORestriction component)
	{
		if (!_entries.Add(new Entry(fact, component)))
		{
			throw new InvalidOperationException("Already registered");
		}
	}

	public void Unregister(EntityFact fact, AoORestriction component)
	{
		_entries.Remove(new Entry(fact, component));
		if (_entries.Count == 0)
		{
			RemoveSelf();
		}
	}

	public bool CanMakeAoOAgainst(BaseUnitEntity provoker)
	{
		foreach (Entry entry in _entries)
		{
			RestrictionCalculator allowingCondition = entry.AllowingCondition;
			if (allowingCondition != null && !allowingCondition.Empty)
			{
				MechanicsContext mechanicsContext = entry.Fact?.MaybeContext;
				if (mechanicsContext != null && !allowingCondition.IsPassed(mechanicsContext, null, provoker))
				{
					return false;
				}
			}
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
		UnitPartAoOInstructions source = new UnitPartAoOInstructions();
		result = Unsafe.As<UnitPartAoOInstructions, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartAoOInstructions>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartAoOInstructions>();
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
