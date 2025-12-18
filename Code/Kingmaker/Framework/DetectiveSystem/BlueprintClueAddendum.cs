using System;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("b3dab521b26745388fb9547f755ef48f")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public sealed class BlueprintClueAddendum : BlueprintCaseItem
{
	[Serializable]
	public sealed class Override
	{
		public OverrideType Type;

		[ValidateNotNull]
		public BpRef<BlueprintClueAddendum> Addendum;
	}

	public enum OverrideType
	{
		Addition
	}

	[ShowIf("False")]
	[Obsolete("WH2-7451")]
	public LocalizedString FalseDescription;

	[ValidateNotNull]
	[InspectorReadOnly]
	public BpRef<BlueprintClue> ParentClue;

	public LinkedClue[] LinkedClues = new LinkedClue[0];

	public Override[] Overrides = new Override[0];

	[Obsolete("WH2-7451")]
	public bool False => false;

	public bool HasOverride(BlueprintClueAddendum addendum)
	{
		return Overrides.HasItem((Override i) => i.Addendum == addendum);
	}
}
