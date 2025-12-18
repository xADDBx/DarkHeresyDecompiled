using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[Obsolete]
[PlayerUpgraderAllowed(false)]
[TypeId("95919996d0714dd4bc6edb0343023195")]
public class CanStartColonyEvent : Condition
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintColonyEventReference m_Event;

	public BlueprintColonyEvent Event => m_Event?.Get();

	protected override bool CheckCondition()
	{
		return false;
	}

	protected override string GetConditionCaption()
	{
		return $"Can start event {m_Event}";
	}
}
