using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Customization;
using Kingmaker.View.Spawners;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IAbstractUnitSpawnerConfig : IEntityConfig
{
	string name { get; }

	BlueprintUnit Blueprint { get; }

	bool SpawnOnSceneInit { get; }

	bool RespawnIfDead { get; }

	[CanBeNull]
	ConditionsHolder SpawnConditions { get; }

	UnitCustomizationVariation SelectedCustomizationVariation { get; }

	IUnitSpawnRestriction[] Restrictions { get; }

	string sceneName { get; }

	string GroupId { get; }
}
