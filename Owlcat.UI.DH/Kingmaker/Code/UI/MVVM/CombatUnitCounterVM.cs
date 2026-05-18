using System.Collections.Generic;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CombatUnitCounterVM : ViewModel, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, IInGameHandler, ISubscriber<IEntity>, ITurnBasedModeHandler, IRoundStartHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler, IEntityDestructionHandler
{
	private readonly ReactiveProperty<int> m_EnemiesCount = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_AlliesCount = new ReactiveProperty<int>();

	private bool m_NeedUpdate;

	public ReadOnlyReactiveProperty<int> EnemiesCount => m_EnemiesCount;

	public ReadOnlyReactiveProperty<int> AlliesCount => m_AlliesCount;

	public CombatUnitCounterVM()
	{
		EventBus.Subscribe(this).AddTo(this);
		m_NeedUpdate = true;
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate), delegate
		{
			TryUpdate();
		}).AddTo(this);
	}

	private void TryUpdate()
	{
		if (m_NeedUpdate)
		{
			m_NeedUpdate = false;
			UpdateCounts();
		}
	}

	private void UpdateCounts()
	{
		if (!Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			return;
		}
		IEnumerable<MechanicEntity> entitiesInCombat = Game.Instance.Controllers.TurnController.EntitiesInCombat;
		int num = 0;
		int num2 = 0;
		foreach (MechanicEntity item in entitiesInCombat)
		{
			if (!item.IsDeadOrUnconscious)
			{
				if (item.IsPlayerEnemy)
				{
					num++;
				}
				else if (item.IsPlayerFaction && !item.IsInPlayerParty)
				{
					num2++;
				}
			}
		}
		m_EnemiesCount.Value = num;
		m_AlliesCount.Value = num2;
	}

	void IUnitSpawnHandler.HandleUnitSpawned()
	{
		m_NeedUpdate = true;
	}

	void IUnitHandler.HandleUnitDestroyed()
	{
		m_NeedUpdate = true;
	}

	void IUnitHandler.HandleUnitDeath()
	{
		m_NeedUpdate = true;
	}

	void IUnitCombatHandler.HandleUnitJoinCombat()
	{
		m_NeedUpdate = true;
	}

	void IUnitCombatHandler.HandleUnitLeaveCombat()
	{
		m_NeedUpdate = true;
	}

	void IInGameHandler.HandleObjectInGameChanged()
	{
		m_NeedUpdate |= EventInvokerExtensions.MechanicEntity is ICombatParticipant;
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		m_NeedUpdate = isTurnBased;
	}

	void IRoundStartHandler.HandleRoundStart(bool isTurnBased)
	{
		m_NeedUpdate = true;
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		m_NeedUpdate = true;
	}

	void IInterruptTurnStartHandler.HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		m_NeedUpdate = true;
	}

	void IEntityDestructionHandler.HandleEntityDestroyed()
	{
		m_NeedUpdate |= EventInvokerExtensions.MechanicEntity is ICombatParticipant;
	}
}
