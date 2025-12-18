using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Traces.Actions;

[Serializable]
[TypeId("cc0635aeade642c4ba3c5b945d3becd7")]
public sealed class MarkDetectiveTraceFollowedToTrueEnd : GameAction
{
	[AllowedEntityType(typeof(DetectiveTraceRootView))]
	public EntityReference Root = new EntityReference();

	public override string GetCaption()
	{
		return $"Mark trace {Root} as followed to true end";
	}

	protected override void RunAction()
	{
		((DetectiveTraceRootEntity)Root.FindData()).MarkFollowedToTrueEnd();
	}
}
