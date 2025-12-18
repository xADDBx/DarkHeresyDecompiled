using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("49ad6acf6b67a114da523f8e353e1c25")]
public class StarshipMicroabilityCooldown : ContextAction
{
	[SerializeField]
	private bool m_UpgradedCooldown;

	[SerializeField]
	private BlueprintFeatureReference m_CostReduction1;

	[SerializeField]
	private BlueprintFeatureReference m_CostReduction2;

	public override string GetCaption()
	{
		return "Start random cooldown for starship microability";
	}

	protected override void RunAction()
	{
	}
}
