using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.QA;
using Kingmaker.View.Spawners;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("2a580ffc7fd649d7a9cdad06b33c8ef9")]
public class SpawnByUnitGroup : GameAction
{
	[SerializeField]
	[AllowedEntityType(typeof(UnitGroupView))]
	public EntityReference m_Group;

	public ActionList ActionsOnSpawn;

	protected override void RunAction()
	{
		if (!(m_Group.FindData() is UnitGroupEntity unitGroupEntity))
		{
			LogChannel.Default.Warning(this, $"Unit group not found in {this} ({m_Group?.UniqueId})");
			QAModeExceptionReporter.MaybeShowError($"Unit group not found in {this} ({m_Group?.UniqueId})");
			return;
		}
		List<(AbstractUnitEntity, SceneEntitiesState)> value;
		using (CollectionPool<List<(AbstractUnitEntity, SceneEntitiesState)>, (AbstractUnitEntity, SceneEntitiesState)>.Get(out value))
		{
			foreach (AbstractUnitSpawnerEntity item in unitGroupEntity.Members.OfType<AbstractUnitSpawnerEntity>())
			{
				if (item != null && item.IsInGame && !item.HasSpawned)
				{
					AbstractUnitEntity abstractUnitEntity = item.Spawn();
					if (abstractUnitEntity != null)
					{
						value.Add((abstractUnitEntity, item.HoldingState));
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
				using (ContextData<SpawnedUnitData>.Request().Setup(unit, state))
				{
					ActionsOnSpawn.Run();
				}
			}
		}
	}

	public override string GetCaption()
	{
		return "Spawn all in " + m_Group?.EntityNameInEditor;
	}
}
