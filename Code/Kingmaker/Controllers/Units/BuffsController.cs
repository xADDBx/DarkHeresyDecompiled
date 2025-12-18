using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Controllers.Units;

public class BuffsController : IControllerTick, IController, IControllerEnable, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, ITurnEndHandler, IInterruptTurnStartHandler, IRoundStartHandler, IRoundEndHandler, ITurnBasedModeHandler, IFactCollectionUpdatedHandler, IAreaHandler
{
	[CanBeNull]
	private EntityFactsManager m_CurrentManager;

	private bool m_FactRemovedWhileTick;

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void OnEnable()
	{
		RemoveInvalidBuffs();
	}

	public void Tick()
	{
		List<Buff> list = TempList.Get<Buff>();
		IReadOnlyList<Buff> readOnlyList = EntityFactService.Instance.Get<Buff>();
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			Buff buff = readOnlyList[i];
			PartLifeState lifeStateOptional = buff.Owner.GetLifeStateOptional();
			bool flag = lifeStateOptional != null && !lifeStateOptional.IsConscious;
			if (buff.IsExpired || (flag && !buff.Blueprint.StayOnDeath))
			{
				list.Add(buff);
			}
		}
		foreach (Buff item in list)
		{
			item.Remove();
		}
	}

	private void TickBuffs(bool isTurnBased, Initiative.Event @event, MechanicEntity eventSource = null)
	{
		if (!isTurnBased && @event != 0)
		{
			return;
		}
		foreach (MechanicEntity mechanicEntity in Game.Instance.EntityPools.MechanicEntities)
		{
			TickBuffsOnEntity(mechanicEntity, eventSource, isTurnBased, @event);
		}
	}

	private void TickBuffsOnEntity(MechanicEntity entity, [CanBeNull] MechanicEntity eventSource, bool isTurnBased, Initiative.Event @event, int depth = 0)
	{
		if (entity is BaseUnitEntity { IsExtra: not false } || !entity.IsInGame)
		{
			return;
		}
		List<Buff> list = TempList.Get<Buff>();
		try
		{
			m_CurrentManager = entity.Facts;
			List<EntityFact> list2 = entity.Facts.List;
			for (int j = 0; j < list2.Count; j++)
			{
				EntityFact entityFact = list2[j];
				if (!(entityFact is Buff buff))
				{
					if (entityFact.Active && @event == Initiative.Event.RoundEnd)
					{
						entityFact.CallComponents(delegate(ITickEachRound i)
						{
							i.OnNewRound();
						});
					}
					continue;
				}
				if (buff.Active && buff.Initiative.ShouldActNow(isTurnBased, @event, out var actRound))
				{
					buff.Initiative.LastTurn = actRound;
					buff.NextRound();
				}
				buff.UpdateIsExpired(@event, eventSource);
				if (buff.IsExpired)
				{
					list.Add(buff);
				}
			}
		}
		finally
		{
			m_CurrentManager = null;
			m_FactRemovedWhileTick = false;
		}
		foreach (Buff item in list)
		{
			item.Remove();
		}
		if (m_FactRemovedWhileTick)
		{
			if (depth > 100)
			{
				throw new Exception("TickBuffsOnEntity: too deep recursive call, something wrong");
			}
			TickBuffsOnEntity(entity, eventSource, isTurnBased, @event, depth + 1);
		}
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		HandleBuffsWhichShouldBeDisabledOutOfCasterTurn();
		TickBuffs(isTurnBased, Initiative.Event.TurnStart, EventInvokerExtensions.MechanicEntity);
	}

	void IInterruptTurnStartHandler.HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		HandleBuffsWhichShouldBeDisabledOutOfCasterTurn();
	}

	void ITurnEndHandler.HandleUnitEndTurn(bool isTurnBased)
	{
		TickBuffs(isTurnBased, Initiative.Event.TurnEnd, EventInvokerExtensions.MechanicEntity);
	}

	void IRoundStartHandler.HandleRoundStart(bool isTurnBased)
	{
		TickBuffs(isTurnBased, Initiative.Event.RoundStart);
	}

	void IRoundEndHandler.HandleRoundEnd(bool isTurnBased)
	{
		TickBuffs(isTurnBased, Initiative.Event.RoundEnd);
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			return;
		}
		List<Buff> list = TempList.Get<Buff>();
		foreach (Buff item in EntityFactService.Instance.Get<Buff>())
		{
			if (item.EndCondition == BuffEndCondition.CombatEnd)
			{
				list.Add(item);
			}
		}
		foreach (Buff item2 in list)
		{
			item2.Remove();
		}
	}

	void IFactCollectionUpdatedHandler.HandleFactCollectionUpdated(EntityFactsProcessor collection)
	{
		if (m_CurrentManager != null)
		{
			m_FactRemovedWhileTick |= collection.Manager == m_CurrentManager;
		}
	}

	void IAreaHandler.OnAreaBeginUnloading()
	{
	}

	void IAreaHandler.OnAreaDidLoad()
	{
		foreach (Buff item in EntityFactService.Instance.Get<Buff>())
		{
			if (item.IsEnabled && item.GetComponent<BuffSpawnFx>() != null)
			{
				item.SpawnFxFromBuffComponent();
			}
		}
	}

	private static void RemoveInvalidBuffs()
	{
		List<Buff> list = TempList.Get<Buff>();
		foreach (AbstractUnitEntity item in Game.Instance.EntityPools.AllUnits.All)
		{
			foreach (Buff rawFact in item.Buffs.RawFacts)
			{
				if (IsInvalid(rawFact))
				{
					list.Add(rawFact);
				}
			}
		}
		foreach (Buff item2 in list)
		{
			PFLog.Default.Error($"Remove buff with missing source: {item2} (owner: {item2.Owner})");
			item2.Remove();
		}
		static bool IsInvalid(Buff buff)
		{
			if (buff.Sources.Count > 0)
			{
				return buff.Sources.AllItems((EntityFactSource i) => i.IsMissing);
			}
			return false;
		}
	}

	private static void HandleBuffsWhichShouldBeDisabledOutOfCasterTurn()
	{
		IReadOnlyList<Buff> readOnlyList = EntityFactService.Instance.Get<Buff>();
		List<Buff> list = ListPool<Buff>.Claim(readOnlyList.Count);
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			Buff buff = readOnlyList[i];
			if (buff.ShouldBeDisabledOutOfCasterTurn)
			{
				list.Add(buff);
			}
		}
		foreach (Buff item in list)
		{
			MechanicEntity maybeCaster = item.Context.MaybeCaster;
			item.DisabledBecauseOfNotCasterTurn = maybeCaster != Game.Instance.Controllers.TurnController.CurrentUnit;
			item.UpdateIsActive();
		}
		ListPool<Buff>.Release(list);
	}
}
