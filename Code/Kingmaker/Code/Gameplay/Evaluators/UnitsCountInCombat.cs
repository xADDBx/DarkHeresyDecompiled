using System.Collections.Generic;
using System.Linq;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Evaluators;

[TypeId("92803e2005e9469d84831b1c7cb76cd3")]
public class UnitsCountInCombat : IntEvaluator
{
	public ConditionsChecker Conditions;

	protected override int GetValueInternal()
	{
		List<BaseUnitEntity> list = Game.Instance.EntityPools.AllBaseUnits.Where((BaseUnitEntity p) => !p.Features.IsUntargetable && !p.LifeState.IsDead && p.IsInCombat).ToList();
		List<BaseUnitEntity> list2 = new List<BaseUnitEntity>();
		foreach (BaseUnitEntity item in list)
		{
			using (EvalContext.PushContext(item.Context, item))
			{
				if (Conditions.Check())
				{
					list2.Add(item);
				}
			}
		}
		return list2.Count;
	}

	public override string GetCaption()
	{
		return "Count of live targetable units in combat";
	}
}
