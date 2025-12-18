using System.Collections.Generic;
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
		UnitGroupView unitGroupView = m_Group.FindView() as UnitGroupView;
		if (unitGroupView == null)
		{
			LogChannel.Default.Warning(this, $"Unit group not found in {this} ({m_Group?.UniqueId})");
			QAModeExceptionReporter.MaybeShowError($"Unit group not found in {this} ({m_Group?.UniqueId})");
			return;
		}
		List<(AbstractUnitEntity, SceneEntitiesState)> value;
		using (CollectionPool<List<(AbstractUnitEntity, SceneEntitiesState)>, (AbstractUnitEntity, SceneEntitiesState)>.Get(out value))
		{
			UnitSpawner[] componentsInChildren = unitGroupView.GetComponentsInChildren<UnitSpawner>();
			foreach (UnitSpawner unitSpawner in componentsInChildren)
			{
				UnitSpawnerBase.MyData data = unitSpawner.Data;
				if (data != null && data.IsInGame && !unitSpawner.HasSpawned)
				{
					AbstractUnitEntity abstractUnitEntity = unitSpawner.Spawn();
					if (abstractUnitEntity != null)
					{
						value.Add((abstractUnitEntity, unitSpawner.Data.HoldingState));
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
