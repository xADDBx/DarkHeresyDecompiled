using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("4006e8b4ece547fa9684da5dd2e51bff")]
public class GiveExpeditionReward : GameAction
{
	private BlueprintStarSystemObjectReference m_StarSystemObject;

	[SerializeField]
	[Tooltip("Can be left as null, then check for any expedition from planet")]
	[CanBeNull]
	private BlueprintPointOfInterestReference m_PointOfInterest;

	public override string GetCaption()
	{
		return "Have expedition from " + m_StarSystemObject?.Get()?.Name;
	}

	protected override void RunAction()
	{
	}
}
