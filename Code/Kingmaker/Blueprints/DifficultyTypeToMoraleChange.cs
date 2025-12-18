using System;
using Kingmaker.Gameplay.Features.Experience;

namespace Kingmaker.Blueprints;

[Serializable]
public sealed class DifficultyTypeToMoraleChange
{
	public UnitDifficultyType DifficultyType;

	public ModifiableByDifficultyParameter AllyDeath;

	public ModifiableByDifficultyParameter EnemyDeath;
}
