using Kingmaker.UnitLogic.Mechanics.Conditions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.ContextConditions;

[TypeId("3b0a04acd0de464d89ef79612571f75c")]
public class ContextConditionCurrentUnitIsPartyMember : ContextCondition
{
	protected override string GetConditionCaption()
	{
		return "";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.Controllers.TurnController.CurrentUnit?.IsInPlayerParty ?? false;
	}
}
