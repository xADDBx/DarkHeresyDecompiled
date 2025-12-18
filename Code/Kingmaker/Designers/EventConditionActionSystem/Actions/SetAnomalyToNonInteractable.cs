using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("e3e5cb605167439d917581a5bd82dd85")]
public class SetAnomalyToNonInteractable : GameAction
{
	[SerializeField]
	private BlueprintAnomaly.Reference m_Anomaly;

	private BlueprintAnomaly Anomaly => m_Anomaly?.Get();

	public override string GetCaption()
	{
		return "Set " + Anomaly.Name + " to non interactable";
	}

	protected override void RunAction()
	{
	}
}
