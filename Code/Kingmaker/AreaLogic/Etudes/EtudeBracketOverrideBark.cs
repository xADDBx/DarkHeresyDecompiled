using System;
using System.Collections.Generic;
using System.Linq;
using Code.Editor;
using Kingmaker.Code.Framework.VO;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
[TypeId("1db2a7f1e66ae3d47b9e16af07f04a25")]
public class EtudeBracketOverrideBark : EtudeBracketOverrideInteraction, IBarkSource
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public EtudeBracketOverrideUnitInteraction Interaction;
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	public LocalizedString WhatToBarkShared;

	public bool ForceVoId;

	[ShowIf("ForceVoId")]
	public List<VoIdField> ForcedVoIds = new List<VoIdField>();

	public bool IsSpammable;

	public VoiceOverActAs ActAs;

	[Tooltip("Bark duration depends on text length")]
	public bool BarkDurationByText = true;

	public IEnumerable<LocalizedString> Barks => new LocalizedString[1] { WhatToBarkShared };

	public bool IsVoIdForced => ForceVoId;

	public List<string> ForcedVoGuids => ForcedVoIds.Select((VoIdField v) => v.Guid).ToList();

	public bool Spammable => IsSpammable;

	public override bool IsDialog => false;

	protected override void OnEnter()
	{
		PartUnitInteractions orCreate = Unit.GetValue().GetOrCreate<PartUnitInteractions>();
		ComponentData componentData = RequestTransientData<ComponentData>();
		componentData.Interaction = new EtudeBracketOverrideUnitInteraction(this);
		orCreate.AddInteraction(componentData.Interaction);
	}

	protected override void OnExit()
	{
		PartUnitInteractions orCreate = Unit.GetValue().GetOrCreate<PartUnitInteractions>();
		ComponentData componentData = RequestTransientData<ComponentData>();
		orCreate.RemoveInteraction(componentData.Interaction);
	}

	protected override void OnResume()
	{
		OnEnter();
	}

	public override AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target)
	{
		float duration = (BarkDurationByText ? UtilityBark.GetBarkDuration(WhatToBarkShared) : UtilityBark.DefaultBarkTime);
		string voGuidBySourceAndTarget = VoiceOverController.GetVoGuidBySourceAndTarget(this, target);
		BarkPlayer.Bark(target, WhatToBarkShared, (VoiceOverType)ActAs, voGuidBySourceAndTarget, duration, user);
		return AbstractUnitCommand.ResultType.Success;
	}
}
