using System;
using Kingmaker.Blueprints;
using Kingmaker.Localization;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[Obsolete("New Question/Answer approach, WIP")]
[TypeId("d2eaba6d88cd41dd942cbbb41771f64b")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public sealed class BlueprintConclusionType : BlueprintScriptableObject
{
	public LocalizedString Name;

	public LocalizedString Description;

	public LocalizedString QuestionMarker;

	public ConclusionCategory Category;
}
