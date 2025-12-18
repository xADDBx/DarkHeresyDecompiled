using System;
using Kingmaker.Blueprints;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
[TypeId("35056563cb9d4deaa985d2f7c6150a69")]
public abstract class BlueprintCaseItem : BlueprintScriptableObject
{
	[ValidateNotNull]
	[InspectorReadOnly]
	public BpRef<BlueprintCase> ParentCase;

	public LocalizedString Name;

	public LocalizedString Description;

	public Sprite Icon;

	[ValidateNoNullEntries]
	[InspectorReadOnly]
	public BpRef<BlueprintConclusion>[] PossibleConclusions = new BpRef<BlueprintConclusion>[0];
}
