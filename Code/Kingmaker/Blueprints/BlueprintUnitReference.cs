using System;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintUnitReference : BlueprintReference<BlueprintUnit>
{
}
