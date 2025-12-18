using System;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("d1933e412cc64482ae7990aaacdf44a8")]
public class ContextActionRestoreActionPoints : ContextAction
{
	[HideIf("ActionPointsToMax")]
	public ContextValue ActionPoints;

	[HideIf("MovePointsToMax")]
	public ContextValue MovePoints;

	[HideIf("IgnoreActionPointsMaximum")]
	public bool ActionPointsToMax;

	[HideIf("IgnoreMovePointsMaximum")]
	public bool MovePointsToMax;

	[HideIf("MovePointsToMax")]
	public bool IgnoreMovePointsMaximum;

	[HideIf("ActionPointsToMax")]
	public bool IgnoreActionPointsMaximum;

	protected override void RunAction()
	{
		MechanicEntity mechanicEntity = base.Target?.Entity;
		if (mechanicEntity == null)
		{
			return;
		}
		PartUnitCombatState combatStateOptional = mechanicEntity.GetCombatStateOptional();
		if (combatStateOptional != null)
		{
			int actionPointsMax = combatStateOptional.ActionPointsMax;
			int num = ActionPoints.Calculate(base.Context);
			if (IgnoreActionPointsMaximum && num != 0)
			{
				combatStateOptional.GainActionPoints(num, base.Context);
			}
			else if (ActionPointsToMax || num != 0)
			{
				int value = (ActionPointsToMax ? actionPointsMax : Math.Clamp(combatStateOptional.ActionPoints + num, 0, actionPointsMax));
				combatStateOptional.SetActionPoints(value, base.Context);
			}
			int movementPointsMax = combatStateOptional.MovementPointsMax;
			int num2 = MovePoints.Calculate(base.Context);
			if (IgnoreMovePointsMaximum && num2 != 0)
			{
				combatStateOptional.GainMovementPoints(num2, base.Context);
			}
			else if (MovePointsToMax || num2 != 0)
			{
				float value2 = (MovePointsToMax ? ((float)movementPointsMax) : Math.Clamp(combatStateOptional.MovementPoints + (float)num2, 0f, movementPointsMax));
				combatStateOptional.SetMovementPoints(value2, base.Context);
			}
			EventBus.RaiseEvent((IMechanicEntity)mechanicEntity, (Action<IUnitActionPointsHandler>)delegate(IUnitActionPointsHandler h)
			{
				h.HandleRestoreActionPoints();
			}, isCheckRuntime: true);
		}
	}

	public override string GetCaption()
	{
		return "Restore Action Points and Move Points";
	}
}
