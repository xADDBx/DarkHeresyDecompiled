using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Conditions;

[TypeId("886856ee142344969699d0e3e7c0b464")]
public class IsRace : Condition
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public Race Race;

	protected override string GetConditionCaption()
	{
		return $"{Unit} Race";
	}

	protected override bool CheckCondition()
	{
		if (Unit.GetValue() is BaseUnitEntity baseUnitEntity)
		{
			return baseUnitEntity.Progression.Race?.RaceId == Race;
		}
		return false;
	}
}
