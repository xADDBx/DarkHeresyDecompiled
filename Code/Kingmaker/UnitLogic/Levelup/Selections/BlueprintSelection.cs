using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.UnitLogic.Levelup.Selections;

[TypeId("60eeb523421a43e3a7c232ee79c0a0ae")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public abstract class BlueprintSelection : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintSelection>
	{
	}
}
