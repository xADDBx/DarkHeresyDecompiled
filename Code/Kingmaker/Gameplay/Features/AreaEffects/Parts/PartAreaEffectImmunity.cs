using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.AreaEffects.Components;
using Kingmaker.UnitLogic.Mechanics;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.AreaEffects.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartAreaEffectImmunity : MechanicEntityPart, IHashable, IOwlPackable<PartAreaEffectImmunity>
{
	private readonly struct Entry : IEquatable<Entry>
	{
		private readonly EntityFactRef _factRef;

		private readonly BlueprintComponentReference _componentRef;

		public EntityFact? Fact => _factRef;

		public AreaEffectImmunity? Component => _componentRef.Get() as AreaEffectImmunity;

		public Entry(EntityFact fact, AreaEffectImmunity component)
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
		Name = "PartAreaEffectImmunity",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void Register(EntityFact fact, AreaEffectImmunity component)
	{
		if (!_entries.Add(new Entry(fact, component)))
		{
			throw new InvalidOperationException("Already registered");
		}
	}

	public void Unregister(EntityFact fact, AreaEffectImmunity component)
	{
		_entries.Remove(new Entry(fact, component));
	}

	public bool IsImmune(AreaEffectEntity areaEffect)
	{
		foreach (Entry entry in _entries)
		{
			AreaEffectImmunity component = entry.Component;
			if (component != null)
			{
				RestrictionCalculator restrictions = component.Restrictions;
				if (restrictions != null && restrictions.IsPassed(areaEffect.Context, base.Owner))
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
		PartAreaEffectImmunity source = new PartAreaEffectImmunity();
		result = Unsafe.As<PartAreaEffectImmunity, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartAreaEffectImmunity>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartAreaEffectImmunity>();
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
