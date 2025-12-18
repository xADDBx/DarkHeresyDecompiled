using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartCounterAttack : UnitPart, ITargetRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, ISubscriber, ITargetRulebookSubscriber, IUnitRunCommandHandler, IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, IHashable, IOwlPackable<UnitPartCounterAttack>
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class Entry : IHashable, IOwlPackable, IOwlPackable<Entry>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "Entry",
			OldNames = null,
			Fields = new FieldInfo[4]
			{
				new FieldInfo("FactId", typeof(string)),
				new FieldInfo("ComponentId", typeof(string)),
				new FieldInfo("UsageLimit", typeof(int?)),
				new FieldInfo("UseCount", typeof(int))
			}
		};

		[JsonProperty]
		[OwlPackInclude]
		public string FactId { get; private set; }

		[JsonProperty]
		[OwlPackInclude]
		public string ComponentId { get; private set; }

		[JsonProperty]
		[OwlPackInclude]
		public int? UsageLimit { get; private set; }

		[JsonProperty]
		[OwlPackInclude]
		public int UseCount { get; private set; }

		public BaseUnitEntity Owner { get; private set; }

		public UnitFact Fact { get; private set; }

		public CounterAttack Component { get; private set; }

		public Cells MaxDistanceToAlly { get; private set; }

		public bool CanUse(CounterAttack.TriggerType trigger, [CanBeNull] BaseUnitEntity targetAlly)
		{
			if (Owner == null || Fact == null || Component == null)
			{
				return false;
			}
			if (UseCount >= UsageLimit)
			{
				return false;
			}
			CounterAttack component = Component;
			if (component != null && component.Trigger < trigger)
			{
				return false;
			}
			int value = MaxDistanceToAlly.Value;
			if (targetAlly != null)
			{
				if (!Component.GuardAllies)
				{
					return false;
				}
				if (value > 0 && Owner.DistanceToInCells(targetAlly) > value)
				{
					return false;
				}
			}
			return true;
		}

		[JsonConstructor]
		private Entry()
		{
		}

		public Entry(BaseUnitEntity owner, UnitFact fact, CounterAttack component)
		{
			FactId = fact.UniqueId;
			Component = component;
			Setup(owner, fact, component);
		}

		public void Setup(BaseUnitEntity owner, UnitFact fact, CounterAttack component)
		{
			Owner = owner;
			Fact = fact;
			Component = component;
			if (component.GuardAllies)
			{
				MaxDistanceToAlly = component.GuardAlliesRange.Calculate(Fact.MaybeContext).Cells();
			}
		}

		public void Use()
		{
			if (UsageLimit.HasValue)
			{
				UseCount++;
			}
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			result.Append(FactId);
			result.Append(ComponentId);
			if (UsageLimit.HasValue)
			{
				int val = UsageLimit.Value;
				result.Append(ref val);
			}
			int val2 = UseCount;
			result.Append(ref val2);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			Entry source = new Entry();
			result = Unsafe.As<Entry, TPossiblyBase>(ref source);
		}

		public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
		{
			(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
			var (objectId, _) = orRegister;
			if (orRegister.isRef)
			{
				formatter.ObjectRef(objectId);
				return;
			}
			ushort type = state.TypeLibrary.RegisterType<Entry>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			string value = FactId;
			formatter.StringField(0, "FactId", ref value, state);
			string value2 = ComponentId;
			formatter.StringField(1, "ComponentId", ref value2, state);
			int? value3 = UsageLimit;
			formatter.UnmanagedNullableField(2, "UsageLimit", ref value3, state);
			int value4 = UseCount;
			formatter.UnmanagedField(3, "UseCount", ref value4, state);
			formatter.EndObject();
		}

		public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Entry>();
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
					FactId = formatter.ReadString(state);
					break;
				case 1:
					ComponentId = formatter.ReadString(state);
					break;
				case 2:
					UsageLimit = formatter.ReadNullableUnmanaged<int>(state);
					break;
				case 3:
					UseCount = formatter.ReadUnmanaged<int>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	private Entry m_DelayedCounterAttackEntry;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartCounterAttack",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Entries", typeof(List<Entry>))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	private List<Entry> m_Entries { get; set; } = new List<Entry>();


	public void Add(UnitFact fact, CounterAttack component)
	{
		Entry entry = m_Entries.Find((Entry i) => i.FactId == fact.UniqueId && i.ComponentId == component.name);
		if (entry != null)
		{
			entry.Setup(base.Owner, fact, component);
		}
		else
		{
			m_Entries.Add(new Entry(base.Owner, fact, component));
		}
	}

	public void Remove(UnitFact fact, CounterAttack component)
	{
		m_Entries.RemoveAll((Entry i) => i.FactId == fact.UniqueId && i.Component == component);
		if (m_Entries.Empty())
		{
			RemoveSelf();
		}
	}

	[CanBeNull]
	private Entry GetBestEntry([CanBeNull] Entry e1, [CanBeNull] Entry e2, CounterAttack.TriggerType trigger, [CanBeNull] BaseUnitEntity targetAlly)
	{
		bool flag = CanUse(e1, trigger, targetAlly);
		bool flag2 = CanUse(e2, trigger, targetAlly);
		if (!flag && !flag2)
		{
			return null;
		}
		if (flag && !flag2)
		{
			return e1;
		}
		if (!flag)
		{
			return e2;
		}
		bool hasValue = e1.UsageLimit.HasValue;
		bool hasValue2 = e2.UsageLimit.HasValue;
		if (!hasValue && hasValue2)
		{
			return e1;
		}
		if (hasValue && !hasValue2)
		{
			return e2;
		}
		return e1;
		static bool CanUse(Entry e, CounterAttack.TriggerType t, [CanBeNull] BaseUnitEntity ally)
		{
			return e?.CanUse(t, ally) ?? false;
		}
	}

	[CanBeNull]
	private Entry FindBestEntry(CounterAttack.TriggerType trigger, [CanBeNull] BaseUnitEntity targetAlly)
	{
		Entry entry = null;
		foreach (Entry entry2 in m_Entries)
		{
			entry = GetBestEntry(entry, entry2, trigger, targetAlly);
		}
		return entry;
	}

	public void OnEventAboutToTrigger(RulePerformAttack evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAttack evt)
	{
		Entry entry = FindBestEntry(CounterAttack.TriggerType.AfterAnyAttack, null);
		if (entry != null && (evt.IsMelee || entry.Component.CanUseInRange))
		{
			if (Game.Instance.Controllers.AttackOfOpportunityController.Provoke(evt.InitiatorUnit, base.Owner, entry.Fact))
			{
				entry.Use();
			}
			else if (entry.Component.CanUseInRange && TryCounterAttackInRange(evt.InitiatorUnit, base.Owner))
			{
				entry.Use();
			}
		}
	}

	public void HandleUnitRunCommand(AbstractUnitCommand cmd)
	{
		if (!(cmd is UnitUseAbility unitUseAbility) || unitUseAbility.Ability.IsMelee || cmd.Executor == base.Owner || cmd.TargetUnit == base.Owner)
		{
			return;
		}
		AbilityData ability = unitUseAbility.Ability;
		BaseUnitEntity baseUnitEntity = null;
		if (ability.GetPatternSettings() == null)
		{
			if (cmd.TargetUnit != null && base.Owner.IsAlly(cmd.TargetUnit) && cmd.TargetUnit is BaseUnitEntity baseUnitEntity2)
			{
				baseUnitEntity = baseUnitEntity2;
			}
		}
		else
		{
			Vector3 casterPosition = ability.GetBestShootingPosition(cmd.Target).Vector3Position();
			foreach (GridNodeBase node in ability.GetPattern(cmd.Target, casterPosition).Nodes)
			{
				BaseUnitEntity firstUnit = node.GetFirstUnit();
				if (firstUnit != null && base.Owner.IsAlly(firstUnit) && (baseUnitEntity == null || base.Owner.DistanceTo(firstUnit) < base.Owner.DistanceTo(baseUnitEntity)))
				{
					baseUnitEntity = firstUnit;
				}
			}
		}
		if (baseUnitEntity != null)
		{
			m_DelayedCounterAttackEntry = FindBestEntry(CounterAttack.TriggerType.AfterAnyAttack, baseUnitEntity);
		}
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand cmd)
	{
		if (m_DelayedCounterAttackEntry != null)
		{
			if (cmd.Executor is BaseUnitEntity target && Game.Instance.Controllers.AttackOfOpportunityController.Provoke(target, base.Owner, m_DelayedCounterAttackEntry.Fact))
			{
				m_DelayedCounterAttackEntry.Use();
			}
			else if (m_DelayedCounterAttackEntry.Component.CanUseInRange && TryCounterAttackInRange(cmd.Executor, base.Owner))
			{
				m_DelayedCounterAttackEntry.Use();
			}
			m_DelayedCounterAttackEntry = null;
		}
	}

	public void HandleUnitFinishedCommand()
	{
	}

	private bool TryCounterAttackInRange(AbstractUnitEntity target, BaseUnitEntity attacker)
	{
		if (!attacker.IsEnemy(target))
		{
			return false;
		}
		if (target.LifeState.IsDead)
		{
			return false;
		}
		if (!attacker.CombatState.CanActInCombat || !attacker.CanAct)
		{
			return false;
		}
		ItemEntityWeapon itemEntityWeapon = attacker.GetThreatHandRanged()?.Weapon;
		if (itemEntityWeapon == null)
		{
			return false;
		}
		Ability ability2 = itemEntityWeapon.Abilities.FirstItem((Ability ability) => ability.Data.IsRanged);
		if (ability2 == null)
		{
			PFLog.Default.Error("No abilities in blueprint ranged weapon " + itemEntityWeapon.Name + " for counter attack (unit " + attacker.Name + ")");
			return false;
		}
		UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(ability2.Data, target)
		{
			IgnoreCooldown = true,
			FreeAction = true,
			NeedLoS = false,
			IgnoreAbilityUsingInThreateningArea = true
		};
		attacker.Commands.AddToQueue(cmdParams);
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<Entry> entries = m_Entries;
		if (entries != null)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				Hash128 val2 = ClassHasher<Entry>.GetHash128(entries[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartCounterAttack source = new UnitPartCounterAttack();
		result = Unsafe.As<UnitPartCounterAttack, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartCounterAttack>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		List<Entry> value = m_Entries;
		formatter.Field(0, "m_Entries", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartCounterAttack>();
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
				m_Entries = formatter.ReadPackable<List<Entry>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
