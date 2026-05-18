using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.Gameplay.Features.Encounter.Events;
using Kingmaker.Gameplay.Features.Morale;
using Kingmaker.Gameplay.Features.Morale.Utility;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Visual.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;

namespace Kingmaker.Code.Gameplay.Controllers;

public class MoraleController : IControllerTick, IController, IControllerEnable, IControllerReset, ITurnBasedModeHandler, ISubscriber, IUnitDeathHandler, IDamageHandler, IMoraleValueHandler, ISubscriber<IMechanicEntity>, IPreparationTurnEndHandler, IJoinEncounterHandler, ISubscriber<MechanicEntity>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, IUnitFactionHandler, IGlobalRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, IGlobalRulebookSubscriber
{
	public class MoralePrediction : IUIUnitMoraleData
	{
		public int Morale { get; set; }

		public int MinValue => Settings.MinValue;

		public int MaxValue => Settings.MaxValue;

		public MoralePhaseType MoralePhase { get; set; }
	}

	private bool _powerBalanceRecalculateRequested;

	private static MoraleRoot Settings => MoraleRoot.Instance;

	private MoraleDataPart Data => Game.Instance.Player.MoraleData;

	public List<MoraleGroup> MoraleGroups => Data.MoraleGroups;

	public UnitVisualSettings.MusicCombatState ForcedMusicCombatState
	{
		get
		{
			return Data.ForcedMusicCombatState;
		}
		set
		{
			Data.ForcedMusicCombatState = value;
		}
	}

	public bool AreAllLeadersDeadOrUnconscious => MoraleGroups.AllItems((MoraleGroup i) => i.IsLeaderDeadOrUnconscious);

	public TickType GetTickType()
	{
		return TickType.EndOfFrame;
	}

	void IControllerTick.Tick()
	{
		if (_powerBalanceRecalculateRequested)
		{
			_powerBalanceRecalculateRequested = false;
			RecalculatePowerBalance();
			ApplyPowerBalance();
			EventBus.RaiseEvent(delegate(IPowerBalanceHandler x)
			{
				x.HandlePowerBalanceRecalculated();
			});
		}
	}

	void IControllerReset.OnReset()
	{
		_powerBalanceRecalculateRequested = false;
	}

	void IControllerEnable.OnEnable()
	{
		ActiveEncounter current = ActiveEncounter.Current;
		if (current == null)
		{
			return;
		}
		foreach (BaseUnitEntity participant in current.Participants)
		{
			UpdateMoraleGroupIfNecessary(participant);
		}
	}

	private void ChooseLeaders()
	{
		ClearLeaders();
		RegisterLeader(Game.Instance.Player.MainCharacterEntity);
		SelectMostDifficultEnemyAsLeader();
	}

	private void ClearLeaders()
	{
		foreach (MoraleGroup moraleGroup in MoraleGroups)
		{
			moraleGroup.ClearLeader();
		}
	}

	public void RegisterLeader(BaseUnitEntity unit)
	{
		MoraleGroups.FirstItem((MoraleGroup i) => i.Contains(unit))?.SetLeader(unit);
	}

	public void UnregisterLeader(BaseUnitEntity unit)
	{
		MoraleGroups.FirstItem((MoraleGroup i) => i.Contains(unit) && i.Leader == unit)?.ClearLeader();
	}

	private void SelectMostDifficultEnemyAsLeader()
	{
		foreach (MoraleGroup moraleGroup in MoraleGroups)
		{
			BaseUnitEntity baseUnitEntity = null;
			int num = int.MinValue;
			foreach (BaseUnitEntity unit in moraleGroup.Units)
			{
				if ((bool)unit.Features.MoraleLeader)
				{
					baseUnitEntity = unit;
					break;
				}
				int unitDifficulty = MoraleRoot.Instance.GetUnitDifficulty(unit.Blueprint.DifficultyType);
				if (unitDifficulty > num)
				{
					num = unitDifficulty;
					baseUnitEntity = unit;
				}
			}
			if (baseUnitEntity != null)
			{
				RegisterLeader(baseUnitEntity);
			}
		}
	}

	private bool IsLeader(BaseUnitEntity unit)
	{
		foreach (MoraleGroup moraleGroup in MoraleGroups)
		{
			if (moraleGroup.Leader == unit)
			{
				return true;
			}
		}
		return false;
	}

	public float GetPlayerPowerBalanceRatio()
	{
		MoraleGroup moraleGroup = MoraleGroups.FirstOrDefault((MoraleGroup g) => g.IsPlayerGroup);
		if (moraleGroup == null || moraleGroup.MostPowerfulEnemy <= 0f)
		{
			return 0f;
		}
		return moraleGroup.PowerValue / moraleGroup.MostPowerfulEnemy;
	}

	private void RequestUpdatePowerBalance()
	{
		_powerBalanceRecalculateRequested = true;
	}

	private void RecalculatePowerBalance()
	{
		foreach (MoraleGroup group in MoraleGroups)
		{
			group.PowerValue = 0f;
			foreach (BaseUnitEntity unit in group.Units)
			{
				group.PowerValue += unit.CalculatePowerBalanceContribution();
			}
			EventBus.RaiseEvent(delegate(IPowerBalanceHandler h)
			{
				h.HandlePowerBalanceValueUpdate(group);
			});
		}
	}

	private void ApplyPowerBalance()
	{
		for (int num = MoraleGroups.Count - 1; num >= 0; num--)
		{
			MoraleGroup moraleGroup = MoraleGroups[num];
			float powerValue = moraleGroup.PowerValue;
			float num2 = 0f;
			foreach (MoraleGroup moraleGroup2 in MoraleGroups)
			{
				if (moraleGroup != moraleGroup2 && moraleGroup.IsEnemy(moraleGroup2))
				{
					num2 = Math.Max(num2, moraleGroup2.PowerValue);
				}
			}
			moraleGroup.MostPowerfulEnemy = num2;
			if (num2 >= powerValue * MoraleRoot.Instance.ShatteredPowerBalanceMultiplier)
			{
				SetPowerBalanceState(moraleGroup, PowerBalanceState.Shattered);
			}
			else if (num2 >= powerValue * MoraleRoot.Instance.LosingBattlePowerBalanceMultiplier)
			{
				SetPowerBalanceState(moraleGroup, PowerBalanceState.LosingBattle);
			}
			else
			{
				SetPowerBalanceState(moraleGroup, PowerBalanceState.Regular);
			}
		}
	}

	private static void SetPowerBalanceState(MoraleGroup group, PowerBalanceState newState)
	{
		if (newState != group.PowerBalanceState)
		{
			group.PowerBalanceState = newState;
			if (!Game.Instance.Controllers.TurnController.IsPreparationTurn)
			{
				UpdatePowerBalanceBuffsOnGroup(group);
			}
			EventBus.RaiseEvent(delegate(IPowerBalanceHandler h)
			{
				h.HandlePowerBalanceStateUpdate(group, newState);
			});
		}
	}

	private static void UpdatePowerBalanceBuffsOnGroup(MoraleGroup group)
	{
		List<BaseUnitEntity> list;
		using (group.Units.ToPooledList(out list))
		{
			switch (group.PowerBalanceState)
			{
			case PowerBalanceState.Regular:
			{
				for (int num2 = list.Count - 1; num2 >= 0; num2--)
				{
					BaseUnitEntity baseUnitEntity2 = list[num2];
					baseUnitEntity2.Buffs.Remove(Settings.LosingBattleBuff);
					baseUnitEntity2.Buffs.Remove(Settings.ShatteredBuff);
				}
				break;
			}
			case PowerBalanceState.LosingBattle:
			{
				for (int num3 = list.Count - 1; num3 >= 0; num3--)
				{
					BaseUnitEntity baseUnitEntity3 = list[num3];
					baseUnitEntity3.Buffs.Remove(Settings.ShatteredBuff);
					if (!baseUnitEntity3.Buffs.Contains((BlueprintBuff?)Settings.LosingBattleBuff))
					{
						baseUnitEntity3.Buffs.Add(Settings.LosingBattleBuff, null, null, BuffEndCondition.CombatEnd);
					}
				}
				break;
			}
			case PowerBalanceState.Shattered:
			{
				for (int num = list.Count - 1; num >= 0; num--)
				{
					BaseUnitEntity baseUnitEntity = list[num];
					baseUnitEntity.Buffs.Remove(Settings.LosingBattleBuff);
					if (!baseUnitEntity.Buffs.Contains((BlueprintBuff?)Settings.ShatteredBuff))
					{
						baseUnitEntity.Buffs.Add(Settings.ShatteredBuff, null, null, BuffEndCondition.CombatEnd);
					}
				}
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public PowerBalanceState GetPowerBalanceState(MechanicEntity entity)
	{
		BaseUnitEntity unit = entity as BaseUnitEntity;
		if (unit == null)
		{
			return PowerBalanceState.Regular;
		}
		return MoraleGroups.FirstItem((MoraleGroup i) => i.Contains(unit))?.PowerBalanceState ?? PowerBalanceState.Regular;
	}

	public IUIUnitMoraleData GetMoralePrediction(BaseUnitEntity unit)
	{
		RuleCalculateMoraleChange ruleCalculateMoraleChange = new RuleCalculateMoraleChange(unit, unit, MoraleEventType.TurnStart);
		Rulebook.Trigger(ruleCalculateMoraleChange);
		MoralePhaseType moralePhase = MoralePhaseType.Regular;
		if (unit.Morale.Value + ruleCalculateMoraleChange.ResultDelta >= Settings.MaxValue)
		{
			moralePhase = MoralePhaseType.Heroic;
		}
		else if (unit.Morale.Value + ruleCalculateMoraleChange.ResultDelta <= Settings.MinValue && ruleCalculateMoraleChange.ResultDelta < 0)
		{
			moralePhase = MoralePhaseType.Broken;
		}
		return new MoralePrediction
		{
			Morale = unit.Morale.Value + ruleCalculateMoraleChange.ResultDelta,
			MoralePhase = moralePhase
		};
	}

	private void UpdateMoraleGroupIfNecessary(BaseUnitEntity unit)
	{
		if (unit.HasMechanicFeature(MechanicsFeatureType.DoNotUseMorale))
		{
			return;
		}
		MoraleGroup moraleGroup = MoraleGroups.FirstItem((MoraleGroup i) => i.Contains(unit));
		if (moraleGroup != null && moraleGroup.IsSuitableFor(unit))
		{
			return;
		}
		moraleGroup?.Remove(unit);
		MoraleGroup moraleGroup2 = MoraleGroups.FirstItem((MoraleGroup i) => i.ContainsCombatGroup(unit.CombatGroup.Group)) ?? MoraleGroups.FirstItem((MoraleGroup i) => i.IsSuitableFor(unit));
		if (moraleGroup2 == null)
		{
			int id = ((MoraleGroups.Count == 0) ? 1 : (MoraleGroups.Max((MoraleGroup i) => i.ID) + 1));
			MoraleGroups.Add(moraleGroup2 = new MoraleGroup(id));
		}
		moraleGroup2.Add(unit);
		RequestUpdatePowerBalance();
	}

	private void HandleUnitDeath(MechanicEntity killer, BaseUnitEntity killedUnit)
	{
		if (killer is BaseUnitEntity baseUnitEntity && baseUnitEntity.IsEnemy(killedUnit))
		{
			GainMoraleForKillingEnemy(baseUnitEntity, killedUnit);
		}
		foreach (BaseUnitEntity item in GetAlliesForMoraleLoss(killedUnit))
		{
			LoseMoraleForAllyDeath(item, killer, killedUnit);
		}
		RequestUpdatePowerBalance();
	}

	private static IEnumerable<BaseUnitEntity> GetAlliesForMoraleLoss(BaseUnitEntity killedUnit)
	{
		bool killedUnitIsCompanion = killedUnit.GetOptional<UnitPartCompanion>()?.IsCompanion ?? false;
		foreach (BaseUnitEntity aliveAlly in GetAliveAllies(killedUnit))
		{
			if ((!killedUnit.Features.MoraleDisposable || (bool)aliveAlly.Features.MoraleDisposable) && (!(aliveAlly.GetOptional<UnitPartCompanion>()?.IsCompanion ?? false) || killedUnitIsCompanion) && aliveAlly.DistanceToInCells(killedUnit) <= MoraleRoot.Instance.MaxDistanceInCellsToMoraleAffect)
			{
				yield return aliveAlly;
			}
		}
	}

	private static IEnumerable<BaseUnitEntity> GetAliveAllies(BaseUnitEntity unit)
	{
		MoraleGroup moraleGroup = Game.Instance.Controllers.MoraleController.MoraleGroups.FirstItem((MoraleGroup i) => i.Contains(unit));
		if (moraleGroup != null)
		{
			return moraleGroup.Units.Where((BaseUnitEntity i) => i != unit && !i.IsDeadOrUnconscious);
		}
		return Enumerable.Empty<BaseUnitEntity>();
	}

	private static void LoseMoraleForAllyDeath(BaseUnitEntity target, MechanicEntity killer, BaseUnitEntity killedUnit)
	{
		bool flag = Game.Instance.Controllers.MoraleController.IsLeader(killedUnit);
		int allyDeathMoraleChange = Settings.GetAllyDeathMoraleChange(killedUnit.Blueprint.DifficultyType, flag);
		Rulebook.Trigger(new RulePerformMoraleChange(killer, target, flag ? (MoraleEventType.AllyDeath | MoraleEventType.LeaderAllyDeath) : MoraleEventType.AllyDeath, allyDeathMoraleChange));
	}

	private static void GainMoraleForKillingEnemy(BaseUnitEntity target, BaseUnitEntity killedUnit)
	{
		bool flag = Game.Instance.Controllers.MoraleController.IsLeader(killedUnit);
		int enemyDeathMoraleChange = Settings.GetEnemyDeathMoraleChange(killedUnit.Blueprint.DifficultyType, flag);
		Rulebook.Trigger(new RulePerformMoraleChange(target, target, flag ? (MoraleEventType.EnemyDeath | MoraleEventType.LeaderEnemyDeath) : MoraleEventType.EnemyDeath, enemyDeathMoraleChange));
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool inCombat)
	{
		if (inCombat)
		{
			ChooseLeaders();
			return;
		}
		ClearLeaders();
		MoraleGroups.Clear();
		Data.ForcedMusicCombatState = UnitVisualSettings.MusicCombatState.None;
	}

	void IUnitDeathHandler.HandleUnitDeath(AbstractUnitEntity deadUnit)
	{
		RequestUpdatePowerBalance();
	}

	void IDamageHandler.HandleDamageDealt(RuleDealDamage dealDamage)
	{
		RequestUpdatePowerBalance();
	}

	void IMoraleValueHandler.HandleMoraleValueChanged(int delta, bool hasCriticalEffect)
	{
		RequestUpdatePowerBalance();
	}

	void IPreparationTurnEndHandler.HandleEndPreparationTurn()
	{
		foreach (MoraleGroup moraleGroup in MoraleGroups)
		{
			UpdatePowerBalanceBuffsOnGroup(moraleGroup);
		}
	}

	void IJoinEncounterHandler.HandleJoinEncounter()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null)
		{
			UpdateMoraleGroupIfNecessary(baseUnitEntity);
		}
	}

	void IUnitCombatHandler.HandleUnitJoinCombat()
	{
	}

	void IUnitCombatHandler.HandleUnitLeaveCombat()
	{
		RequestUpdatePowerBalance();
	}

	void IUnitFactionHandler.HandleFactionChanged()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && baseUnitEntity.IsInCombat)
		{
			UpdateMoraleGroupIfNecessary(baseUnitEntity);
		}
	}

	void IRulebookHandler<RuleDealDamage>.OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	void IRulebookHandler<RuleDealDamage>.OnEventDidTrigger(RuleDealDamage evt)
	{
		if (evt != null && evt.HPBeforeDamage > 0)
		{
			PartHealth targetHealth = evt.TargetHealth;
			if (targetHealth != null && targetHealth.HitPointsLeft <= 0 && evt.ConcreteTarget is BaseUnitEntity killedUnit)
			{
				HandleUnitDeath(evt.Initiator, killedUnit);
			}
		}
	}

	public bool IsAllyLeaderDead(BaseUnitEntity owner)
	{
		return MoraleGroups.FirstItem((MoraleGroup i) => i.Contains(owner))?.IsLeaderDeadOrUnconscious ?? false;
	}

	public bool AreAllEnemyLeadersDead(BaseUnitEntity owner)
	{
		foreach (MoraleGroup moraleGroup in MoraleGroups)
		{
			if (!moraleGroup.Contains(owner) && moraleGroup.IsPlayerEnemy != owner.IsPlayerEnemy && !moraleGroup.IsLeaderDeadOrUnconscious)
			{
				return false;
			}
		}
		return true;
	}

	public UnitVisualSettings.MusicCombatState GetMusicCombatStateByPowerBalance()
	{
		if (Data.ForcedMusicCombatState != UnitVisualSettings.MusicCombatState.None)
		{
			return Data.ForcedMusicCombatState;
		}
		if (MoraleGroups.Count == 0)
		{
			return UnitVisualSettings.MusicCombatState.Regular;
		}
		if (MoraleGroups.Count == 1 && MoraleGroups[0].IsPlayerGroup)
		{
			return UnitVisualSettings.MusicCombatState.Winning;
		}
		MoraleGroup moraleGroup = MoraleGroups.FirstItem((MoraleGroup i) => i.IsPlayerGroup);
		if (moraleGroup == null)
		{
			return UnitVisualSettings.MusicCombatState.Regular;
		}
		PowerBalanceState powerBalanceState = moraleGroup.PowerBalanceState;
		PowerBalanceState powerBalanceState2 = PowerBalanceState.Shattered;
		foreach (MoraleGroup moraleGroup2 in MoraleGroups)
		{
			if (!moraleGroup2.IsPlayerGroup && moraleGroup.IsEnemy(moraleGroup2) && moraleGroup2.PowerBalanceState < powerBalanceState2)
			{
				powerBalanceState2 = moraleGroup2.PowerBalanceState;
			}
		}
		switch (powerBalanceState)
		{
		case PowerBalanceState.Regular:
			switch (powerBalanceState2)
			{
			case PowerBalanceState.Regular:
				return UnitVisualSettings.MusicCombatState.Regular;
			case PowerBalanceState.LosingBattle:
			case PowerBalanceState.Shattered:
				return UnitVisualSettings.MusicCombatState.Winning;
			}
			break;
		case PowerBalanceState.LosingBattle:
		case PowerBalanceState.Shattered:
			return UnitVisualSettings.MusicCombatState.Losing;
		}
		return UnitVisualSettings.MusicCombatState.Regular;
	}
}
