using Kingmaker.ElementsSystem;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("96a3a467d8a2dca418e387d0c18c483e")]
public class DoorOpen : Condition
{
	[SerializeReference]
	public MapObjectEvaluator Door;

	protected override string GetConditionCaption()
	{
		return $"{Door} is open";
	}

	protected override bool CheckCondition()
	{
		return Door.GetValue().GetOptional<InteractionDoorPart>().IsOpen;
	}
}
