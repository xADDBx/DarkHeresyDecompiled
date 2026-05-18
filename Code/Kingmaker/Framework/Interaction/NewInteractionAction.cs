using System.Collections.Generic;
using System.Linq;
using Code.Editor;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Framework.VO;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Localization;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using UnityEngine;

namespace Kingmaker.Framework.Interaction;

[DisallowMultipleComponent]
[KnowledgeDatabaseID("d964b8dd4210a014cb26ad42e267f20e")]
public sealed class NewInteractionAction : NewInteractionComponent<NewInteractionActionPart, NewInteractionActionSettings>, IBarkSource
{
	public override float ProximityRadius => Settings.ProximityRadius;

	public IEnumerable<LocalizedString> Barks
	{
		get
		{
			foreach (InteractionModule module in Settings.Modules)
			{
				if (module is BarkInteractionModule barkInteractionModule)
				{
					LocalizedString bark = barkInteractionModule.Bark;
					if (bark != null && !bark.Empty)
					{
						yield return bark;
					}
				}
			}
		}
	}

	public bool IsVoIdForced => Settings.Modules.OfType<BarkInteractionModule>().Any((BarkInteractionModule m) => m.ForceVoId);

	public List<string> ForcedVoGuids => (from v in (from m in Settings.Modules.OfType<BarkInteractionModule>()
			where m.ForceVoId
			select m).SelectMany((BarkInteractionModule m) => m.ForcedVoIds)
		select v.Guid).ToList();

	public bool Spammable => Settings.Modules.OfType<BarkInteractionModule>().Any((BarkInteractionModule m) => m.IsSpammable);

	public override DialogReferenceType GetUsagesFor(BlueprintDialog dialog)
	{
		foreach (InteractionModule module in Settings.Modules)
		{
			if (module is StartDialogInteractionModule startDialogInteractionModule && startDialogInteractionModule.Dialog.MaybeBlueprint == dialog)
			{
				return DialogReferenceType.Start;
			}
		}
		return DialogReferenceType.None;
	}
}
