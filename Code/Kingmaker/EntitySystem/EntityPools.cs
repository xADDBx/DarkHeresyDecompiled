using System.Collections.Generic;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Gameplay.Features.Elevator;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.EntitySystem;

public class EntityPools
{
	public readonly EntityPool<Entity> Entities = new EntityPool<Entity>();

	public readonly EntityPool<Entity> SuppressibleEntities = new EntityPool<Entity>(CanBeSuppressed);

	public readonly EntityPool<MechanicEntity> MechanicEntities = new EntityPool<MechanicEntity>();

	public readonly EntityPool<MechanicEntity> CombatParticipants = new EntityPool<MechanicEntity>(IsCombatParticipant);

	public readonly EntityPool<MechanicEntity> TargetableEntities = new EntityPool<MechanicEntity>(IsTargetable);

	public readonly EntityPool<AbstractUnitEntity> AllUnits = new EntityPool<AbstractUnitEntity>();

	public readonly EntityPool<BaseUnitEntity> AllBaseUnits = new EntityPool<BaseUnitEntity>();

	public readonly EntityPool<DestructibleEntity> DestructibleEntities = new EntityPool<DestructibleEntity>();

	public readonly EntityPool<CutscenePlayerData> Cutscenes = new EntityPool<CutscenePlayerData>();

	public readonly EntityPool<MapObjectEntity> MapObjects = new EntityPool<MapObjectEntity>();

	public readonly EntityPool<AreaEffectEntity> AreaEffects = new EntityPool<AreaEffectEntity>();

	public readonly EntityPool<ScriptZoneEntity> ScriptZones = new EntityPool<ScriptZoneEntity>();

	public readonly EntityPool<ElevatorPlatformEntity> ElevatorPlatforms = new EntityPool<ElevatorPlatformEntity>();

	public readonly List<AbstractUnitEntity> AllAwakeUnits = new List<AbstractUnitEntity>();

	public readonly List<BaseUnitEntity> AllBaseAwakeUnits = new List<BaseUnitEntity>();

	public void SetNewAwakeUnits(IEnumerable<AbstractUnitEntity> awakeUnits)
	{
		ClearAwakeUnits();
		foreach (AbstractUnitEntity awakeUnit in awakeUnits)
		{
			if (awakeUnit is BaseUnitEntity item)
			{
				AllBaseAwakeUnits.Add(item);
			}
			AllAwakeUnits.Add(awakeUnit);
		}
	}

	public void ClearAwakeUnits()
	{
		AllAwakeUnits.Clear();
		AllBaseAwakeUnits.Clear();
	}

	private static bool CanBeSuppressed(Entity entity)
	{
		return entity.IsSuppressible;
	}

	private static bool IsCombatParticipant(Entity entity)
	{
		return entity is ICombatParticipant;
	}

	private static bool IsTargetable(Entity entity)
	{
		if (!(entity is UnitEntity))
		{
			return entity is DestructibleEntity;
		}
		return true;
	}
}
