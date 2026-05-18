using System.Collections.Generic;
using Code.Editor;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Localization;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

[DisallowMultipleComponent]
[KnowledgeDatabaseID("d7d73dc08a714880abefecc3491c6477")]
public class InteractionDetectiveTrace : NewInteractionComponent<InteractionPartDetectiveTrace, InteractionDetectiveTraceSettings>, IBarkSource
{
	public override float ProximityRadius => Settings.ProximityRadius;

	public IEnumerable<LocalizedString> Barks => Settings.Barks;

	public bool IsVoIdForced => Settings.IsVoIdForced;

	public List<string> ForcedVoGuids => Settings.ForcedVoGuids;

	public bool Spammable => Settings.Spammable;

	public bool IsVariative => Settings.IsVariative;

	public bool ShowNotFollowedOnMap => Settings.ShowNotFollowedOnMap;

	public override DialogReferenceType GetUsagesFor(BlueprintDialog dialog)
	{
		if (Settings.Dialog.MaybeBlueprint != dialog)
		{
			return DialogReferenceType.None;
		}
		return DialogReferenceType.Start;
	}
}
