using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("43fcc20ec8304c5dad04eb1bf5839c9c")]
public class FinishColonyEvent : GameAction
{
	[SerializeField]
	private BlueprintColonyEventResult.Reference m_EventResult;

	private BlueprintColonyEventResult EventResult => m_EventResult?.Get();

	public override string GetCaption()
	{
		return "Apply rewards from event";
	}

	protected override void RunAction()
	{
	}
}
