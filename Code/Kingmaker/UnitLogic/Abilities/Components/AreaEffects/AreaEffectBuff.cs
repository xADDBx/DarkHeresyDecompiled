using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Abilities.Components.AreaEffects;

[AllowMultipleComponents]
[TypeId("ebc9e186f0894144d9c1327dab36124a")]
public class AreaEffectBuff : AreaEffectLogic
{
	public ConditionsChecker Condition;

	public bool CheckConditionEveryRound;

	[SerializeField]
	[FormerlySerializedAs("Buff")]
	private BlueprintBuffReference m_Buff;

	public bool ReduceAndAddRanks;

	public BlueprintBuff Buff => m_Buff?.Get();

	protected override void OnEntityEnter(MechanicsContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		TryApplyBuff(context, areaEffect, entity);
	}

	protected override void OnEntityExit(MechanicsContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		TryRemoveBuff(context, areaEffect, entity);
	}

	protected override void OnRound(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		if (!CheckConditionEveryRound)
		{
			return;
		}
		foreach (MechanicEntity item in areaEffect.InGameEntitiesInside)
		{
			if (IsConditionPassed(context, item))
			{
				TryApplyBuff(context, areaEffect, item);
			}
			else
			{
				TryRemoveBuff(context, areaEffect, item);
			}
		}
	}

	private void TryApplyBuff(MechanicsContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		if ((FindAppliedBuff(areaEffect, entity) == null || ReduceAndAddRanks) && IsConditionPassed(context, entity))
		{
			entity.Buffs.Add(Buff, context)?.AddSource(areaEffect);
		}
	}

	private void TryRemoveBuff(MechanicsContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		Buff buff = FindAppliedBuff(areaEffect, entity);
		if (buff != null)
		{
			if (!ReduceAndAddRanks || buff.Rank == 1)
			{
				buff.Remove();
			}
			if (ReduceAndAddRanks)
			{
				buff.RemoveRank();
			}
		}
	}

	private bool IsConditionPassed(MechanicsContext context, MechanicEntity entity)
	{
		using (context.SetScope(entity.ToITargetWrapper()))
		{
			return Condition.Check();
		}
	}

	private Buff FindAppliedBuff(AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		if (!ReduceAndAddRanks)
		{
			return entity.Facts.FindBySource(Buff, areaEffect) as Buff;
		}
		return entity.Buffs.GetBuff(Buff);
	}
}
