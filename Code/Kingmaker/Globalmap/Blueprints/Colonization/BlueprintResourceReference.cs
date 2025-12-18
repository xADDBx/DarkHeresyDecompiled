using System;
using Kingmaker.Blueprints;
using OwlPack.Runtime;

namespace Kingmaker.Globalmap.Blueprints.Colonization;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintResourceReference : BlueprintReference<BlueprintResource>
{
}
