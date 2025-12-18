using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Interfaces;
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
		IEntityViewBase entityViewBase = m_Group.FindView();
		if (entityViewBase == null)
		{
			return;
		}
		foreach (Transform item in entityViewBase.ViewTransform)
		{
			AbstractUnitEntity abstractUnitEntity = null;
			UnitSpawnerBase component = item.GetComponent<UnitSpawnerBase>();
			if (!(component is UnitSpawner unitSpawner))
			{
				if (component is CompanionSpawner { IsControllingCompanion: not false } companionSpawner)
				{
					abstractUnitEntity = companionSpawner.SpawnedUnit;
				}
			}
			else
			{
				abstractUnitEntity = unitSpawner.SpawnedUnit;
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
