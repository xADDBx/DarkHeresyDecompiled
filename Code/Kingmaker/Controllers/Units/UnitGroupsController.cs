using System.Collections.Generic;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Groups;

namespace Kingmaker.Controllers.Units;

public class UnitGroupsController : IControllerEnable, IController, IControllerTick, IControllerStart, IControllerStop, IControllerReset, IUnitChangeAttackFactionsHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IUnitFactionHandler, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>
{
	private readonly HashSet<UnitGroup> m_GroupsForUpdateAttackFactions = new HashSet<UnitGroup>();

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		bool flag = false;
		foreach (UnitGroup unitGroup in Game.Instance.UnitGroups)
		{
			if (unitGroup.Empty())
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			Game.Instance.UnitGroups.Cleanup();
		}
		foreach (UnitGroup groupsForUpdateAttackFaction in m_GroupsForUpdateAttackFactions)
		{
			groupsForUpdateAttackFaction.UpdateAttackFactionsCache();
		}
		m_GroupsForUpdateAttackFactions.Clear();
	}

	void IControllerEnable.OnEnable()
	{
		foreach (UnitGroup unitGroup in Game.Instance.UnitGroups)
		{
			unitGroup.UpdateAttackFactionsCache();
		}
	}

	void IControllerStart.OnStart()
	{
		Tick();
	}

	void IControllerStop.OnStop()
	{
		Tick();
	}

	void IControllerReset.OnReset()
	{
		Game.Instance.UnitGroups.Clear();
		m_GroupsForUpdateAttackFactions.Clear();
	}

	void IUnitChangeAttackFactionsHandler.HandleUnitChangeAttackFactions(MechanicEntity unit)
	{
		m_GroupsForUpdateAttackFactions.Add(EventInvokerExtensions.BaseUnitEntity.CombatGroup.Group);
	}

	void IUnitSpawnHandler.HandleUnitSpawned()
	{
		EventInvokerExtensions.BaseUnitEntity?.CombatGroup.UpdateAttackFactionsCache();
	}

	void IUnitHandler.HandleUnitDestroyed()
	{
		EventInvokerExtensions.BaseUnitEntity?.CombatGroup.UpdateAttackFactionsCache();
	}

	void IUnitHandler.HandleUnitDeath()
	{
	}

	void IUnitFactionHandler.HandleFactionChanged()
	{
		m_GroupsForUpdateAttackFactions.Add(EventInvokerExtensions.BaseUnitEntity.CombatGroup.Group);
	}
}
