using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechanics.Entities;
using Newtonsoft.Json;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartHealth : MechanicEntityPart, IInGameHandler<EntitySubscriber>, IInGameHandler, ISubscriber<IEntity>, ISubscriber, IEventTag<IInGameHandler, EntitySubscriber>, IDamageablePart, IHashable, IOwlPackable<PartHealth>
{
	public interface IOwner : IEntityPartOwner<PartHealth>, IEntityPartOwner
	{
		PartHealth Health { get; }
	}

	[OwlPackable(OwlPackableMode.Generate)]
	public class TemporaryHitPointsData : IHashable, IOwlPackable, IOwlPackable<TemporaryHitPointsData>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "TemporaryHitPointsData",
			OldNames = null,
			Fields = new FieldInfo[3]
			{
				new FieldInfo("Source", typeof(EntityFactRef<Buff>)),
				new FieldInfo("Round", typeof(int)),
				new FieldInfo("Value", typeof(int))
			}
		};

		[JsonProperty]
		[OwlPackInclude]
		public EntityFactRef<Buff> Source { get; private set; }

		[JsonProperty]
		[OwlPackInclude]
		public int Round { get; set; }

		[JsonProperty]
		[OwlPackInclude]
		public int Value { get; set; }

		[JsonConstructor]
		private TemporaryHitPointsData()
		{
		}

		public TemporaryHitPointsData([NotNull] Buff source)
		{
			Source = source;
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			EntityFactRef<Buff> obj = Source;
			Hash128 val = StructHasher<EntityFactRef<Buff>>.GetHash128(ref obj);
			result.Append(ref val);
			int val2 = Round;
			result.Append(ref val2);
			int val3 = Value;
			result.Append(ref val3);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			TemporaryHitPointsData source = new TemporaryHitPointsData();
			result = Unsafe.As<TemporaryHitPointsData, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<TemporaryHitPointsData>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			EntityFactRef<Buff> value = Source;
			formatter.Field(0, "Source", ref value, state);
			int value2 = Round;
			formatter.UnmanagedField(1, "Round", ref value2, state);
			int value3 = Value;
			formatter.UnmanagedField(2, "Value", ref value3, state);
			formatter.EndObject();
		}

		public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<TemporaryHitPointsData>();
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
					Source = formatter.ReadPackable<EntityFactRef<Buff>>(state);
					break;
				case 1:
					Round = formatter.ReadUnmanaged<int>(state);
					break;
				case 2:
					Value = formatter.ReadUnmanaged<int>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	private int m_DamageReceivedThisTurn;

	[JsonProperty]
	[OwlPackInclude]
	private int m_LastTurnWhenReceiveDamage;

	[JsonProperty]
	[OwlPackInclude]
	private int m_ConsecutiveOutOfCombatTurnsWithoutDamage;

	[JsonProperty]
	[CanBeNull]
	[OwlPackInclude]
	private List<TemporaryHitPointsData> m_TemporaryHitPoints = new List<TemporaryHitPointsData>();

	[JsonProperty]
	[OwlPackInclude]
	private float m_MissingHpFraction;

	[CanBeNull]
	private List<(EntityFact Fact, BlueprintComponent Component, int Value)> m_HealthGuards;

	private int m_MinHitPoints;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartHealth",
		OldNames = null,
		Fields = new FieldInfo[5]
		{
			new FieldInfo("m_DamageReceivedThisTurn", typeof(int)),
			new FieldInfo("m_LastTurnWhenReceiveDamage", typeof(int)),
			new FieldInfo("m_ConsecutiveOutOfCombatTurnsWithoutDamage", typeof(int)),
			new FieldInfo("m_TemporaryHitPoints", typeof(List<TemporaryHitPointsData>)),
			new FieldInfo("m_MissingHpFraction", typeof(float))
		}
	};

	public int Damage
	{
		get
		{
			return Mathf.FloorToInt(m_MissingHpFraction * (float)MaxHitPoints);
		}
		private set
		{
			m_MissingHpFraction = (float)value / (float)MaxHitPoints;
		}
	}

	private StatsContainer StatsContainer => base.Owner.GetRequired<PartStatsContainer>().Container;

	public ModifiableValueAttributeStat Toughness => StatsContainer.GetAttribute(StatType.Toughness);

	public ModifiableValueHitPoints HitPoints => StatsContainer.GetStat<ModifiableValueHitPoints>(StatType.HitPoints);

	[CanBeNull]
	public RuleDealDamage LastHandledDamage { get; set; }

	public int HitPointsLeft => (int)HitPoints - Damage;

	public float HitPointsLeftFraction => (float)HitPointsLeft / (float)MaxHitPoints;

	public int MaxHitPoints => HitPoints;

	public int TemporaryHitPoints => CalculateTemporaryHitPoints();

	protected override void OnAttachOrPrePostLoad()
	{
		StatsContainer.RegisterAttribute(StatType.Toughness);
		StatsContainer.Register<ModifiableValueHitPoints>(StatType.HitPoints);
	}

	void IInGameHandler.HandleObjectInGameChanged()
	{
		LastHandledDamage = null;
	}

	public void SetDamage(int damage)
	{
		if (damage < 0)
		{
			PFLog.Default.Error("Damage can't be less than 0");
			return;
		}
		int damage2 = Damage;
		int num;
		if (base.Owner.IsMechanism)
		{
			PartArmor required = base.Owner.GetRequired<PartArmor>();
			if (required != null)
			{
				num = required.Damage;
				goto IL_0046;
			}
		}
		num = ClampDamage(damage);
		goto IL_0046;
		IL_0046:
		int num2 = num;
		if (num2 != damage2)
		{
			Damage = num2;
			if (Damage > damage2)
			{
				m_LastTurnWhenReceiveDamage = Game.Instance.Controllers.TurnController.GameRound;
				m_ConsecutiveOutOfCombatTurnsWithoutDamage = 0;
			}
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IEntityDamageChanged>)delegate(IEntityDamageChanged h)
			{
				h.HandleDamageChanged(this);
			}, isCheckRuntime: true);
			if (base.Owner.View is AbstractUnitEntityView abstractUnitEntityView)
			{
				abstractUnitEntityView.HandleDamage();
			}
			base.Owner.GetDestructionStagesManagerOptional()?.Update();
		}
	}

	public int ClampDamage(int damage)
	{
		bool flag = base.Owner.HasMechanicFeature(MechanicsFeatureType.Undying);
		int num = Math.Max(m_MinHitPoints, flag ? 1 : 0);
		return Math.Clamp(damage, 0, Math.Max(0, MaxHitPoints - num));
	}

	public void DealDamage(int damage)
	{
		SetDamage(Damage + ApplyTemporaryHitPoints(damage));
	}

	public void HealDamage(int heal)
	{
		SetDamage(Math.Max(0, Damage - heal));
	}

	public void HealDamageAll()
	{
		SetDamage(0);
	}

	public void SetHitPointsLeft(int targetHP)
	{
		SetDamage(Math.Max(0, MaxHitPoints - targetHP));
	}

	public void HealAll()
	{
		HealDamageAll();
		ReduceCriticalEffectsStageBy(int.MaxValue, base.Owner);
	}

	private void UpdateMinHitPoints()
	{
		m_MinHitPoints = 0;
		if (m_HealthGuards == null)
		{
			return;
		}
		foreach (var healthGuard in m_HealthGuards)
		{
			int val = Math.Clamp(healthGuard.Value, 0, MaxHitPoints);
			m_MinHitPoints = Math.Max(m_MinHitPoints, val);
		}
		if (HitPointsLeft < m_MinHitPoints)
		{
			SetHitPointsLeft(m_MinHitPoints);
		}
	}

	public void AddHealthGuard(EntityFact fact, BlueprintComponent component, int value)
	{
		if (m_HealthGuards == null)
		{
			m_HealthGuards = new List<(EntityFact, BlueprintComponent, int)>();
		}
		m_HealthGuards.Add((fact, component, value));
		UpdateMinHitPoints();
	}

	public void RemoveHealthGuard(EntityFact fact, BlueprintComponent component)
	{
		m_HealthGuards?.RemoveAll(((EntityFact Fact, BlueprintComponent Component, int Value) i) => i.Fact == fact && i.Component == component);
		m_HealthGuards = (m_HealthGuards.Empty() ? null : m_HealthGuards);
		UpdateMinHitPoints();
	}

	private int CalculateTemporaryHitPoints()
	{
		if (m_TemporaryHitPoints == null)
		{
			return 0;
		}
		TemporaryHitPointsData temporaryHitPointsData = m_TemporaryHitPoints.MaxBy((TemporaryHitPointsData i) => i.Value);
		int num = 0;
		foreach (TemporaryHitPointsData temporaryHitPoint in m_TemporaryHitPoints)
		{
			if (temporaryHitPoint != temporaryHitPointsData && temporaryHitPoint.Round == Game.Instance.Controllers.TurnController.GameRound)
			{
				num += temporaryHitPoint.Value;
			}
		}
		int num2 = temporaryHitPointsData?.Value ?? 0;
		if ((bool)base.Owner.Features.Vanguard)
		{
			return num2 + num;
		}
		return num2;
	}

	private int ApplyTemporaryHitPoints(int damage)
	{
		if (m_TemporaryHitPoints.Empty())
		{
			return damage;
		}
		int num = damage;
		if ((bool)base.Owner.Features.Vanguard)
		{
			while (true)
			{
				TemporaryHitPointsData temporaryHitPointsData = GetStackingHP();
				if (temporaryHitPointsData == null || damage <= 0)
				{
					break;
				}
				Handle(temporaryHitPointsData, ref damage);
			}
		}
		foreach (TemporaryHitPointsData item in m_TemporaryHitPoints.ToTempList())
		{
			if (!base.Owner.Features.Vanguard || item.Round != Game.Instance.Controllers.TurnController.GameRound)
			{
				int damageValue2 = num;
				Handle(item, ref damageValue2);
				damage = Math.Min(damageValue2, damage);
			}
		}
		return damage;
		TemporaryHitPointsData GetStackingHP()
		{
			return m_TemporaryHitPoints?.Where((TemporaryHitPointsData i) => i.Round == Game.Instance.Controllers.TurnController.GameRound).MaxBy((TemporaryHitPointsData i) => i.Value);
		}
		void Handle(TemporaryHitPointsData tHP, ref int damageValue)
		{
			int value = tHP.Value;
			int num2 = damageValue - value;
			tHP.Value = ((num2 < 0) ? (-num2) : 0);
			damageValue = Math.Max(0, num2);
			if (tHP.Value <= 0)
			{
				m_TemporaryHitPoints?.Remove(tHP);
				tHP.Source.Fact?.Remove();
			}
		}
	}

	public void AddTemporaryHitPoints(int amount, Buff sourceBuff)
	{
		if (!m_TemporaryHitPoints.HasItem((TemporaryHitPointsData i) => i.Source == sourceBuff))
		{
			TemporaryHitPointsData item = new TemporaryHitPointsData(sourceBuff)
			{
				Value = amount,
				Round = Game.Instance.Controllers.TurnController.GameRound
			};
			if (m_TemporaryHitPoints == null)
			{
				m_TemporaryHitPoints = new List<TemporaryHitPointsData>();
			}
			m_TemporaryHitPoints.Add(item);
			EventBus.RaiseEvent((IEntity)base.Owner, (Action<ITemporaryHitPoints>)delegate(ITemporaryHitPoints h)
			{
				h.HandleOnAddTemporaryHitPoints(amount, sourceBuff);
			}, isCheckRuntime: true);
		}
	}

	public void RemoveTemporaryHitPoints(Buff sourceBuff)
	{
		if (!m_TemporaryHitPoints.Empty() && sourceBuff != null)
		{
			EventBus.RaiseEvent((IEntity)base.Owner, (Action<ITemporaryHitPoints>)delegate(ITemporaryHitPoints h)
			{
				h.HandleOnRemoveTemporaryHitPoints(GetTemporaryHitPointFromBuff(sourceBuff.Blueprint), sourceBuff);
			}, isCheckRuntime: true);
		}
		m_TemporaryHitPoints?.Remove((TemporaryHitPointsData i) => i.Source == sourceBuff);
		if (m_TemporaryHitPoints.Empty())
		{
			m_TemporaryHitPoints = null;
		}
	}

	public int GetTemporaryHitPointFromBuff([NotNull] BlueprintBuff sourceBuff)
	{
		if (m_TemporaryHitPoints != null && !m_TemporaryHitPoints.Empty())
		{
			return m_TemporaryHitPoints.Where((TemporaryHitPointsData p1) => p1.Source.Fact?.Blueprint == sourceBuff).Sum((TemporaryHitPointsData p2) => p2.Value);
		}
		return 0;
	}

	private void ShiftCriticalEffectStage(BlueprintBodyPart damageBodyPart, int amount, MechanicEntity caster)
	{
		BlueprintBuff maybeBlueprint = damageBodyPart.CriticalEffect.MaybeBlueprint;
		if (maybeBlueprint == null)
		{
			return;
		}
		int maxRank = maybeBlueprint.MaxRank;
		int currentStage = GetCriticalStage(damageBodyPart);
		int newStage = Math.Clamp(currentStage + amount, 0, maxRank);
		if (currentStage == newStage)
		{
			return;
		}
		if (currentStage <= 0 && newStage > 0)
		{
			BuffCollection buffs = base.Owner.Buffs;
			int rank = newStage;
			buffs.Add(maybeBlueprint, caster, null, default(BuffDuration), rank);
		}
		else if (newStage <= 0)
		{
			base.Owner.Buffs.Remove(maybeBlueprint, caster);
		}
		else if (newStage > currentStage)
		{
			base.Owner.Buffs.Get(maybeBlueprint)?.AddRank(newStage - currentStage, caster);
		}
		else if (newStage < currentStage)
		{
			base.Owner.Buffs.Get(maybeBlueprint)?.RemoveRank(currentStage - newStage, caster);
		}
		if (newStage == maxRank)
		{
			MoraleRoot moraleRoot = ConfigRoot.Instance.MoraleRoot;
			using MechanicsContext mechanicsContext = MechanicsContext.Claim(moraleRoot, caster, null, null, base.Owner);
			using (mechanicsContext.SetScope())
			{
				moraleRoot.MaxCriticalStageActions.Run();
			}
		}
		EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<ICriticalEffectStageChanged>)delegate(ICriticalEffectStageChanged h)
		{
			h.HandleCriticalEffectStageChanged(damageBodyPart, currentStage, newStage);
		}, isCheckRuntime: true);
	}

	public void AddCriticalEffectStages(BlueprintBodyPart damageBodyPart, int amount, MechanicEntity caster)
	{
		if (amount > 0)
		{
			ShiftCriticalEffectStage(damageBodyPart, amount, caster);
			return;
		}
		int currentStage = GetCriticalStage(damageBodyPart);
		if (currentStage < damageBodyPart.CriticalEffectStagesCount)
		{
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<ICriticalEffectStageChangeFailed>)delegate(ICriticalEffectStageChangeFailed h)
			{
				h.HandleCriticalEffectStageChangeFailed(damageBodyPart, currentStage + 1);
			}, isCheckRuntime: true);
		}
	}

	public void RemoveCriticalEffectStages(BlueprintBodyPart damageBodyPart, int amount, MechanicEntity caster)
	{
		if (amount > 0)
		{
			ShiftCriticalEffectStage(damageBodyPart, -amount, caster);
		}
	}

	public void ReduceCriticalEffectsStageBy(int amount, MechanicEntity caster)
	{
		foreach (BlueprintBodyPart bodyPart in base.Owner.BodyParts)
		{
			RemoveCriticalEffectStages(bodyPart, Math.Min(GetCriticalStage(bodyPart), amount), caster);
		}
	}

	public int GetCriticalStage(BlueprintBodyPart damageBodyPart)
	{
		BpRef<BlueprintBuff> criticalEffect = damageBodyPart.CriticalEffect;
		if ((object)criticalEffect == null)
		{
			return 0;
		}
		return base.Owner.Buffs.Get((BlueprintBuff?)criticalEffect)?.Rank ?? 0;
	}

	public static void RestUnit(BaseUnitEntity unit)
	{
		if (unit.LifeState.IsDead)
		{
			unit.LifeState.Resurrect();
			unit.Position = Game.Instance.Player.MainCharacter.Entity.Position;
		}
		Rulebook.Trigger(RuleHealDamage.Setup(unit, unit).Base(unit.Health.HitPoints.ModifiedValue).Create());
		unit.CombatState.LastStraightMoveLength = 0;
		unit.CombatState.LastDiagonalCount = 0;
		unit.CombatState.ResetActionAndMovementPoints();
		unit.CombatState.AttackInRoundCount = 0;
		unit.CombatState.AttackedInRoundCount = 0;
		unit.CombatState.HitInRoundCount = 0;
		unit.CombatState.GotHitInRoundCount = 0;
		unit.GetAbilityCooldownsOptional()?.Clear();
		unit.GetTwoWeaponFightingOptional()?.ResetAttacks();
		TryResetDebuffs(unit);
		foreach (ModifiableValueAttributeStat attribute in unit.Attributes)
		{
			attribute.Damage = 0;
			attribute.Drain = 0;
		}
		unit.Health.HealAll();
	}

	private static void TryResetDebuffs(BaseUnitEntity unit)
	{
		SkillCheckRoot skillCheckRoot = ConfigRoot.Instance.SkillCheckRoot;
		unit.Buffs.Remove(skillCheckRoot.Fatigued);
		unit.Buffs.Remove(skillCheckRoot.Disturbed);
		unit.Buffs.Remove(skillCheckRoot.Perplexed);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_DamageReceivedThisTurn);
		result.Append(ref m_LastTurnWhenReceiveDamage);
		result.Append(ref m_ConsecutiveOutOfCombatTurnsWithoutDamage);
		List<TemporaryHitPointsData> temporaryHitPoints = m_TemporaryHitPoints;
		if (temporaryHitPoints != null)
		{
			for (int i = 0; i < temporaryHitPoints.Count; i++)
			{
				Hash128 val2 = ClassHasher<TemporaryHitPointsData>.GetHash128(temporaryHitPoints[i]);
				result.Append(ref val2);
			}
		}
		result.Append(ref m_MissingHpFraction);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartHealth source = new PartHealth();
		result = Unsafe.As<PartHealth, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartHealth>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "m_DamageReceivedThisTurn", ref m_DamageReceivedThisTurn, state);
		formatter.UnmanagedField(1, "m_LastTurnWhenReceiveDamage", ref m_LastTurnWhenReceiveDamage, state);
		formatter.UnmanagedField(2, "m_ConsecutiveOutOfCombatTurnsWithoutDamage", ref m_ConsecutiveOutOfCombatTurnsWithoutDamage, state);
		formatter.Field(3, "m_TemporaryHitPoints", ref m_TemporaryHitPoints, state);
		formatter.UnmanagedField(4, "m_MissingHpFraction", ref m_MissingHpFraction, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartHealth>();
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
				m_DamageReceivedThisTurn = formatter.ReadUnmanaged<int>(state);
				break;
			case 1:
				m_LastTurnWhenReceiveDamage = formatter.ReadUnmanaged<int>(state);
				break;
			case 2:
				m_ConsecutiveOutOfCombatTurnsWithoutDamage = formatter.ReadUnmanaged<int>(state);
				break;
			case 3:
				m_TemporaryHitPoints = formatter.ReadPackable<List<TemporaryHitPointsData>>(state);
				break;
			case 4:
				m_MissingHpFraction = formatter.ReadUnmanaged<float>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
