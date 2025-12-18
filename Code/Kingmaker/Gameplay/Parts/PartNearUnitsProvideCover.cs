using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartNearUnitsProvideCover : BaseUnitPart, IHashable, IOwlPackable<PartNearUnitsProvideCover>
{
	private readonly struct Entry
	{
		public readonly TargetType Filter;

		public readonly EntityFactRef SourceFact;

		public readonly BlueprintComponent SourceComponent;

		public Entry(TargetType filter, EntityFactRef sourceFact, BlueprintComponent sourceComponent)
		{
			Filter = filter;
			SourceFact = sourceFact;
			SourceComponent = sourceComponent;
		}

		public bool IsSuitable(BaseUnitEntity owner, BaseUnitEntity otherUnit)
		{
			if (otherUnit.IsDeadOrUnconscious)
			{
				return false;
			}
			if (Filter switch
			{
				TargetType.Enemy => owner.IsEnemy(otherUnit), 
				TargetType.Ally => owner.IsAlly(otherUnit), 
				TargetType.Any => true, 
				_ => throw new ArgumentOutOfRangeException(), 
			} && SourceFact.Fact is MechanicEntityFact mechanicEntityFact && SourceComponent is IRestrictionProvider restrictionProvider)
			{
				RestrictionCalculator restriction = restrictionProvider.GetRestriction();
				if (restriction != null)
				{
					return restriction.IsPassed(mechanicEntityFact.Context, otherUnit);
				}
			}
			return false;
		}
	}

	private readonly List<Entry> _entries = new List<Entry>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartNearUnitsProvideCover",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void Add(TargetType filter, EntityFact sourceFact, BlueprintComponent sourceComponent)
	{
		_entries.Add(new Entry(filter, sourceFact, sourceComponent));
	}

	public void Remove(EntityFact sourceFact, BlueprintComponent sourceComponent)
	{
		_entries.RemoveAll((Entry i) => i.SourceFact == sourceFact && i.SourceComponent == sourceComponent);
		if (_entries.Empty())
		{
			RemoveSelf();
		}
	}

	public bool IsSuitableCover(BaseUnitEntity otherUnit)
	{
		foreach (Entry entry in _entries)
		{
			if (entry.IsSuitable(base.Owner, otherUnit))
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
		PartNearUnitsProvideCover source = new PartNearUnitsProvideCover();
		result = Unsafe.As<PartNearUnitsProvideCover, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartNearUnitsProvideCover>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartNearUnitsProvideCover>();
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
