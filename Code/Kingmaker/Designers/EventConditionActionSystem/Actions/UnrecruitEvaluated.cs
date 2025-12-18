using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("59eaeb98b3814fd893bf044869305f2c")]
public class UnrecruitEvaluated : GameAction
{
	[SerializeReference]
	public AbstractUnitEvaluator? CompanionEvaluator;

	[SerializeField]
	public ActionList? OnUnrecruit;

	protected override void RunAction()
	{
		BlueprintUnit blueprintUnit = CompanionEvaluator?.GetValue().Blueprint;
		if (blueprintUnit == null)
		{
			Element.LogError($"Unit evaluation failed for {CompanionEvaluator}");
		}
		else
		{
			Unrecruit.UnrecruitUnit(blueprintUnit, OnUnrecruit);
		}
	}

	public override string GetCaption()
	{
		return $"Unrecruit ({CompanionEvaluator})";
	}
}
