using System;
using Kingmaker.UnitLogic.ActivatableAbilities;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintActivatableAbilityReference : BlueprintReference<BlueprintActivatableAbility>
{
}
