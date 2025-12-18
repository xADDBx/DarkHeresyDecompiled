using System;
using Kingmaker.Blueprints.Items;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintItemReference : BlueprintReference<BlueprintItem>
{
}
