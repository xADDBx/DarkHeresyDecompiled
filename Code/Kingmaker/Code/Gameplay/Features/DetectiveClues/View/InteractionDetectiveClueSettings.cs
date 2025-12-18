using System;
using System.Collections.Generic;
using System.Linq;
using Code.Editor;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.Code.Framework.VO;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;
using UnityEngine.Serialization;

namespace Kingmaker.Code.Gameplay.Features.DetectiveClues.View;

[Serializable]
public class InteractionDetectiveClueSettings : IBarkSource
{
	public bool CanBeShownByTab;

	public bool IsVariative;

	[FormerlySerializedAs("ProximityRadius")]
	public int DetectionRadius = 2;

	public SharedStringAsset Bark;

	public SharedStringAsset DisplayName;

	public bool ForceVoId;

	[ShowIf("ForceVoId")]
	public List<VoIdField> ForcedVoIds = new List<VoIdField>();

	public bool IsSpammable;

	public BpRef<BlueprintDialog> Dialog = new BpRef<BlueprintDialog>();

	public BpRef<BlueprintCutscene> Cutscene = new BpRef<BlueprintCutscene>();

	public ActionsReference Actions = new ActionsReference();

	public IEnumerable<LocalizedString> Barks => new LocalizedString[1] { Bark.String };

	public bool IsVoIdForced => ForceVoId;

	public List<string> ForcedVoGuids => ForcedVoIds.Select((VoIdField v) => v.Guid).ToList();

	public bool Spammable => IsSpammable;
}
