using System.Linq;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.ContextActions;
using Kingmaker.Gameplay.Features.Concentration;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace Kingmaker.Code.Gameplay.Blueprints;

public static class BodyPartActionsOnHitExtensions
{
	public static bool CanBreakTargetConcentrationIfHit(this BlueprintBodyPart bodyPart, MechanicEntity target, bool checkTargetHasConcentration = true)
	{
		if (checkTargetHasConcentration && target?.GetOptional<PartConcentration>() == null)
		{
			return false;
		}
		ActionList actionList = bodyPart.GetActionList();
		if (actionList == null || !actionList.HasActions)
		{
			return false;
		}
		ContextActionBreakConcentration contextActionBreakConcentration = actionList.Actions.OfType<ContextActionBreakConcentration>().FirstOrDefault();
		if (contextActionBreakConcentration == null)
		{
			return false;
		}
		if (!contextActionBreakConcentration.BreakEvenSteadyConcentration)
		{
			return !target.Features.SteadyConcentration;
		}
		return true;
	}

	public static bool CanChangeTargetTurnOrderIfHit(this BlueprintBodyPart bodyPart)
	{
		ActionList actionList = bodyPart.GetActionList();
		if (actionList == null || !actionList.HasActions)
		{
			return false;
		}
		ContextActionChangeTurnOrderWithinRound contextActionChangeTurnOrderWithinRound = actionList.Actions.OfType<ContextActionChangeTurnOrderWithinRound>().FirstOrDefault();
		if (contextActionChangeTurnOrderWithinRound == null)
		{
			return false;
		}
		return contextActionChangeTurnOrderWithinRound.Steps > 0;
	}

	private static ActionList GetActionList(this BlueprintBodyPart bodyPart)
	{
		return bodyPart.ActionsOnPreciseAttackHit.MaybeBlueprint?.Actions;
	}
}
