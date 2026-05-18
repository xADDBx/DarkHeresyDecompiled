using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.View.Spawners;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
[TypeId("bd9184405dadcd540920f42c11a3c737")]
public class EtudeBracketShowObjects : EtudeBracketTrigger
{
	public EntityReference[] Objects;

	protected override void OnEnter()
	{
		ShowObjects();
	}

	protected override void OnResume()
	{
		ShowObjects();
	}

	protected override void OnExit()
	{
		HideObjects();
	}

	private void ShowObjects()
	{
		EntityReference[] objects = Objects;
		for (int i = 0; i < objects.Length; i++)
		{
			IEntity entity = objects[i].FindData();
			if (entity is AbstractUnitSpawnerEntity abstractUnitSpawnerEntity)
			{
				entity = abstractUnitSpawnerEntity.SpawnedUnit;
			}
			if (entity != null)
			{
				entity.IsInGame = true;
			}
		}
	}

	private void HideObjects()
	{
		EntityReference[] objects = Objects;
		for (int i = 0; i < objects.Length; i++)
		{
			IEntity entity = objects[i].FindData();
			if (entity is AbstractUnitSpawnerEntity abstractUnitSpawnerEntity)
			{
				entity = abstractUnitSpawnerEntity.SpawnedUnit;
			}
			if (entity != null)
			{
				entity.IsInGame = false;
			}
		}
	}
}
