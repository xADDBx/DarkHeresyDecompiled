using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[PlayerUpgraderAllowed(false)]
[TypeId("c4737cb044be4448a075d120d4b70cdc")]
public class ExpeditionSend : Condition
{
	[SerializeField]
	private BlueprintStarSystemObjectReference m_StarSystemObject;

	[SerializeField]
	[Tooltip("Can be left as null, then check for any expedition from planet")]
	[CanBeNull]
	private BlueprintPointOfInterestReference m_PointOfInterest;

	protected override string GetConditionCaption()
	{
		return "Have expedition from " + m_StarSystemObject?.Get()?.Name;
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
