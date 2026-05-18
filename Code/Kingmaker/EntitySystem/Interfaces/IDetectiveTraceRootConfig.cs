using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Framework.EntitySystem.Interfaces.Config;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IDetectiveTraceRootConfig : IMapObjectEntityConfig, IMechanicEntityConfig, IEntityConfig
{
	IEnumerable<EntityRef<DetectiveTraceEntity>> Traces { get; }
}
