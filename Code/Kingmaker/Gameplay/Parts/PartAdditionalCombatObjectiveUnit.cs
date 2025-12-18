using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Code._Legacy.Components;
using Kingmaker.EntitySystem;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartAdditionalCombatObjectiveUnit : BaseUnitPart, IHashable, IOwlPackable<PartAdditionalCombatObjectiveUnit>
{
	private readonly struct Entry : IEquatable<Entry>
	{
		private readonly EntityFactRef _factRef;

		private readonly BlueprintComponentReference _componentRef;

		public EntityFact? Fact => _factRef;

		public AdditionalCombatObjectiveComponent? Component => _componentRef.Get() as AdditionalCombatObjectiveComponent;

		public HighlightType HighlightType => Component?.HighlightType ?? HighlightType.Default;

		public Entry(EntityFact fact, AdditionalCombatObjectiveComponent component)
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

	private readonly HashSet<Entry> _entries = new HashSet<Entry>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartAdditionalCombatObjectiveUnit",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("ObjectIsViewed", typeof(bool))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public bool ObjectIsViewed { get; private set; }

	public HighlightType HighlightType { get; private set; }

	public void Register(EntityFact fact, AdditionalCombatObjectiveComponent component)
	{
		_entries.Add(new Entry(fact, component));
		UpdateCurrentHighlightType();
		base.Owner.View?.UpdateHighlight();
	}

	public void Unregister(EntityFact fact, AdditionalCombatObjectiveComponent component)
	{
		_entries.Remove(new Entry(fact, component));
		UpdateCurrentHighlightType();
		base.Owner.View?.UpdateHighlight();
		if (_entries.Count == 0)
		{
			RemoveSelf();
		}
	}

	public LocalizedString? GetDescription()
	{
		return _entries.FirstOrDefault((Entry e) => e.Component?.Description?.String != null).Component?.Description?.String;
	}

	public void SetAsViewed()
	{
		ObjectIsViewed = true;
	}

	private void UpdateCurrentHighlightType()
	{
		HighlightType = HighlightType.Default;
		foreach (Entry entry in _entries)
		{
			if (entry.HighlightType == HighlightType.Always)
			{
				HighlightType = HighlightType.Always;
				break;
			}
			if (entry.HighlightType == HighlightType.Once)
			{
				HighlightType = HighlightType.Once;
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = ObjectIsViewed;
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartAdditionalCombatObjectiveUnit source = new PartAdditionalCombatObjectiveUnit();
		result = Unsafe.As<PartAdditionalCombatObjectiveUnit, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartAdditionalCombatObjectiveUnit>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		bool value = ObjectIsViewed;
		formatter.UnmanagedField(0, "ObjectIsViewed", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartAdditionalCombatObjectiveUnit>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				ObjectIsViewed = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
