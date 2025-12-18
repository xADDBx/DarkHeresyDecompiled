using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("006dc20d2ed94924a340a339b6e146ef")]
public class AnomalyShipFactionLevelReached : Condition
{
	[SerializeField]
	private int m_ReputationLvl;

	protected override string GetConditionCaption()
	{
		return "Check faction of currently interacted anomaly";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
