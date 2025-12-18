using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("2813bc95f78eebd42b53689fa079618a")]
public class ContextConditionAlive : ContextCondition
{
	protected override string GetConditionCaption()
	{
		return "";
	}

	protected override bool CheckCondition()
	{
		MechanicEntity entity = base.Target.Entity;
		if (entity != null)
		{
			return !entity.IsDead;
		}
		return false;
	}
}
