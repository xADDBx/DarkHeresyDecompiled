using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.RuleSystem;

public abstract class RulebookEvent : IRulebookEvent
{
	public class CustomDataKey
	{
		private static int s_NextKey;

		private readonly int m_Key;

		private readonly string m_Name;

		public CustomDataKey(string name)
		{
			m_Key = s_NextKey++;
			m_Name = name;
		}

		public override bool Equals(object obj)
		{
			return this == obj;
		}

		public override int GetHashCode()
		{
			return m_Key;
		}

		public override string ToString()
		{
			return m_Name;
		}
	}

	[NotNull]
	private readonly IMechanicEntity _self;

	private bool m_Triggered;

	[CanBeNull]
	private Dictionary<CustomDataKey, object> m_CustomData;

	private bool m_GameLogDisabled;

	public RuleReason Reason { get; set; }

	public MechanicEntity ConcreteInitiator => Initiator;

	public MechanicEntity Initiator => (MechanicEntity)_self;

	public virtual MechanicEntity Target => null;

	IMechanicEntity IRulebookEvent.Initiator => _self;

	[CanBeNull]
	IMechanicEntity IRulebookEvent.Target => Target;

	public MechanicEntity Self => (MechanicEntity)_self;

	IMechanicEntity IRulebookEvent.Self => _self;

	public bool DisableGameLog
	{
		set
		{
			m_GameLogDisabled |= value;
		}
	}

	public virtual bool IsGameLogDisabled => m_GameLogDisabled;

	[CanBeNull]
	public BaseUnitEntity InitiatorUnit => Initiator as BaseUnitEntity;

	public bool IsTriggered => m_Triggered;

	[CanBeNull]
	public virtual AbilityData MaybeAbility => Reason.Ability;

	public Type RootType => typeof(RulebookEvent);

	protected RulebookEvent([NotNull] IMechanicEntity self)
	{
		_self = self;
	}

	public abstract void OnTrigger(RulebookEventContext context);

	public void OnDidTrigger()
	{
		m_Triggered = true;
	}

	public void SetCustomData<T>(CustomDataKey key, T value)
	{
		(m_CustomData ?? (m_CustomData = new Dictionary<CustomDataKey, object>()))[key] = value;
	}

	public bool TryGetCustomData<T>(CustomDataKey key, out T value)
	{
		if (m_CustomData == null || !m_CustomData.TryGetValue(key, out var value2) || !(value2 is T))
		{
			value = default(T);
			return false;
		}
		value = (T)value2;
		return true;
	}

	protected static RuleRollD100 RollD100()
	{
		return Rulebook.Trigger(new RuleRollD100(Rulebook.CurrentContext.Current?.Initiator));
	}
}
public abstract class RulebookEvent<TInitiator> : RulebookEvent where TInitiator : MechanicEntity
{
	public new TInitiator Initiator => (TInitiator)base.Initiator;

	public new TInitiator Self => (TInitiator)base.Self;

	protected RulebookEvent([NotNull] TInitiator self)
		: base(self)
	{
	}
}
