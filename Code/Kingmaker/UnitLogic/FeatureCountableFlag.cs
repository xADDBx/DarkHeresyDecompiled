using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;

namespace Kingmaker.UnitLogic;

public class FeatureCountableFlag
{
	public class FactsList
	{
		public class Element
		{
			public readonly BuffInformation BuffInformation;

			[CanBeNull]
			public readonly EntityFact Fact;

			private int m_Counter;

			public int Counter => m_Counter;

			public Element(EntityFact fact)
			{
				Fact = fact;
				BuffInformation = BuffInformation.Create(fact);
				m_Counter++;
			}

			public Element(ItemEntity item)
			{
				BuffInformation = BuffInformation.Create(item);
				m_Counter++;
			}

			public void Retain()
			{
				m_Counter++;
			}

			public void Release()
			{
				m_Counter--;
			}
		}

		private readonly Dictionary<string, Element> m_Facts = new Dictionary<string, Element>();

		public IEnumerable<Element> Elements => m_Facts.Values;

		public IEnumerable<EntityFact> Facts => m_Facts.Values.Select((Element i) => i.Fact).NotNull();

		public void TryAdd(MechanicEntityFact fact)
		{
			if (fact != null)
			{
				if (!m_Facts.TryGetValue(fact.UniqueId, out var value))
				{
					m_Facts.Add(fact.UniqueId, new Element(fact));
				}
				else
				{
					value.Retain();
				}
			}
		}

		public void TryRemove(MechanicEntityFact fact)
		{
			if (fact != null && m_Facts.TryGetValue(fact.UniqueId, out var value))
			{
				value.Release();
				if (value.Counter <= 0)
				{
					m_Facts.Remove(fact.UniqueId);
				}
			}
		}

		public void Clear()
		{
			m_Facts.Clear();
		}
	}

	private readonly CountableFlag m_Flag = new CountableFlag();

	private readonly FactsList m_AssociatedFacts = new FactsList();

	private MechanicsFeatureType m_Type;

	private AbstractUnitEntity m_Owner;

	public int Count => m_Flag.Count;

	public bool Value => m_Flag.Value;

	public MechanicsFeatureType Type => m_Type;

	public FactsList AssociatedFacts => m_AssociatedFacts;

	public EntityFact FirstAssociatedFact => m_AssociatedFacts.Facts.FirstOrDefault();

	public FeatureCountableFlag(MechanicEntity owner, MechanicsFeatureType type)
	{
		m_Type = type;
		m_Owner = owner as AbstractUnitEntity;
	}

	public void Retain(MechanicEntityFact associatedFact = null)
	{
		m_AssociatedFacts.TryAdd(associatedFact);
		m_Flag.Retain();
		EventBus.RaiseEvent((IAbstractUnitEntity)m_Owner, (Action<IUnitFeaturesHandler>)delegate(IUnitFeaturesHandler h)
		{
			h.HandleFeatureAdded(this);
		}, isCheckRuntime: true);
	}

	public void Release(MechanicEntityFact associatedFact = null)
	{
		m_AssociatedFacts.TryRemove(associatedFact);
		m_Flag.Release();
		EventBus.RaiseEvent((IAbstractUnitEntity)m_Owner, (Action<IUnitFeaturesHandler>)delegate(IUnitFeaturesHandler h)
		{
			h.HandleFeatureRemoved(this);
		}, isCheckRuntime: true);
	}

	public void ReleaseAll()
	{
		m_AssociatedFacts.Clear();
		m_Flag.ReleaseAll();
		EventBus.RaiseEvent((IAbstractUnitEntity)m_Owner, (Action<IUnitFeaturesHandler>)delegate(IUnitFeaturesHandler h)
		{
			h.HandleFeatureRemoved(this);
		}, isCheckRuntime: true);
	}

	public static implicit operator bool(FeatureCountableFlag flag)
	{
		if (flag != null && flag.m_Flag != null)
		{
			return flag.m_Flag.Count > 0;
		}
		return false;
	}

	public override string ToString()
	{
		return $"{(bool)this}({m_Flag.Count})";
	}
}
