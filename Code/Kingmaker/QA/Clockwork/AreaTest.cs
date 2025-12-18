using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.QA.Clockwork;

[Obsolete]
[ComponentName("Clockwork/Area Test")]
[AllowedOn(typeof(BlueprintClockworkScenario))]
[AllowMultipleComponents]
[TypeId("a8bf397f945191045b9c34b9d82fd0a4")]
public class AreaTest : BlueprintComponent
{
	[ValidateNotNull]
	public BlueprintAreaReference Area;

	public bool SortCommands;

	public ClockworkCommandList CommandList;
}
