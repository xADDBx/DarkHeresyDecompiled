using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Code.Framework.VO;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;

namespace Kingmaker.Visual.Sound;

public class LifeStateAsksController : BaseAsksController, ITickUnitAsksController, IUnitAsksController, IDisposable, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, ISubscriber, IDamageHandler
{
	private class UnitDamage
	{
		public readonly RuleDealDamage LastHandledDamage;

		public readonly UnitLifeState UnitLifeState;

		public readonly bool IsEnemyOfInitiator;

		public readonly bool IsPlayerFaction;

		public readonly BaseUnitEntity Initiator;

		public UnitDamage(RuleDealDamage lastHandledDamage, BaseUnitEntity target)
		{
			LastHandledDamage = lastHandledDamage;
			UnitLifeState = target.LifeState.State;
			IsEnemyOfInitiator = lastHandledDamage != null && lastHandledDamage.InitiatorUnit != null && lastHandledDamage.Target.IsEnemy(lastHandledDamage.InitiatorUnit);
			IsPlayerFaction = target.IsPlayerFaction;
			Initiator = lastHandledDamage?.InitiatorUnit;
		}
	}

	private class AskWrapperUnitPair
	{
		public AskWrapper AskWrapper { get; private set; }

		public BaseUnitEntity Unit { get; private set; }

		public AskWrapperUnitPair(AskWrapper wrapper, BaseUnitEntity unit)
		{
			AskWrapper = wrapper;
			Unit = unit;
		}
	}

	private readonly Dictionary<BaseUnitEntity, UnitDamage> m_DamagedUnits = new Dictionary<BaseUnitEntity, UnitDamage>();

	private readonly Dictionary<BaseUnitEntity, int> m_EnemyKillsCounter = new Dictionary<BaseUnitEntity, int>();

	private readonly List<AskWrapperUnitPair> m_Barks = new List<AskWrapperUnitPair>();

	private readonly List<AskWrapperUnitPair> m_PersonalizedBarks = new List<AskWrapperUnitPair>();

	public override void Dispose()
	{
		base.Dispose();
		m_DamagedUnits.Clear();
		m_EnemyKillsCounter.Clear();
		m_Barks.Clear();
		m_PersonalizedBarks.Clear();
	}

	private void CacheDamage(BaseUnitEntity entity, UnitDamage lastDamageData)
	{
		if (lastDamageData != null)
		{
			if (m_DamagedUnits.ContainsKey(entity))
			{
				m_DamagedUnits[entity] = lastDamageData;
			}
			else
			{
				m_DamagedUnits.Add(entity, lastDamageData);
			}
		}
	}

	void IUnitLifeStateChanged.HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity is UnitEntity && prevLifeState == UnitLifeState.Conscious && baseUnitEntity.LifeState.State != 0)
		{
			CacheDamage(EventInvokerExtensions.BaseUnitEntity, new UnitDamage(baseUnitEntity.Health.LastHandledDamage, baseUnitEntity));
		}
	}

	void IDamageHandler.HandleDamageDealt(RuleDealDamage dealDamage)
	{
		if (dealDamage.ConcreteTarget.View != null && dealDamage.ConcreteTarget is UnitEntity unitEntity)
		{
			CacheDamage(unitEntity, new UnitDamage(dealDamage, unitEntity));
		}
	}

	public void Tick()
	{
		BaseUnitEntity key;
		foreach (KeyValuePair<BaseUnitEntity, UnitDamage> damagedUnit in m_DamagedUnits)
		{
			damagedUnit.Deconstruct(out key, out var value);
			BaseUnitEntity baseUnitEntity = key;
			UnitDamage unitDamage = value;
			if (baseUnitEntity == null)
			{
				continue;
			}
			CacheEnemyKills(unitDamage);
			if (baseUnitEntity.IsDisposed || baseUnitEntity.IsDisposingNow || !baseUnitEntity.CanSpeakAsks() || (unitDamage.LastHandledDamage?.DisableFxAndSound ?? false))
			{
				continue;
			}
			if (baseUnitEntity.LifeState.IsConscious)
			{
				using (EvalContext.PushAsksContext(unitDamage.Initiator, baseUnitEntity))
				{
					if (unitDamage.LastHandledDamage != null && unitDamage.LastHandledDamage.Reason.Fact?.Blueprint == VOSettings.Instance.PariahFeature?.Blueprint)
					{
						SpeakPainPariah(baseUnitEntity);
					}
					if (unitDamage.LastHandledDamage?.ResultArmorCrack ?? false)
					{
						SpeakArmorBroken(baseUnitEntity);
					}
					else
					{
						SpeakGetDamage(baseUnitEntity);
					}
				}
			}
			else if (baseUnitEntity.LifeState.IsUnconscious)
			{
				SpeakBecomeUnconscious(baseUnitEntity, unitDamage);
			}
			else if (baseUnitEntity.LifeState.IsDead)
			{
				SpeakBecomeDead(baseUnitEntity);
			}
		}
		m_DamagedUnits.Clear();
		foreach (KeyValuePair<BaseUnitEntity, int> item in m_EnemyKillsCounter)
		{
			item.Deconstruct(out key, out var value2);
			BaseUnitEntity baseUnitEntity2 = key;
			int num = value2;
			if (baseUnitEntity2?.View != null && baseUnitEntity2.View.Asks != null)
			{
				AskWrapper askWrapper = baseUnitEntity2.View.Asks.EnemyDeath;
				if (num >= UnitAsksHelper.EnemyMassDeathKillsCount && baseUnitEntity2.View.Asks.EnemyMassDeath.HasBarks)
				{
					askWrapper = baseUnitEntity2.View.Asks?.EnemyMassDeath;
				}
				using (EvalContext.PushAsksContext(baseUnitEntity2, baseUnitEntity2))
				{
					askWrapper.Schedule();
				}
			}
		}
		m_EnemyKillsCounter.Clear();
	}

	private static void SpeakBecomeDead(BaseUnitEntity unit)
	{
		using (EvalContext.PushAsksContext(unit, unit))
		{
			unit.View.Asks?.Death.Schedule();
		}
	}

	private void SpeakBecomeUnconscious(BaseUnitEntity unit, UnitDamage damageData)
	{
		using (EvalContext.PushAsksContext(damageData.Initiator, unit))
		{
			if (damageData.IsPlayerFaction)
			{
				HandlePartyMemberUnconscious(unit);
			}
			unit.View.Asks?.Unconscious.Schedule();
		}
	}

	private static void SpeakGetDamage(BaseUnitEntity target)
	{
		target.View.Asks?.Pain.Schedule();
	}

	private static void SpeakArmorBroken(BaseUnitEntity unit)
	{
		unit.View.Asks?.BrokenArmour.Schedule();
	}

	private static void SpeakPainPariah(BaseUnitEntity unit)
	{
		unit.View.Asks?.PainPariahKeystone.Schedule();
	}

	private void CacheEnemyKills(UnitDamage damage)
	{
		RuleDealDamage lastHandledDamage = damage.LastHandledDamage;
		if (lastHandledDamage != null && damage.UnitLifeState == UnitLifeState.Dead && damage.IsEnemyOfInitiator && lastHandledDamage.InitiatorUnit != null && lastHandledDamage.InitiatorUnit.CanSpeakAsks())
		{
			if (m_EnemyKillsCounter.ContainsKey(lastHandledDamage.InitiatorUnit))
			{
				m_EnemyKillsCounter[lastHandledDamage.InitiatorUnit]++;
			}
			else
			{
				m_EnemyKillsCounter.Add(lastHandledDamage.InitiatorUnit, 1);
			}
		}
	}

	private static AskWrapper GetPartyMemberUnconsciousBark(UnitAsksManager asksComponent, MechanicEntity unconsciousUnit)
	{
		AskWrapper[] partyMemberUnconsciousPersonalized = asksComponent.PartyMemberUnconsciousPersonalized;
		foreach (AskWrapper askWrapper in partyMemberUnconsciousPersonalized)
		{
			if (askWrapper.HasBarks && !askWrapper.IsOnCooldown && askWrapper.AsksSet is PersonalizedAsk personalizedAsk && Enumerable.Any(personalizedAsk.UnitReferences, (BlueprintUnitReference unitReference) => unconsciousUnit.Blueprint is BlueprintUnit blueprintUnit && blueprintUnit.CheckEqualsWithPrototype(unitReference.Get())))
			{
				return askWrapper;
			}
		}
		using (EvalContext.PushAsksContext(unconsciousUnit, asksComponent.Unit))
		{
			return asksComponent.PartyMemberUnconsciousGeneral;
		}
	}

	private void HandlePartyMemberUnconscious(BaseUnitEntity unit)
	{
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			if (partyAndPet == null || !partyAndPet.LifeState.IsConscious || partyAndPet.View == null || partyAndPet.View.Asks == null)
			{
				continue;
			}
			AskWrapper partyMemberUnconsciousBark = GetPartyMemberUnconsciousBark(partyAndPet.View.Asks, unit);
			if (partyMemberUnconsciousBark != null)
			{
				if (partyMemberUnconsciousBark.AsksSet is PersonalizedAsk)
				{
					m_PersonalizedBarks.Add(new AskWrapperUnitPair(partyMemberUnconsciousBark, partyAndPet));
				}
				else
				{
					m_Barks.Add(new AskWrapperUnitPair(partyMemberUnconsciousBark, partyAndPet));
				}
			}
		}
		AskWrapperUnitPair askWrapperUnitPair = m_PersonalizedBarks.Random(PFStatefulRandom.Visuals.UnitAsks);
		if (askWrapperUnitPair != null)
		{
			using (EvalContext.PushAsksContext(unit, askWrapperUnitPair.Unit))
			{
				askWrapperUnitPair.AskWrapper?.Schedule();
			}
		}
		else
		{
			AskWrapperUnitPair askWrapperUnitPair2 = m_Barks.Random(PFStatefulRandom.Visuals.UnitAsks);
			if (askWrapperUnitPair2 != null)
			{
				using (EvalContext.PushAsksContext(unit, askWrapperUnitPair2.Unit))
				{
					askWrapperUnitPair2.AskWrapper?.Schedule();
				}
			}
		}
		m_Barks.Clear();
		m_PersonalizedBarks.Clear();
	}
}
