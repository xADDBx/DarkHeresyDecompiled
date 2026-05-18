using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Framework.EntitySystem.Interfaces.Config;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IDetectiveTraceConfig : IMapObjectEntityConfig, IMechanicEntityConfig, IEntityConfig
{
	IReadOnlyList<DetectiveTracePoint> PointsData { get; }

	bool TrueEnd { get; }

	bool HideInteract { get; }

	bool HideInteractionIfFollowed { get; }

	bool HideInteractionIfFollowedToDeadEnd { get; }

	bool Found { get; }

	IEnumerable<EntityRef<DetectiveTraceEntity>> Continuations { get; }
}
