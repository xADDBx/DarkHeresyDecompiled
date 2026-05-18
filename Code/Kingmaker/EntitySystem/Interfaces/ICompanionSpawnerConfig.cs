using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;

namespace Kingmaker.EntitySystem.Interfaces;

public interface ICompanionSpawnerConfig : IAbstractUnitSpawnerConfig, IEntityConfig
{
	[CanBeNull]
	BlueprintFaction OverrideFaction { get; }

	bool SpawnNpcCopy { get; }

	bool SpawnWhenRemote { get; }

	bool SpawnWhenEx { get; }

	bool SpawnWhenInCapital { get; }

	bool SpawnWhenDetached { get; }

	[CanBeNull]
	ConditionsHolder ControlCondition { get; }

	[CanBeNull]
	ConditionsHolder ShowCondition { get; }
}
