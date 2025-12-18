using System.Collections.Generic;
using Code.Editor;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Localization;
using Kingmaker.View.MapObjects.InteractionComponentBase;

namespace Kingmaker.Code.Gameplay.Features.DetectiveClues.View;

[KnowledgeDatabaseID("b579d072a2874f7fac615b74ccf584bf")]
public class InteractionDetectiveClue : NewInteractionComponent<InteractionPartDetectiveClue, InteractionDetectiveClueSettings>, IBarkSource
{
	public override float ProximityRadius => Settings.DetectionRadius;

	public IEnumerable<LocalizedString> Barks => Settings.Barks;

	public bool IsVoIdForced => Settings.IsVoIdForced;

	public List<string> ForcedVoGuids => Settings.ForcedVoGuids;

	public bool Spammable => Settings.Spammable;

	public override DialogReferenceType GetUsagesFor(BlueprintDialog dialog)
	{
		if (Settings.Dialog.MaybeBlueprint != dialog)
		{
			return DialogReferenceType.None;
		}
		return DialogReferenceType.Start;
	}
}
