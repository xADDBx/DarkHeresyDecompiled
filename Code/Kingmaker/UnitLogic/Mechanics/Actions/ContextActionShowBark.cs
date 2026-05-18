using System.Collections.Generic;
using System.Linq;
using Code.Editor;
using Kingmaker.Blueprints;
using Kingmaker.Code.Framework.VO;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("d8e2c5bb9ff388542b90552f59c8d14a")]
public class ContextActionShowBark : ContextAction, IBarkSource, IInitializableElement
{
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	public LocalizedString WhatToBark;

	public bool ForceVoId;

	[ShowIf("ForceVoId")]
	public List<VoIdField> ForcedVoIds = new List<VoIdField>();

	public bool IsSpammable;

	public VoiceOverActAs ActAs;

	[HideInInspector]
	public LocalizedString WhatToBarkShared;

	public bool ShowWhileUnconscious;

	[Tooltip("Play bark for hidden units")]
	public bool ForceShow;

	[Tooltip("Bark duration depends on text length")]
	public bool BarkDurationByText = true;

	public IEnumerable<LocalizedString> Barks
	{
		get
		{
			if (WhatToBarkShared != null)
			{
				yield return WhatToBarkShared;
			}
			else if (WhatToBark != null)
			{
				yield return WhatToBark;
			}
		}
	}

	public bool IsVoIdForced => ForceVoId;

	public List<string> ForcedVoGuids => ForcedVoIds.Select((VoIdField x) => x.Guid).ToList();

	public bool Spammable => IsSpammable;

	public override string GetCaption()
	{
		return "Show bark on target unit";
	}

	protected override void RunAction()
	{
		MechanicEntity entity = base.Target.Entity;
		if (entity == null)
		{
			return;
		}
		PartLifeState lifeStateOptional = entity.GetLifeStateOptional();
		if (lifeStateOptional == null || lifeStateOptional.IsConscious || ShowWhileUnconscious)
		{
			if (base.Context.Caster == null)
			{
				Element.LogError(this, "Caster is missing");
				return;
			}
			LocalizedString localizedString = ((!WhatToBarkShared.IsEmpty()) ? WhatToBarkShared : WhatToBark);
			float duration = (BarkDurationByText ? UtilityBark.GetBarkDuration(localizedString) : UtilityBark.DefaultBarkTime);
			string voGuidBySourceAndTarget = VoiceOverController.GetVoGuidBySourceAndTarget(this, entity);
			BarkPlayer.Bark(entity, localizedString, (VoiceOverType)ActAs, voGuidBySourceAndTarget, duration, ContextData<InteractingUnitData>.Current?.Unit, synced: true, ForceShow);
		}
	}

	public void InitInEditor(string propertyPath, SimpleBlueprint ownerBlueprint)
	{
	}
}
