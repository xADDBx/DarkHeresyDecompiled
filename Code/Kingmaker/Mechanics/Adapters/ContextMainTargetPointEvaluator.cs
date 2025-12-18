using System;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Mechanics.Adapters;

[Serializable]
[TypeId("f546ad0a106841039114675666f194fc")]
public class ContextMainTargetPointEvaluator : PositionEvaluator
{
	public override string GetCaption()
	{
		return "Evaluate target position of Context";
	}

	protected override Vector3 GetValueInternal()
	{
		return (SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Current ?? throw new FailToEvaluateException(this)).ClickedTarget.Point;
	}
}
