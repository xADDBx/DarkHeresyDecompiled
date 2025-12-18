using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Levelup.Selections.Stats;

[Serializable]
[TypeId("47631632c8e04d6ca6b3017f9bbe73a6")]
public sealed class BlueprintSelectionAttributes : BlueprintSelectionStats
{
	[ValidateNotEmpty]
	[ValidateNoNullEntries]
	public BpRef<BlueprintAttributeAdvancement>[] AttributeAdvancements;

	public override IEnumerable<BlueprintStatAdvancement> Advancements => AttributeAdvancements.Dereference().NotNull();
}
