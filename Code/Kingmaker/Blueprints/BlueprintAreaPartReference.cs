using System;
using Kingmaker.Blueprints.Area;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintAreaPartReference : BlueprintReference<BlueprintAreaPart>
{
}
