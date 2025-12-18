using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Levelup.Selections.Stats;

[Serializable]
[TypeId("ce6897d1f25a4dd2a46ddb5dfbe70835")]
public sealed class BlueprintSelectionSkills : BlueprintSelectionStats
{
	[ValidateNotEmpty]
	[ValidateNoNullEntries]
	public BpRef<BlueprintSkillAdvancement>[] SkillAdvancements;

	public override IEnumerable<BlueprintStatAdvancement> Advancements => SkillAdvancements.Dereference().NotNull();
}
