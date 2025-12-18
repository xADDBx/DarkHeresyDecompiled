using System;
using Kingmaker.Blueprints.Area;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Obsolete]
[TypeId("b2998c3ab362a7f43b7104bce1b2906b")]
public class TrapTargetPointPosition : PositionEvaluator
{
	protected override Vector3 GetValueInternal()
	{
		return (ContextData<BlueprintTrap.ElementsData>.Current ?? throw new FailToEvaluateException(this)).TrapObject.Settings.TargetPoint.position;
	}

	public override string GetCaption()
	{
		return "Trap target point position";
	}
}
