using Kingmaker.Blueprints;
using Kingmaker.Framework.EntitySystem.Interfaces.Config;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IUnitEntityConfig : IMechanicEntityConfig, IEntityConfig
{
	float Corpulence { get; }

	BlueprintUnit Blueprint { get; }
}
