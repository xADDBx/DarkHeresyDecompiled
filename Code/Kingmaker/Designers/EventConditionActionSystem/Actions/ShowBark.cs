using System.Collections.Generic;
using System.Linq;
using Code.Editor;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Framework.VO;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.ElementsSystem.Interfaces;
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
public class ShowBark : GameAction, IBarkSource, IInitializableElement
{
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	public LocalizedString WhatToBark;

	public bool ForceVoId;

	[ShowIf("ForceVoId")]
	public List<VoIdField> ForcedVoIds = new List<VoIdField>();

	public bool IsSpammable;

	[Tooltip("Play bark for hidden units")]
	public bool ForceShow;

	public VoiceOverActAs ActAs;

	[HideInInspector]
	public LocalizedString WhatToBarkShared;

	[Tooltip("Bark duration depends on text length")]
	public bool BarkDurationByText = true;

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
			if (!WhatToBarkShared.Empty)
			{
				return new LocalizedString[1] { WhatToBarkShared };
			}
			return new LocalizedString[1] { WhatToBark };
		}
	}

	public bool IsVoIdForced => ForceVoId;

	public List<string> ForcedVoGuids => ForcedVoIds.Select((VoIdField v) => v.Guid).ToList();

	public bool Spammable => IsSpammable;

	protected override void RunAction()
	{
		Entity target;
		LocalizedString str;
		float barkDuration;
		if (TargetUnit == null || TargetUnit.GetValue().LifeState.IsConscious)
		{
			target = Target;
			str = ((!WhatToBarkShared.Empty) ? WhatToBarkShared : WhatToBark);
			barkDuration = UtilityBark.DefaultBarkTime;
			if (BarkDurationByText)
			{
				barkDuration = UtilityBark.GetBarkDuration(str);
			}
			if (OverrideBarkDuration)
			{
				barkDuration = BarkDuration;
			}
			DoBark();
		}
		void DoBark()
		{
			if (target != null && Game.Instance.Controllers.EntitySpawner.IsEntityInCreationQueue(target))
			{
				Game.Instance.Controllers.CoroutinesController.InvokeInTicks(DoBark, 2);
			}
			else
			{
				string voGuidBySourceAndTarget = VoiceOverController.GetVoGuidBySourceAndTarget(this, target);
				BarkPlayer.Bark(target, str, (VoiceOverType)ActAs, voGuidBySourceAndTarget, barkDuration, ContextData<InteractingUnitData>.Current?.Unit, synced: true, ForceShow);
			}
		}
	}

	public override string GetCaption()
	{
		Element arg = ((TargetUnit != null) ? ((MechanicEntityEvaluator)TargetUnit) : ((MechanicEntityEvaluator)TargetMapObject));
		return $"Show Bark (on {arg})";
	}

	public void InitInEditor(string propertyPath, SimpleBlueprint ownerBlueprint)
	{
	}
}
