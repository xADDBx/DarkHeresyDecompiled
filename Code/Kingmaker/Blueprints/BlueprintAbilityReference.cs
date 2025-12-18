using System;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintAbilityReference : BlueprintReference<BlueprintAbility>
{
}
