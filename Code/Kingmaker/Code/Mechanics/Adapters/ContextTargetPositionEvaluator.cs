using System;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Mechanics.Adapters;

[Serializable]
[TypeId("c064b7c6f2194f3aa8dcaf6b45a261cd")]
public class ContextTargetPositionEvaluator : PositionEvaluator
{
	public override string GetCaption()
	{
		return "Evaluate target position of Context";
	}

	protected override Vector3 GetValueInternal()
	{
		return (SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Current ?? throw new FailToEvaluateException(this)).Point;
	}
}
