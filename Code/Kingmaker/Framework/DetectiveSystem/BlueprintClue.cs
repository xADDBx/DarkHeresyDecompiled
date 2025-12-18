using System;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("84259b9690ad4b2a897ef8b3f8b8fb3c")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public sealed class BlueprintClue : BlueprintCaseItem
{
	public enum UIType
	{
		Default,
		Person,
		Location,
		Weapon,
		Event
	}

	[Serializable]
	public sealed class Override
	{
		public OverrideType Type;

		[ValidateNotNull]
		public BpRef<BlueprintClue> Clue;
	}

	public enum OverrideType
	{
		Addition
	}

	public UIType UIClueType;

	[ValidateNoNullEntries]
	public BpRef<BlueprintClueAddendum>[] Addendums = new BpRef<BlueprintClueAddendum>[0];

	[ValidateNoNullEntries]
	public BpRef<BlueprintClueNote>[] Notes = new BpRef<BlueprintClueNote>[0];

	[ValidateNoNullEntries]
	public BpRef<BlueprintClueStudy>[] Studies = new BpRef<BlueprintClueStudy>[0];

	public bool IsDirectlyConnectedToCase;

	public LinkedClue[] LinkedClues = new LinkedClue[0];

	public Override[] Overrides = new Override[0];

	public bool HasOverride(BlueprintClue clue)
	{
		return Overrides.HasItem((Override i) => i.Clue == clue);
	}
}
