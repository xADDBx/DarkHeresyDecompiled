using System;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("0fe139d89ec84e26a8e69291e53ed09a")]
public class MoveEventFromStartedToScheduled : PlayerUpgraderOnlyAction
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintColonyEventReference m_Event;

	public BlueprintColonyEvent Event => m_Event?.Get();

	public override string GetCaption()
	{
		return $"Move event {Event} from started to scheduled";
	}

	protected override void RunActionOverride()
	{
	}
}
