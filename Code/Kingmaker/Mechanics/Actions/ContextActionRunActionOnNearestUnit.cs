using System.Collections.Generic;
using System.Linq;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
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
		MechanicsContext current = SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Current;
		if (current == null)
		{
			return;
		}
		Vector3 target = current.ClickedTarget.Point;
		MechanicEntity targetEntity = current.ClickedTarget.Entity;
		List<MechanicEntity> targets = Game.Instance.EntityPools.AllBaseUnits.Where((BaseUnitEntity p) => !p.Features.IsUntargetable && !p.LifeState.IsDead && p.IsInCombat && IsConditionPassed(base.Context, p)).Cast<MechanicEntity>().ToList();
		IEnumerable<MechanicEntity> enumerable = targets.Where((MechanicEntity p) => p.DistanceToInCells(target) == targets.Min((MechanicEntity p1) => p1.DistanceToInCells(target)) && p != targetEntity);
		if (!RunOnAllTargets)
		{
			MechanicEntity mechanicEntity = enumerable.Random(PFStatefulRandom.Mechanics);
			if (mechanicEntity == null)
			{
				return;
			}
			using (base.Context.SetScope(mechanicEntity, null))
			{
				Actions.Run();
				return;
			}
		}
		foreach (MechanicEntity item in enumerable)
		{
			using (base.Context.SetScope(item, null))
			{
				Actions.Run();
			}
		}
	}

	private bool IsConditionPassed(MechanicsContext context, BaseUnitEntity unit)
	{
		using (context.SetScope(unit.ToITargetWrapper()))
		{
			return Condition.Check();
		}
	}
}
