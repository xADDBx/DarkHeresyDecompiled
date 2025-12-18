using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("3c60436a963649e8ad359c994d3ef94f")]
public class RecruitInactiveEvaluated : GameAction
{
	[SerializeReference]
	public AbstractUnitEvaluator? CompanionEvaluator;

	public ActionList? OnRecruit;

	protected override void RunAction()
	{
		BlueprintUnit blueprintUnit = CompanionEvaluator?.GetValue().Blueprint;
		if (blueprintUnit == null)
		{
			Element.LogError($"Unit evaluation failed for {CompanionEvaluator}");
		}
		else
		{
			RecruitInactive.RecruitInactiveUnit(blueprintUnit, OnRecruit);
		}
	}

	public override string GetCaption()
	{
		return $"Recruit ({CompanionEvaluator}) to capital";
	}
}
