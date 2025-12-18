using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[PlayerUpgraderAllowed(false)]
[TypeId("afd9ea6813ff47aea53696d7bcc4091d")]
public class RemoveColonyEvent : GameAction
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintColonyEventReference m_Event;

	[SerializeField]
	[Header("Replace with other event if event in Scheduled or Started state")]
	private bool m_ReplaceWithOtherEvent;

	[SerializeField]
	[Header("Add to exclusive planet after remove if exclusive planet exists")]
	private bool m_AddToExclusivePlanet;

	public BlueprintColonyEvent Event => m_Event?.Get();

	public override string GetCaption()
	{
		return $"Remove colony event {Event}";
	}

	protected override void RunAction()
	{
	}
}
