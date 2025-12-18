using System.Collections.Generic;
using System.Linq;
using Code.Editor;
using Kingmaker.Code.Framework.VO;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("d8e2c5bb9ff388542b90552f59c8d14a")]
public class ContextActionShowBark : ContextAction, IBarkSource
{
	public LocalizedString WhatToBark;

	public bool ForceVoId;

	[ShowIf("ForceVoId")]
	public List<VoIdField> ForcedVoIds = new List<VoIdField>();

	public bool IsSpammable;

	public VoiceOverActAs ActAs;

	[HideInInspector]
	public SharedStringAsset WhatToBarkShared;

	public bool ShowWhileUnconscious;

	[Tooltip("Bark duration depends on text length")]
	public bool BarkDurationByText;

	public IEnumerable<LocalizedString> Barks => new LocalizedString[1] { WhatToBark };

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
			if (base.Context.MaybeCaster == null)
			{
				Element.LogError(this, "Caster is missing");
				return;
			}
			LocalizedString localizedString = (WhatToBarkShared ? WhatToBarkShared.String : WhatToBark);
			float duration = (BarkDurationByText ? UtilityBark.GetBarkDuration(localizedString) : UtilityBark.DefaultBarkTime);
			string voGuidBySourceAndTarget = VoiceOverController.GetVoGuidBySourceAndTarget(this, entity);
			BarkPlayer.Bark(entity, localizedString, (VoiceOverType)ActAs, voGuidBySourceAndTarget, duration, ContextData<InteractingUnitData>.Current?.Unit);
		}
	}
}
