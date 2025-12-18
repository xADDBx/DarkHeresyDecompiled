using System;
using Kingmaker.UnitLogic.Progression.Features;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintFeatureReference : BlueprintReference<BlueprintFeature>
{
}
