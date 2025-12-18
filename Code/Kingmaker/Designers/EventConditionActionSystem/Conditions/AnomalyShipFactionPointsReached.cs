using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("2ee0e9573a7643afbf57c7cd1cd9fa18")]
public class AnomalyShipFactionPointsReached : Condition
{
	[SerializeField]
	private int m_Reputation;

	protected override string GetConditionCaption()
	{
		return "Check faction of currently interacted anomaly";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
