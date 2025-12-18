using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.UnitLogic.Progression.Paths;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
[TypeId("c26e1d660f8b4a66a433c35a0965d037")]
public class BlueprintOriginPath : BlueprintPath
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintOriginPath>
	{
	}
}
