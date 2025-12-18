using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("26c2fa8bc6924fcca8f2c134c38813ef")]
public class AnomalyInteractedOutOfScene : Condition
{
	[SerializeField]
	private BlueprintStarSystemMapReference m_SystemMap;

	[SerializeField]
	private BlueprintAnomaly.Reference m_Anomaly;

	protected override string GetConditionCaption()
	{
		return "Check if anomaly interacted when we out of scene";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
