using System.Collections.Generic;
using Kingmaker.Code.Gameplay.Features.DetectiveClues.View;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Framework.EntitySystem.Interfaces.Config;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IDetectiveClueConfig : IMapObjectEntityConfig, IMechanicEntityConfig, IEntityConfig
{
	bool IsJammer { get; }

	bool TurnOffAllCluesInGroupAfterInteraction { get; }

	IEnumerable<EntityRef<DetectiveClueEntity>> PreviousClues { get; }

	IEnumerable<EntityRef<DetectiveClueEntity>> NextClues { get; }

	IEnumerable<EntityRef<DetectiveClueEntity>> CluesToTurnOffAfterInteraction { get; }
}
