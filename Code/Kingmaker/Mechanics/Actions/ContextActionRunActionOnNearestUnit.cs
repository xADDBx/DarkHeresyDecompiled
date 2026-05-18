using System.Collections.Generic;
using System.Linq;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[TypeId("b45b69ed39bb4d9eae68c2927e09cb33")]
public class ContextActionRunActionOnNearestUnit : ContextAction
{
	public ConditionsChecker Condition;

	public ActionList Actions;

	public bool RunOnAllTargets;

	public override string GetCaption()
	{
		return "Run action on nearest unit, that meets the conditions";
	}

	protected override void RunAction()
	{
		TargetWrapper clickedTarget = base.Context.ClickedTarget;
		if (clickedTarget == null)
		{
			return;
		}
		Vector3 target = clickedTarget.Point;
		MechanicEntity targetEntity = clickedTarget.Entity;
		List<MechanicEntity> targets = Game.Instance.EntityPools.AllBaseUnits.Where((BaseUnitEntity p) => !p.Features.IsUntargetable && !p.LifeState.IsDead && p.IsInCombat && IsConditionPassed(p)).Cast<MechanicEntity>().ToList();
		IEnumerable<MechanicEntity> enumerable = targets.Where((MechanicEntity p) => p.DistanceToInCells(target) == targets.Min((MechanicEntity p1) => p1.DistanceToInCells(target)) && p != targetEntity);
		if (!RunOnAllTargets)
		{
			MechanicEntity mechanicEntity = enumerable.Random(PFStatefulRandom.Mechanics);
			if (mechanicEntity == null)
			{
				return;
			}
			using (base.Context.PushTarget(mechanicEntity))
			{
				Actions.Run();
				return;
			}
		}
		foreach (MechanicEntity item in enumerable)
		{
			using (base.Context.PushTarget(item))
			{
				Actions.Run();
			}
		}
	}

	private bool IsConditionPassed(BaseUnitEntity unit)
	{
		using (base.Context.PushTarget(unit))
		{
			return Condition.Check();
		}
	}
}
