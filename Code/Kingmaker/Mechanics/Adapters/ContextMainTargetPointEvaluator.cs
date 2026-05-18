using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Framework;
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
		return (EvalContext.Current.ClickedTarget ?? throw new FailToEvaluateException(this)).Point;
	}
}
