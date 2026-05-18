using Kingmaker.ElementsSystem;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("487f9724f43edfb4d831b0d59cd7c885")]
public class DoorCanClose : Condition
{
	[SerializeReference]
	public MapObjectEvaluator Door;

	public bool CheckOutsideCombat;

	protected override string GetConditionCaption()
	{
		if (!CheckOutsideCombat)
		{
			return $"{Door} can be closed";
		}
		return $"{Door} can be closed (checked even outside combat)";
	}

	protected override bool CheckCondition()
	{
		return (Door?.GetValue()?.GetOptional<InteractionDoorPart>())?.CheckCanClose(CheckOutsideCombat) ?? true;
	}
}
