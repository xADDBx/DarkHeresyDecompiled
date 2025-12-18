using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.View.MapObjects.InteractionComponentBase;

namespace Kingmaker.Code.Gameplay.Features.VariableInteractions;

[KnowledgeDatabaseID("cf2390e88089478486a7c28baa681b3b")]
public class InteractionVariative : NewInteractionComponent<InteractionVariativePart, InteractionVariativeSettings>
{
	public override float ProximityRadius => Settings.ProximityRadius;

	public override DialogReferenceType GetUsagesFor(BlueprintDialog dialog)
	{
		return DialogReferenceType.None;
	}
}
