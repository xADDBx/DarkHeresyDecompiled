using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[PlayerUpgraderAllowed(false)]
[TypeId("4d2b3bd16c3f2bf4ebb8a19907e2be7e")]
public class CueSeen : Condition
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Cue")]
	private BlueprintCueBaseReference m_Cue;

	[Tooltip("Only check shown cues in current dialog instance. By default whole game history matters.")]
	public bool CurrentDialog;

	public BlueprintCueBase Cue => m_Cue?.Get();

	protected override string GetConditionCaption()
	{
		return $"Cue Seen ({Cue})";
	}

	protected override bool CheckCondition()
	{
		if (!CurrentDialog)
		{
			return Game.Instance.DialogState.ShownCuesContains(Cue);
		}
		return Game.Instance.Controllers.DialogController.LocalShownCues.Contains(Cue);
	}
}
