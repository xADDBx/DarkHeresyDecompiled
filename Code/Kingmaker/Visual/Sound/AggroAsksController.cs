using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Visual.Sound;

public class AggroAsksController : BaseAsksController, ITickUnitAsksController, IUnitAsksController, IDisposable, IPartyCombatHandler, ISubscriber
{
	private bool m_CheckAggro;

	public override void Dispose()
	{
		base.Dispose();
		m_CheckAggro = false;
	}

	void IPartyCombatHandler.HandlePartyCombatStateChanged(bool inCombat)
	{
		m_CheckAggro = inCombat;
	}

	public void Tick()
	{
		if (!m_CheckAggro)
		{
			return;
		}
		BaseUnitEntity mainCharacterEntity = Game.Instance.Player.MainCharacterEntity;
		if (mainCharacterEntity.IsInCombat && mainCharacterEntity.LifeState.IsConscious && mainCharacterEntity.View != null && mainCharacterEntity.View.Asks != null)
		{
			foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.EntityPools.AllBaseAwakeUnits)
			{
				if (allBaseAwakeUnit.IsPlayerEnemy && allBaseAwakeUnit.IsInCombat && allBaseAwakeUnit.Blueprint.Army != null)
				{
					m_CheckAggro = false;
					return;
				}
			}
		}
		BaseUnitEntity randomPartyEntity = UnitAsksHelper.GetRandomPartyEntity((BaseUnitEntity x) => x.IsInCombat && x.LifeState.IsConscious && x.View != null && x.View.Asks != null && x.View.Asks.Aggro.HasBarks);
		if (randomPartyEntity != null)
		{
			using (EvalContext.PushAsksContext(randomPartyEntity, randomPartyEntity))
			{
				randomPartyEntity.View.Asks.Aggro.Schedule(is2D: false, delegate
				{
					AggroCallback(isRaceAnswer: false);
				});
			}
		}
		m_CheckAggro = false;
	}

	private static void AggroCallback(bool isRaceAnswer)
	{
		UnitAsksHelper.GetRandomEntity((BaseUnitEntity x) => x.Faction.IsPlayerEnemy && x.IsInCombat && x.LifeState.IsConscious && x.View != null && x.View.Asks != null && x.View.Asks.Aggro.HasBarks)?.View.Asks?.Aggro.Schedule();
	}
}
