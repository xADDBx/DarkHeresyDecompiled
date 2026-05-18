using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Obsolete("Unused")]
[TypeId("3755ee60f6c4a114489442845b9433c1")]
public class RunActionInFrontOfEnemy : AbilityApplyEffect
{
	public int frontConeAngle;

	public int frontConeRotation;

	public int frontConeRangeMin;

	public int frontConeRangeMax;

	public bool randomRotationOn180;

	public int repeats;

	public int addRangeEachRepeat;

	public ActionList Actions;

	public override void Apply(AbilityExecutionContext context, TargetWrapper target)
	{
		if (target.Entity == null)
		{
			PFLog.Default.Error(context.AbilityBlueprint, "No target.Entity");
			return;
		}
		for (int i = 0; i <= repeats; i++)
		{
			using (EvalContext.PushContext(context, new TargetWrapper(GetActionPosition(target, i))))
			{
				Actions.Run();
			}
		}
	}

	public Vector3 GetActionPosition(TargetWrapper target, int repeat)
	{
		throw new NotImplementedException();
	}
}
