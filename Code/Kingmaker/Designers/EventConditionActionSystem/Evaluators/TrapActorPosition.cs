using System;
using Kingmaker.Blueprints.Area;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Obsolete]
[TypeId("e83ccd5e6cc6b7b4499c8b3f53dbfe68")]
public class TrapActorPosition : PositionEvaluator
{
	protected override Vector3 GetValueInternal()
	{
		return (ContextData<BlueprintTrap.ElementsData>.Current ?? throw new FailToEvaluateException(this)).TrapObject.Position;
	}

	public override string GetCaption()
	{
		return "Trap actor position";
	}
}
