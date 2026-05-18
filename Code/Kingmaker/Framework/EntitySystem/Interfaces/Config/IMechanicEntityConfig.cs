using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.UnitLogic.Mechanics.Blueprints;

namespace Kingmaker.Framework.EntitySystem.Interfaces.Config;

public interface IMechanicEntityConfig : IEntityConfig
{
	[NotNull]
	BlueprintMechanicEntityFact MechanicFactBlueprint { get; }
}
