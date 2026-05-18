using System;
using System.Collections.Generic;
using System.Linq;
using Code.Editor;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.Code.Framework.VO;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Sound;
using Owlcat.Fmw.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

[Serializable]
public class InteractionDetectiveTraceSettings : IBarkSource
{
	public bool CanBeShownByTab;

	public bool IsVariative;

	public bool ShowNotFollowedOnMap;

	public int ProximityRadius = 5;

	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	public LocalizedString Bark;

	public LocalizedString DisplayName;

	public bool ForceVoId;

	[ShowIf("ForceVoId")]
	public List<VoIdField> ForcedVoIds = new List<VoIdField>();

	public bool IsSpammable;

	public BpRef<BlueprintDialog> Dialog = new BpRef<BlueprintDialog>();

	public BpRef<BlueprintCutscene> Cutscene = new BpRef<BlueprintCutscene>();

	public ActionsReference Actions = new ActionsReference();

	[FormerlySerializedAs("FxOnTracesAppear")]
	[Tooltip("Звук при появлении следов")]
	[AkEventReference]
	public string SoundFX = "FX_DetectiveTrace_Activate";

	public IEnumerable<LocalizedString> Barks => new LocalizedString[1] { Bark };

	public bool IsVoIdForced => ForceVoId;

	public List<string> ForcedVoGuids => ForcedVoIds.Select((VoIdField v) => v.Guid).ToList();

	public bool Spammable => IsSpammable;
}
