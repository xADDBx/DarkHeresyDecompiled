using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.Gameplay.Controllers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.Morale;
using Kingmaker.Gameplay.Features.Morale.Components;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartMorale : BaseUnitPart, IUIUnitMoraleData, ITurnStartHandler<EntitySubscriber>, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEntitySubscriber, IEventTag<ITurnStartHandler, EntitySubscriber>, ITurnEndHandler<EntitySubscriber>, ITurnEndHandler, IEventTag<ITurnEndHandler, EntitySubscriber>, IUnitCombatHandler<EntitySubscriber>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IUnitCombatHandler, EntitySubscriber>, ICriticalEffectStageChanged<EntitySubscriber>, ICriticalEffectStageChanged, IEventTag<ICriticalEffectStageChanged, EntitySubscriber>, IHashable, IOwlPackable<PartMorale>
{
	[JsonProperty]
	[OwlPackInclude]
	private int m_Morale;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_JustJoinedInCombat;

	[JsonProperty]
	[OwlPackInclude]
	private MoralePhaseType m_MoralePhase;

	[JsonProperty]
	[OwlPackInclude]
	private EntityFactRef<Buff> m_PhaseBuff;

	public readonly CompositeModifiersManager PowerFactorModifiers = new CompositeModifiersManager();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartMorale",
		OldNames = null,
		Fields = new FieldInfo[5]
		{
			new FieldInfo("m_Morale", typeof(int)),
			new FieldInfo("m_JustJoinedInCombat", typeof(bool)),
			new FieldInfo("m_MoralePhase", typeof(MoralePhaseType)),
			new FieldInfo("m_PhaseBuff", typeof(EntityFactRef<Buff>)),
			new FieldInfo("PhaseLockRoundsLeft", typeof(int))
		}
	};

	private static MoraleRoot Settings => MoraleRoot.Instance;

	[JsonProperty]
	[OwlPackInclude]
	public int PhaseLockRoundsLeft { get; private set; }

	public int Value => m_Morale;

	public MoralePhaseType Phase => m_MoralePhase;

	public bool IsPhaseLocked => PhaseLockRoundsLeft > 0;

	public float PowerFactor => (float)PowerFactorModifiers.Apply(100) / 100f;

	int IUIUnitMoraleData.Morale => Value;

	int IUIUnitMoraleData.MinValue => Settings.MinValue;

	int IUIUnitMoraleData.MaxValue => Settings.MaxValue;

	MoralePhaseType IUIUnitMoraleData.MoralePhase => Phase;

	void IUnitCombatHandler.HandleUnitJoinCombat()
	{
		m_JustJoinedInCombat = true;
	}

	void IUnitCombatHandler.HandleUnitLeaveCombat()
	{
		m_Morale = 0;
		m_MoralePhase = MoralePhaseType.Regular;
		PhaseLockRoundsLeft = 0;
		m_JustJoinedInCombat = false;
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		if (base.Owner.HasMechanicFeature(MechanicsFeatureType.DoNotUseMorale) || !isTurnBased)
		{
			return;
		}
		PhaseLockRoundsLeft = Math.Max(0, PhaseLockRoundsLeft - 1);
		if (m_MoralePhase == MoralePhaseType.Regular)
		{
			MoraleEventType moraleEventType = MoraleEventType.TurnStart;
			if (m_JustJoinedInCombat)
			{
				m_JustJoinedInCombat = false;
				moraleEventType |= MoraleEventType.CombatStart;
			}
			Rulebook.Trigger(new RulePerformMoraleChange(base.Owner, base.Owner, moraleEventType));
		}
	}

	void ITurnEndHandler.HandleUnitEndTurn(bool isTurnBased)
	{
		if (!base.Owner.HasMechanicFeature(MechanicsFeatureType.DoNotUseMorale) && isTurnBased)
		{
			bool flag = base.Owner.IsInPlayerParty && m_MoralePhase == MoralePhaseType.Heroic;
			if (m_MoralePhase != 0 && PhaseLockRoundsLeft == 0 && !flag)
			{
				ResetPhase();
			}
		}
	}

	void ICriticalEffectStageChanged.HandleCriticalEffectStageChanged(BlueprintBodyPart bodyPart, int previous, int current)
	{
		if (!base.Owner.HasMechanicFeature(MechanicsFeatureType.DoNotUseMorale) && current > previous && current >= MoraleRoot.Instance.TraumaStackTriggersMoraleDrop)
		{
			int moraleAddOnTraumaStack = MoraleRoot.Instance.MoraleAddOnTraumaStack;
			Rulebook.Trigger(new RulePerformMoraleChange(base.Owner, base.Owner, MoraleEventType.TraumaStacked, moraleAddOnTraumaStack));
		}
	}

	public void SwitchPhase(MoralePhaseType newPhase, [NotNull] MechanicEntity initiator)
	{
		if (!base.Owner.HasMechanicFeature(MechanicsFeatureType.DoNotUseMorale) && m_MoralePhase != newPhase && !MoraleCheats.MoraleDisableChanges)
		{
			m_MoralePhase = newPhase;
			m_Morale = newPhase switch
			{
				MoralePhaseType.Regular => 0, 
				MoralePhaseType.Heroic => Settings.MaxValue, 
				MoralePhaseType.Broken => Settings.MinValue, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			PhaseLockRoundsLeft = ((newPhase == MoralePhaseType.Heroic || newPhase == MoralePhaseType.Broken) ? GetPhaseLockDuration(initiator) : 0);
			UpdatePhaseBuff();
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IMoralePhaseHandler>)delegate(IMoralePhaseHandler h)
			{
				h.HandleMoralePhaseChanged(newPhase);
			}, isCheckRuntime: true);
		}
	}

	public void SetValue(int value, [NotNull] MechanicEntity initiator, bool becauseOfCriticalEffect)
	{
		if (base.Owner.HasMechanicFeature(MechanicsFeatureType.DoNotUseMorale) || m_Morale == value || MoraleCheats.MoraleDisableChanges)
		{
			return;
		}
		if (m_MoralePhase != 0)
		{
			if (!base.Owner.IsInPlayerParty || IsPhaseLocked || m_MoralePhase != MoralePhaseType.Heroic || value >= m_Morale)
			{
				return;
			}
			ResetPhase();
		}
		int prevValue = m_Morale;
		m_Morale = value;
		EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IMoraleValueHandler>)delegate(IMoraleValueHandler h)
		{
			h.HandleMoraleValueChanged(m_Morale - prevValue, becauseOfCriticalEffect);
		}, isCheckRuntime: true);
		MoralePhaseType phaseForValue = GetPhaseForValue(m_Morale);
		if (Phase != phaseForValue)
		{
			SwitchPhase(phaseForValue, initiator);
		}
	}

	private void ResetPhase()
	{
		int prevValue = m_Morale;
		MoralePhaseType moralePhase = m_MoralePhase;
		m_Morale = 0;
		m_MoralePhase = MoralePhaseType.Regular;
		PhaseLockRoundsLeft = 0;
		ClearPhaseBuff();
		if (m_Morale != prevValue)
		{
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IMoraleValueHandler>)delegate(IMoraleValueHandler h)
			{
				h.HandleMoraleValueChanged(m_Morale - prevValue, hasCriticalEffect: false);
			}, isCheckRuntime: true);
		}
		if (m_MoralePhase != moralePhase)
		{
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IMoralePhaseHandler>)delegate(IMoralePhaseHandler h)
			{
				h.HandleMoralePhaseChanged(m_MoralePhase);
			}, isCheckRuntime: true);
		}
	}

	private void ResetValue()
	{
		int prevValue = m_Morale;
		m_Morale = 0;
		if (m_Morale != prevValue)
		{
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IMoraleValueHandler>)delegate(IMoraleValueHandler h)
			{
				h.HandleMoraleValueChanged(m_Morale - prevValue, hasCriticalEffect: false);
			}, isCheckRuntime: true);
		}
	}

	private static MoralePhaseType GetPhaseForValue(int value)
	{
		if (value >= Settings.MaxValue)
		{
			return MoralePhaseType.Heroic;
		}
		if (value <= Settings.MinValue)
		{
			return MoralePhaseType.Broken;
		}
		return MoralePhaseType.Regular;
	}

	private void UpdatePhaseBuff()
	{
		BlueprintBuff blueprintBuff = Phase switch
		{
			MoralePhaseType.Heroic => GetHeroicBuffBlueprint(), 
			MoralePhaseType.Broken => GetBrokenBuffBlueprint(), 
			_ => null, 
		};
		BuffEndCondition buffEndCondition = ((blueprintBuff.GetComponent<MoralePhaseBuffRemainAfterCombat>() == null) ? BuffEndCondition.CombatEnd : BuffEndCondition.RemainAfterCombat);
		Buff fact = m_PhaseBuff.Fact;
		if (fact?.Blueprint != blueprintBuff)
		{
			fact?.Remove();
			m_PhaseBuff = base.Owner.Buffs.Add(blueprintBuff, base.Owner, null, buffEndCondition);
		}
	}

	private int GetPhaseLockDuration([NotNull] MechanicEntity initiator)
	{
		return Rulebook.Trigger(new RuleCalculateMoralePhaseDuration(initiator, base.Owner, Phase, MoraleEventType.ForcedChangeMoralePhase)).Rounds;
	}

	private BlueprintBuff GetHeroicBuffBlueprint()
	{
		BlueprintArmyType army = base.Owner.Blueprint.Army;
		if (army != null)
		{
			BlueprintBuff heroicBuffOverride = army.HeroicBuffOverride;
			if (heroicBuffOverride != null)
			{
				return heroicBuffOverride;
			}
		}
		return MoraleRoot.Instance.HeroicBuff;
	}

	private BlueprintBuff GetBrokenBuffBlueprint()
	{
		if (!base.Owner.IsPlayerEnemy)
		{
			return MoraleRoot.Instance.BrokenBuff;
		}
		BlueprintArmyType army = base.Owner.Blueprint.Army;
		if (army != null)
		{
			BlueprintBuff brokenBuffOverride = army.BrokenBuffOverride;
			if (brokenBuffOverride != null)
			{
				return brokenBuffOverride;
			}
		}
		return MoraleRoot.Instance.GetBuffForDifficulty(base.Owner.Blueprint.DifficultyType) ?? ((BlueprintBuff?)MoraleRoot.Instance.BrokenBuff);
	}

	private void ClearPhaseBuff()
	{
		m_PhaseBuff.Fact?.Remove();
		m_PhaseBuff = null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_Morale);
		result.Append(ref m_JustJoinedInCombat);
		result.Append(ref m_MoralePhase);
		EntityFactRef<Buff> obj = m_PhaseBuff;
		Hash128 val2 = StructHasher<EntityFactRef<Buff>>.GetHash128(ref obj);
		result.Append(ref val2);
		int val3 = PhaseLockRoundsLeft;
		result.Append(ref val3);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartMorale source = new PartMorale();
		result = Unsafe.As<PartMorale, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartMorale>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "m_Morale", ref m_Morale, state);
		formatter.UnmanagedField(1, "m_JustJoinedInCombat", ref m_JustJoinedInCombat, state);
		formatter.EnumField(2, "m_MoralePhase", ref m_MoralePhase, state);
		formatter.Field(3, "m_PhaseBuff", ref m_PhaseBuff, state);
		int value = PhaseLockRoundsLeft;
		formatter.UnmanagedField(4, "PhaseLockRoundsLeft", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartMorale>();
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
				m_Morale = formatter.ReadUnmanaged<int>(state);
				break;
			case 1:
				m_JustJoinedInCombat = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_MoralePhase = formatter.ReadEnum<MoralePhaseType>(state);
				break;
			case 3:
				m_PhaseBuff = formatter.ReadPackable<EntityFactRef<Buff>>(state);
				break;
			case 4:
				PhaseLockRoundsLeft = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
