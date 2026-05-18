using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.Spawners;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("5a0f8a1614a840f8b0629a71b6de51f7")]
public class SpawnBySummonPool : GameAction
{
	[SerializeField]
	private BlueprintSummonPoolReference m_Pool;

	public ActionList ActionsOnSpawn;

	public BlueprintSummonPool Pool
	{
		get
		{
			return m_Pool?.Get();
		}
		set
		{
			m_Pool = SimpleBlueprintExtendAsObject.Or(value, null)?.ToReference<BlueprintSummonPoolReference>();
		}
	}

	protected override void RunAction()
	{
		List<(AbstractUnitEntity, SceneEntitiesState)> value;
		using (CollectionPool<List<(AbstractUnitEntity, SceneEntitiesState)>, (AbstractUnitEntity, SceneEntitiesState)>.Get(out value))
		{
			foreach (AbstractUnitSpawnerEntity temp in EntityService.Instance.GetTempList<AbstractUnitSpawnerEntity>())
			{
				if (temp == null)
				{
					continue;
				}
				SpawnerSummonPoolSettings.Part optional = temp.GetOptional<SpawnerSummonPoolSettings.Part>();
				if (optional != null && optional.Source.Pools.HasReference(m_Pool.Get()))
				{
					AbstractUnitEntity abstractUnitEntity = temp.Spawn();
					if (abstractUnitEntity != null)
					{
						value.Add((abstractUnitEntity, temp.HoldingState));
					}
				}
			}
			if (!ActionsOnSpawn.HasActions)
			{
				return;
			}
			Game.Instance.Controllers.EntitySpawner.Tick();
			foreach (var (unit, state) in value)
			{
				using (ContextData<SummonPoolUnitData>.Request().Setup(unit))
				{
					using (ContextData<SpawnedUnitData>.Request().Setup(unit, state))
					{
						ActionsOnSpawn.Run();
					}
				}
			}
		}
	}

	public override string GetCaption()
	{
		return "Spawn all in " + m_Pool?.Get().NameSafe();
	}
}
