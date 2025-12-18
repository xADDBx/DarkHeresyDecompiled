using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Groups;
using UnityEngine.Pool;

namespace Kingmaker.Gameplay.Features.Encounter;

public sealed class EncounterController : IControllerTick, IController, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		TryStartEncounter();
		ActiveEncounter current = ActiveEncounter.Current;
		if (current != null)
		{
			AddNewParticipants(current);
			EnsureUnitCombatState(current);
			current.TryComplete();
		}
	}

	private static void TryStartEncounter()
	{
		ActiveEncounter current = ActiveEncounter.Current;
		if ((current == null || current.IsDefault) && !Game.Instance.LoadedAreaState.Settings.Peaceful && !Game.Instance.LoadedAreaState.Settings.CapitalPartyMode)
		{
			BlueprintEncounter encounterForParty = GetEncounterForParty();
			if (encounterForParty != null && encounterForParty != current?.Blueprint)
			{
				ActiveEncounter.Start(encounterForParty);
			}
		}
	}

	[CanBeNull]
	private static BlueprintEncounter GetEncounterForParty()
	{
		bool flag = false;
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			if (!CanBeInCombat(item))
			{
				continue;
			}
			foreach (BaseUnitEntity value in item.Vision.CanBeInRange.Values)
			{
				if (!CanBeInCombat(value) || !value.IsEnemy(item) || !value.HasLOS(item))
				{
					continue;
				}
				PartEncounter encounter = value.Encounter;
				if (encounter != null)
				{
					BlueprintEncounter blueprint = encounter.Blueprint;
					if (blueprint != null && !blueprint.IsDefault)
					{
						return blueprint;
					}
				}
				flag = true;
			}
		}
		return flag ? ConfigRoot.Instance.EncounterRoot.DefaultEncounter : null;
	}

	private static void AddNewParticipants(ActiveEncounter encounter)
	{
		HashSet<UnitGroup> value;
		using (CollectionPool<HashSet<UnitGroup>, UnitGroup>.Get(out value))
		{
			foreach (BaseUnitEntity participant in encounter.Participants)
			{
				value.Add(participant.CombatGroup.Group);
				if (!participant.IsInPlayerParty && encounter.IsDefault)
				{
					continue;
				}
				foreach (BaseUnitEntity value2 in participant.Vision.CanBeInRange.Values)
				{
					PartEncounter encounter2 = value2.Encounter;
					if (((encounter2 == null && participant.IsEnemy(value2) && CanBeInCombat(value2)) || (encounter2 != null && !encounter2.Joined && encounter2.Blueprint == encounter.Blueprint)) && value2.HasLOS(participant))
					{
						value.Add(value2.CombatGroup.Group);
					}
				}
			}
			foreach (UnitGroup item in value)
			{
				UnitGroupEnumerator enumerator4 = item.GetEnumerator();
				while (enumerator4.MoveNext())
				{
					BaseUnitEntity current3 = enumerator4.Current;
					PartEncounter encounter3 = current3.Encounter;
					if ((encounter3 == null || !encounter3.Joined) && CanBeInCombat(current3))
					{
						encounter.AddParticipant(current3);
					}
				}
			}
		}
	}

	private static void EnsureUnitCombatState([NotNull] ActiveEncounter encounter)
	{
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.EntityPools.AllBaseUnits)
		{
			if (allBaseUnit.Encounter?.Blueprint == encounter.Blueprint)
			{
				bool flag = CanBeInCombat(allBaseUnit);
				if (!allBaseUnit.IsInCombat && flag)
				{
					allBaseUnit.CombatState.JoinCombat();
					foreach (BaseUnitEntity participant in encounter.Participants)
					{
						if (allBaseUnit.IsEnemy(participant))
						{
							Game.Instance.Controllers.UnitMemoryController.AddToMemory(allBaseUnit, participant);
						}
					}
					Game.Instance.Controllers.UnitMemoryController.UpdateUnit(allBaseUnit);
				}
				else if (allBaseUnit.IsInCombat && !flag)
				{
					allBaseUnit.CombatState.LeaveCombat();
					Game.Instance.Controllers.UnitMemoryController.UpdateUnit(allBaseUnit);
				}
			}
			else if (allBaseUnit.IsInCombat)
			{
				allBaseUnit.CombatState.LeaveCombat();
				Game.Instance.Controllers.UnitMemoryController.UpdateUnit(allBaseUnit);
			}
		}
	}

	private static bool CanBeInCombat(BaseUnitEntity unit)
	{
		if (unit.IsInState && unit.IsConscious && !unit.IsExtra && !unit.Features.IsIgnoredByCombat && !unit.Passive && !unit.Faction.NeverJoinCombat)
		{
			return !unit.IsInLockControlCutscene;
		}
		return false;
	}

	void IUnitSpawnHandler.HandleUnitSpawned()
	{
		ActiveEncounter current = ActiveEncounter.Current;
		if (current != null)
		{
			BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
			PartEncounter partEncounter = baseUnitEntity?.GetOptional<PartEncounter>();
			if (partEncounter != null && !partEncounter.Joined && partEncounter.Blueprint == current.Blueprint)
			{
				current.AddParticipant(baseUnitEntity);
			}
		}
	}
}
