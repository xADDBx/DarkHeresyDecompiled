using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.Controllers.Optimization;

public class AreaEffectTrigger : MonoBehaviour
{
	public readonly HashSet<EntityRef<MechanicEntity>> Inside = new HashSet<EntityRef<MechanicEntity>>();

	public readonly HashSet<EntityRef<MechanicEntity>> Entered = new HashSet<EntityRef<MechanicEntity>>();

	public readonly HashSet<EntityRef<MechanicEntity>> Exited = new HashSet<EntityRef<MechanicEntity>>();

	public AreaEffectEntity Entity { get; set; }

	public void ClearDelta()
	{
		Entered.Clear();
		Exited.Clear();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (EntityDataLink.GetEntity(other) is MechanicEntity mechanicEntity && (mechanicEntity is BaseUnitEntity { IsExtra: false } || mechanicEntity is DestructibleEntity) && !WasAlreadyInAreaEffectCluster(mechanicEntity))
		{
			if (!Inside.Add(mechanicEntity))
			{
				EntityBoundsController.Logger.Error("Cant add {0} to vision of {1} - already added", mechanicEntity, Entity);
			}
			else
			{
				Entered.Add(mechanicEntity);
				Entity.ForceUpdate();
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (EntityDataLink.GetEntity(other) is MechanicEntity mechanicEntity && (mechanicEntity is BaseUnitEntity { IsExtra: false } || mechanicEntity is DestructibleEntity) && !WillRemainInAreaEffectCluster(mechanicEntity))
		{
			if (!Inside.Remove(mechanicEntity))
			{
				EntityBoundsController.Logger.Error("Cant remove {0} to vision of {1} - never added", mechanicEntity, Entity);
			}
			else
			{
				Exited.Add(mechanicEntity);
				Entity.ForceUpdate();
			}
		}
	}

	private bool WasAlreadyInAreaEffectCluster(MechanicEntity entity)
	{
		AreaEffectClusterComponent component = Entity.Blueprint.GetComponent<AreaEffectClusterComponent>();
		if (component == null)
		{
			return false;
		}
		PartUnitInAreaEffectCluster partUnitInAreaEffectClusterOptional = entity.GetPartUnitInAreaEffectClusterOptional();
		if (partUnitInAreaEffectClusterOptional == null)
		{
			partUnitInAreaEffectClusterOptional = entity.GetOrCreate<PartUnitInAreaEffectCluster>();
			partUnitInAreaEffectClusterOptional.AddClusterKey(component.ClusterLogicBlueprint);
			partUnitInAreaEffectClusterOptional.AddEnteringAreaEffectToList(component.ClusterLogicBlueprint, Entity);
			return false;
		}
		partUnitInAreaEffectClusterOptional.AddEnteringAreaEffectToList(component.ClusterLogicBlueprint, Entity);
		if (IsInAnotherAreaEffect(entity, component.ClusterLogicBlueprint, Entity))
		{
			return true;
		}
		partUnitInAreaEffectClusterOptional.AddClusterKey(component.ClusterLogicBlueprint);
		return false;
	}

	private bool WillRemainInAreaEffectCluster(MechanicEntity entity)
	{
		AreaEffectClusterComponent component = Entity.Blueprint.GetComponent<AreaEffectClusterComponent>();
		if (component == null)
		{
			return false;
		}
		if (IsInAnotherAreaEffect(entity, component.ClusterLogicBlueprint, Entity))
		{
			entity.GetPartUnitInAreaEffectClusterOptional()?.RemoveExitingAreaEffectFromList(component.ClusterLogicBlueprint, Entity);
			return true;
		}
		entity.GetPartUnitInAreaEffectClusterOptional()?.RemoveClusterKey(component.ClusterLogicBlueprint);
		return false;
	}

	private static bool IsInAnotherAreaEffect(MechanicEntity entity, BlueprintAreaEffectClusterLogic clusterKey, AreaEffectEntity areaEffectEntity)
	{
		if (!entity.HasCurrentClusterKey(clusterKey))
		{
			return false;
		}
		if (entity.IsCurrentlyInAnotherClusterArea(clusterKey, areaEffectEntity))
		{
			return true;
		}
		return false;
	}
}
