using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Pathfinding;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Elevator;

[RequireComponent(typeof(ValidNavmeshArea))]
[KnowledgeDatabaseID("3926c21051eb4886b596e9c1560dbb42")]
public sealed class ElevatorPlatformStopView : EntityViewBase, IElevatorPlatformStopConfig
{
	string IElevatorPlatformStopConfig.EntityId => UniqueId;

	bool IElevatorPlatformStopConfig.IsInGame => base.IsInGameBySettings;

	Vector3 IElevatorPlatformStopConfig.Position => base.transform.position;

	float IElevatorPlatformStopConfig.Orientation => base.transform.rotation.eulerAngles.y;

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new ElevatorPlatformStopEntity(this));
	}
}
