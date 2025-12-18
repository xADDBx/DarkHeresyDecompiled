using System;
using OwlPack.Runtime;
using StateHasher.Core;

namespace Kingmaker.Blueprints;

[Serializable]
[HashRoot]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintItemsStashReference : BlueprintReference<BlueprintItemsStash>
{
}
