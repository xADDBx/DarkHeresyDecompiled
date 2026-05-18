using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.Gameplay.Features.Weakpoints;

[OwlPackable(OwlPackableMode.Generate)]
public class PartWeakpoints : BaseUnitPart, IHashable, IOwlPackable<PartWeakpoints>
{
	public class Entry
	{
		public readonly struct Source
		{
			public readonly EntityFactRef FactRef;

			public readonly BlueprintComponent Component;

			public Source(EntityFactRef factRef, BlueprintComponent component)
			{
				FactRef = factRef;
				Component = component;
			}

			public bool Is(EntityFact fact, BlueprintComponent component)
			{
				if (FactRef == fact)
				{
					return Component == component;
				}
				return false;
			}
		}

		public readonly WeakpointSide Side;

		public readonly List<Source> Sources = new List<Source>();

		public GameObject Marker { get; set; }

		public Entry(WeakpointSide side)
		{
			Side = side;
		}
	}

	private Dictionary<WeakpointSide, Entry> _entries = new Dictionary<WeakpointSide, Entry>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartWeakpoints",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void Add(WeakpointSide side, EntityFact sourceFact, BlueprintComponent sourceComponent)
	{
		Entry entry = _entries.Get(side);
		bool num = entry == null;
		if (entry == null)
		{
			Entry entry3 = (_entries[side] = new Entry(side));
			entry = entry3;
		}
		if (entry.Sources.HasItem((Entry.Source i) => i.Is(sourceFact, sourceComponent)))
		{
			throw new InvalidOperationException("Weakpoint already added");
		}
		entry.Sources.Add(new Entry.Source(sourceFact, sourceComponent));
		if (num)
		{
			base.EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IWeakpointAdded>)delegate(IWeakpointAdded h)
			{
				h.HandleWeakpointAdded(side);
			}, isCheckRuntime: true);
		}
	}

	public void Remove(EntityFact sourceFact, BlueprintComponent sourceComponent)
	{
		List<WeakpointSide> value;
		using (CollectionPool<List<WeakpointSide>, WeakpointSide>.Get(out value))
		{
			foreach (var (item, entry2) in _entries)
			{
				entry2.Sources.RemoveAll((Entry.Source i) => i.Is(sourceFact, sourceComponent));
				if (entry2.Sources.Empty())
				{
					value.Add(item);
				}
			}
			foreach (WeakpointSide side in value)
			{
				_entries.Remove(side);
				base.EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IWeakpointRemoved>)delegate(IWeakpointRemoved h)
				{
					h.HandleWeakpointRemoved(side);
				}, isCheckRuntime: true);
			}
			if (_entries.Empty())
			{
				RemoveSelf();
			}
		}
	}

	public void RemoveAll(WeakpointSide side)
	{
		Entry entry = _entries.Get(side);
		if (entry == null)
		{
			return;
		}
		BaseUnitEntity owner = base.Owner;
		_entries.Remove(side);
		foreach (Entry.Source source in entry.Sources)
		{
			EntityFact fact = source.FactRef.Fact;
			fact?.Manager.Remove(fact);
		}
		base.EventBus.RaiseEvent((IBaseUnitEntity)owner, (Action<IWeakpointRemoved>)delegate(IWeakpointRemoved h)
		{
			h.HandleWeakpointRemoved(side);
		}, isCheckRuntime: true);
	}

	public bool HasWeakpoint(WeakpointSide side)
	{
		return _entries.ContainsKey(side);
	}

	public bool HasWeakpoint(WeakpointSide side, MechanicEntity caster)
	{
		return _entries.Get(side)?.Sources.HasItem((Entry.Source i) => i.FactRef.Fact?.MaybeContext?.MaybeCaster == caster) ?? false;
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
		PartWeakpoints source = new PartWeakpoints();
		result = Unsafe.As<PartWeakpoints, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartWeakpoints>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartWeakpoints>();
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
