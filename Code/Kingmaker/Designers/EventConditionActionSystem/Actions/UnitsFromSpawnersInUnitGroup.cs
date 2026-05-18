using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.Spawners;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("d6438d31eff349d4bf1ec872cfc0a001")]
public class UnitsFromSpawnersInUnitGroup : GameAction
{
	public class UnitData : AbstractUnitData<UnitData>
	{
	}

	[SerializeField]
	[AllowedEntityType(typeof(UnitGroupView))]
	private EntityReference m_Group;

	public ActionList Actions;

	protected override void RunAction()
	{
		if (!(m_Group.FindData() is UnitGroupEntity unitGroupEntity))
		{
			return;
		}
		foreach (Entity member in unitGroupEntity.Members)
		{
			AbstractUnitEntity abstractUnitEntity = null;
			if (!(member is UnitSpawnerEntity unitSpawnerEntity))
			{
				if (member is CompanionSpawnerEntity { IsControllingCompanion: not false } companionSpawnerEntity)
				{
					abstractUnitEntity = companionSpawnerEntity.SpawnedUnit;
				}
			}
			else
			{
				abstractUnitEntity = unitSpawnerEntity.SpawnedUnit;
			}
			if (abstractUnitEntity != null)
			{
				using (ContextData<UnitData>.Request().Setup(abstractUnitEntity))
				{
					Actions.Run();
				}
			}
		}
	}

	public override string GetCaption()
	{
		return "For all spawners grouped under one object in " + m_Group?.EntityNameInEditor;
	}
}
