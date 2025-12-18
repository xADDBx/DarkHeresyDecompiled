using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using OwlPack.Runtime;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("da8ebf59d65042748529c3a0b6110230")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public sealed class BlueprintClueStudy : BlueprintScriptableObject
{
	[ValidateNotNull]
	[InspectorReadOnly]
	public BpRef<BlueprintCase> ParentCase;

	[ValidateNotNull]
	[InspectorReadOnly]
	public BpRef<BlueprintClue> ParentClue;

	public LocalizedString Name;

	public ConditionsChecker UnlockCondition;

	public BpRef<BlueprintUnit> StudyCompanion;

	public LocalizedString StudyBark;

	[InfoBox("BlueprintClueAddendum или BlueprintClue")]
	public BpRef<BlueprintCaseItem>[] GiveItems = new BpRef<BlueprintCaseItem>[0];
}
