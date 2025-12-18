using System.Collections.Generic;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Elevator;

public interface IElevatorPlatformConfig
{
	string EntityId { get; }

	bool IsInGame { get; }

	Vector2 Size { get; }

	BlueprintCutscene? Cutscene { get; }

	IEnumerable<EntityRef<LocatorEntity>> PartyPlaces { get; }

	IEnumerable<EntityRef<ElevatorPlatformStopEntity>> Stops { get; }

	IEnumerable<IElevatorPlatformRouteSettings> Routes { get; }
}
