using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("2cb0d3578ce44f57ac8edc1fb0dd1f57")]
public class UnitsInCombatGetter : IntPropertyGetter, PropertyContextAccessor.IMechanicContext, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public ConditionsChecker Conditions;

	protected override int GetBaseValue()
	{
		List<BaseUnitEntity> list = Game.Instance.EntityPools.AllBaseUnits.Where((BaseUnitEntity p) => !p.Features.IsUntargetable && !p.LifeState.IsDead && p.IsInCombat).ToList();
		List<BaseUnitEntity> list2 = new List<BaseUnitEntity>();
		foreach (BaseUnitEntity item in list)
		{
			using (EvalContext.Current.PushTarget(item))
			{
				if (Conditions.Check())
				{
					list2.Add(item);
				}
			}
		}
		return list2.Count;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Count of live targetable units in combat";
	}
}
