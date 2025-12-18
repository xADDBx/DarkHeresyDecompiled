using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.ContextConditions;

[TypeId("59c6f1359ff743d5bf8f051c2844864c")]
public class ContextConditionCurrentUnitHasFact : ContextCondition
{
	[SerializeField]
	private BlueprintUnitFactReference m_Fact;

	public BlueprintUnitFact Fact => m_Fact?.Get();

	protected override string GetConditionCaption()
	{
		return "";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.Controllers.TurnController.CurrentUnit?.Facts.Contains(Fact) ?? false;
	}
}
