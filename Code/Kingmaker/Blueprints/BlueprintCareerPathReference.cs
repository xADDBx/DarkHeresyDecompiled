using System;
using Kingmaker.UnitLogic.Progression.Paths;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintCareerPathReference : BlueprintReference<BlueprintCareerPath>
{
}
