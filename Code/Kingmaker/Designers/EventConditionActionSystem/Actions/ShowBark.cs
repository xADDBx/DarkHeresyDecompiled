using System.Collections.Generic;
using System.Linq;
using Code.Editor;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Framework.VO;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/ShowBark")]
[AllowMultipleComponents]
[TypeId("e164ef6758f918a4abcc3889472a2a3c")]
public class ShowBark : GameAction, IBarkSource
{
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	public LocalizedString WhatToBark;

	public bool ForceVoId;

	[ShowIf("ForceVoId")]
	public List<VoIdField> ForcedVoIds = new List<VoIdField>();

	public bool IsSpammable;

	public VoiceOverActAs ActAs;

	[HideInInspector]
	public SharedStringAsset WhatToBarkShared;

	[Tooltip("Bark duration depends on text length")]
	public bool BarkDurationByText;

	[FormerlySerializedAs("Target")]
	[SerializeReference]
	public AbstractUnitEvaluator TargetUnit;

	[SerializeReference]
	public MapObjectEvaluator TargetMapObject;

	[Tooltip("Allow set exact playback time")]
	public bool OverrideBarkDuration;

	[Tooltip("Exact playback time")]
	[ShowIf("OverrideBarkDuration")]
	public float BarkDuration;

	private Entity Target
	{
		get
		{
			if (TargetUnit == null)
			{
				if (TargetMapObject == null)
				{
					return null;
				}
				return TargetMapObject.GetValue();
			}
			return TargetUnit.GetValue();
		}
	}

	public IEnumerable<LocalizedString> Barks
	{
		get
		{
			if ((bool)WhatToBarkShared)
			{
				return new LocalizedString[1] { WhatToBarkShared.String };
			}
			return new LocalizedString[1] { WhatToBark };
		}
	}

	public bool IsVoIdForced => ForceVoId;

	public List<string> ForcedVoGuids => ForcedVoIds.Select((VoIdField v) => v.Guid).ToList();

	public bool Spammable => IsSpammable;

	protected override void RunAction()
	{
		if (TargetUnit == null || TargetUnit.GetValue().LifeState.IsConscious)
		{
			Entity target = Target;
			LocalizedString localizedString = (WhatToBarkShared ? WhatToBarkShared.String : WhatToBark);
			float duration = UtilityBark.DefaultBarkTime;
			if (BarkDurationByText)
			{
				duration = UtilityBark.GetBarkDuration(localizedString);
			}
			if (OverrideBarkDuration)
			{
				duration = BarkDuration;
			}
			BarkPlayer.Bark(voGuid: VoiceOverController.GetVoGuidBySourceAndTarget(this, Target), entity: target, text: localizedString, voiceOverType: (VoiceOverType)ActAs, duration: duration, interactUser: ContextData<InteractingUnitData>.Current?.Unit);
		}
	}

	public override string GetCaption()
	{
		Element arg = ((TargetUnit != null) ? ((MechanicEntityEvaluator)TargetUnit) : ((MechanicEntityEvaluator)TargetMapObject));
		return $"Show Bark (on {arg})";
	}
}
