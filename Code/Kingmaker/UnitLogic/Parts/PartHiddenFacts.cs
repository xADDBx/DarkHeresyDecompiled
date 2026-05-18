using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartHiddenFacts : MechanicEntityPart, IHashable, IOwlPackable<PartHiddenFacts>
{
	public readonly struct Reason : IEquatable<Reason>
	{
		public readonly EntityFactRef Fact;

		public readonly BlueprintComponent Component;

		public Reason(EntityFact fact, BlueprintComponent component)
		{
			Fact = fact;
			Component = component;
		}

		public Reason(EntityFact fact, IHiddenFacts hiddenFacts)
			: this(fact, hiddenFacts as BlueprintComponent)
		{
		}

		public bool Equals(Reason other)
		{
			if (Fact.Equals(other.Fact))
			{
				return object.Equals(Component, other.Component);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Reason other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Fact, Component);
		}
	}

	private readonly Dictionary<BlueprintFact, HashSet<Reason>> m_HiddenFacts = new Dictionary<BlueprintFact, HashSet<Reason>>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartHiddenFacts",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public bool IsHidden(BlueprintFact fact)
	{
		IEnumerable<Reason> hideReasons;
		return IsHidden(fact, out hideReasons);
	}

	public bool IsHidden(BlueprintFact fact, out IEnumerable<Reason> hideReasons)
	{
		if (m_HiddenFacts.TryGetValue(fact, out var value))
		{
			hideReasons = value;
			return true;
		}
		hideReasons = Enumerable.Empty<Reason>();
		return false;
	}

	public void Add(EntityFact fact, IHiddenFacts hiddenFacts)
	{
		foreach (BlueprintFact fact2 in hiddenFacts.Facts)
		{
			if (!m_HiddenFacts.TryGetValue(fact2, out var value))
			{
				m_HiddenFacts.Add(fact2, value = new HashSet<Reason>());
			}
			value.Add(new Reason(fact, hiddenFacts));
		}
		base.EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IHiddenFactsUpdatedHandler>)delegate(IHiddenFactsUpdatedHandler h)
		{
			h.HandleHiddenFactsUpdated(hiddenFacts.Facts);
		}, isCheckRuntime: true);
	}

	public void Remove(EntityFact fact, IHiddenFacts hiddenFacts)
	{
		foreach (BlueprintFact fact2 in hiddenFacts.Facts)
		{
			if (m_HiddenFacts.TryGetValue(fact2, out var value))
			{
				value.Remove(new Reason(fact, hiddenFacts));
				if (value.Empty())
				{
					m_HiddenFacts.Remove(fact2);
				}
			}
		}
		if (m_HiddenFacts.Empty())
		{
			RemoveSelf();
		}
		base.EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IHiddenFactsUpdatedHandler>)delegate(IHiddenFactsUpdatedHandler h)
		{
			h.HandleHiddenFactsUpdated(hiddenFacts.Facts);
		}, isCheckRuntime: true);
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
		PartHiddenFacts source = new PartHiddenFacts();
		result = Unsafe.As<PartHiddenFacts, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartHiddenFacts>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartHiddenFacts>();
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
