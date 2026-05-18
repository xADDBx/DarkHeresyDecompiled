using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Framework;
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
		return (EvalContext.Current.Target ?? throw new FailToEvaluateException(this)).Point;
	}
}
