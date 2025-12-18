using System;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintScriptableObjectReference : BlueprintReference<BlueprintScriptableObject>
{
}
