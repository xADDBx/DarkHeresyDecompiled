using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Controllers.Combat;

[Obsolete("WH2-20084")]
public class UnitCombatLeaveController : IControllerTick, IController
{
	private static HashSet<BaseUnitEntity> s_SpawnPsychicPhenomenaCache = new HashSet<BaseUnitEntity>();

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		TickGroups(Game.Instance.IsModeActive(GameModeType.GlobalMap));
	}

	public static void TickGroups(bool ignoreTimer)
	{
		bool isInCombat = Game.Instance.Player.IsInCombat;
		foreach (UnitGroup unitGroup in Game.Instance.UnitGroups)
		{
			TickGroup(unitGroup, ignoreTimer);
		}
		int num = 0;
		s_SpawnPsychicPhenomenaCache.Clear();
		foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.EntityPools.AllBaseAwakeUnits)
		{
			if (allBaseAwakeUnit.IsInCombat && ((bool)allBaseAwakeUnit.Features.IsIgnoredByCombat || (bool)allBaseAwakeUnit.Passive))
			{
				allBaseAwakeUnit.CombatState.LeaveCombat();
			}
			else if (allBaseAwakeUnit.IsInCombat && allBaseAwakeUnit.IsPlayerEnemy)
			{
				if (allBaseAwakeUnit.SpawnFromPsychicPhenomena)
				{
					s_SpawnPsychicPhenomenaCache.Add(allBaseAwakeUnit);
				}
				num++;
			}
		}
		if (s_SpawnPsychicPhenomenaCache.Count > 0 && num == s_SpawnPsychicPhenomenaCache.Count)
		{
			foreach (BaseUnitEntity item in s_SpawnPsychicPhenomenaCache)
			{
				item.LifeState.ManualDeath();
			}
		}
		s_SpawnPsychicPhenomenaCache.Clear();
		if (!isInCombat || Game.Instance.Player.IsInCombat)
		{
			return;
		}
		foreach (BaseUnitEntity allCharacter in Game.Instance.Player.AllCharacters)
		{
			allCharacter.Buffs.OnCombatEnded();
			allCharacter.Abilities.OnCombatEnd();
		}
		EventBus.RaiseEvent(delegate(IPartyCombatHandler h)
		{
			h.HandlePartyCombatStateChanged(inCombat: false);
		});
	}

	private static void TickGroup(UnitGroup group, bool ignoreLeaveTimer)
	{
		if ((bool)group.IsInCombat && ShouldLeaveCombat(group))
		{
			group.LeaveCombatTimer += Game.Instance.Controllers.TimeController.GameDeltaTime;
			if (!ignoreLeaveTimer)
			{
				return;
			}
			group.LeaveCombatTimer = 0f;
			for (int i = 0; i < group.Count; i++)
			{
				BaseUnitEntity baseUnitEntity = group[i];
				if (baseUnitEntity != null && baseUnitEntity.IsInCombat)
				{
					baseUnitEntity.CombatState.LeaveCombat();
				}
			}
			if (group.IsPlayerParty && !Game.Instance.Player.IsInCombat)
			{
				KillAllBetrayalUnits();
			}
		}
		else
		{
			group.LeaveCombatTimer = 0f;
		}
	}

	private static bool ShouldLeaveCombat(UnitGroup group)
	{
		if (Game.Instance.Controllers.TurnController.IsManualCombatTurn)
		{
			return false;
		}
		bool flag = true;
		bool flag2 = false;
		UnitGroupEnumerator enumerator = group.GetEnumerator();
		while (enumerator.MoveNext())
		{
			BaseUnitEntity current = enumerator.Current;
			if (!current.LifeState.IsDead)
			{
				flag = false;
			}
			if (TurnController.IsInTurnBasedCombat() && current.Commands.HasOffensiveCommand())
			{
				flag2 = true;
			}
		}
		if (flag)
		{
			return true;
		}
		if (flag2)
		{
			return false;
		}
		if (group.IsFollowingUnitInCombat)
		{
			return false;
		}
		foreach (UnitGroupMemory.UnitInfo units in group.Memory.UnitsList)
		{
			if (ShouldEngageEnemy(group, units))
			{
				return false;
			}
		}
		return true;
	}

	private static bool ShouldEngageEnemy(UnitGroup group, UnitGroupMemory.UnitInfo enemyInfo)
	{
		BaseUnitEntity unit = enemyInfo.Unit;
		if (unit == null)
		{
			return false;
		}
		if (group.IsEnemy(unit) && !unit.LifeState.IsDead)
		{
			if (group.IsPlayerParty)
			{
				return unit.IsInCombat;
			}
			return true;
		}
		return false;
	}

	private static void KillAllBetrayalUnits()
	{
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.EntityPools.AllBaseUnits)
		{
			if (allBaseUnit.Buffs.Contains((BlueprintBuff?)MoraleRoot.Instance.BetrayalBuff))
			{
				allBaseUnit.LifeState.MarkedForDeath = true;
				allBaseUnit.LifeState.ForceDismember = UnitDismemberType.None;
				UnitLifeController.ForceTickOnUnit(allBaseUnit);
			}
		}
	}
}
