using System;
using System.Collections.Generic;
using Kingmaker.Controllers.Optimization;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("e5ea156d7248471887237e2c871de79a")]
public class UnitsInRangeGetter : IntPropertyGetter, PropertyContextAccessor.ICurrentTarget, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalMechanicContext, PropertyContextAccessor.IOptional
{
	[Flags]
	private enum UnitCombatGroup
	{
		Ally = 1,
		Enemy = 2
	}

	private const float HalfDiagonal = 0.95f;

	[SerializeField]
	private PropertyCalculator m_Value = new PropertyCalculator();

	[SerializeField]
	private UnitCombatGroup m_CombatGroup;

	[SerializeField]
	private int m_RangeInCells;

	public ContextValue Value;

	protected override int GetBaseValue()
	{
		if (!(base.CurrentEntity is BaseUnitEntity baseUnitEntity))
		{
			PFLog.Default.Error(this, "Target is missing");
			return 0;
		}
		int num = m_RangeInCells;
		if (Value != null)
		{
			num += Value.Calculate(EvalContext.Current);
		}
		float num2 = ((m_RangeInCells == 1) ? 0.95f : 0f);
		List<BaseUnitEntity> list = EntityBoundsHelper.FindUnitsInRange(baseUnitEntity.Position, num.Cells().Meters + num2);
		int num3 = 0;
		foreach (BaseUnitEntity item in list)
		{
			if (item.UniqueId != baseUnitEntity.UniqueId && item.LifeState.IsConscious && IsSuitableCombatGroup(item, baseUnitEntity) && (m_Value.Empty || m_Value.GetBoolValue(item)))
			{
				num3++;
			}
		}
		return num3;
	}

	private bool IsSuitableCombatGroup(BaseUnitEntity unit, BaseUnitEntity target)
	{
		if (m_CombatGroup.HasFlag(UnitCombatGroup.Ally) && m_CombatGroup.HasFlag(UnitCombatGroup.Enemy))
		{
			return true;
		}
		if (m_CombatGroup != UnitCombatGroup.Ally || unit.IsEnemy(target))
		{
			if (m_CombatGroup == UnitCombatGroup.Enemy)
			{
				return unit.IsEnemy(target);
			}
			return false;
		}
		return true;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (m_CombatGroup.HasFlag(UnitCombatGroup.Ally) && m_CombatGroup.HasFlag(UnitCombatGroup.Enemy))
		{
			return $"Amount of units in range of {m_RangeInCells} cells around {FormulaTargetScope.Current}";
		}
		return string.Format("Amount of {0} units in range of {1} cells around {2}", m_CombatGroup switch
		{
			UnitCombatGroup.Ally => "Ally", 
			UnitCombatGroup.Enemy => "Enemy", 
			_ => "NONE SELECTED", 
		}, m_RangeInCells, FormulaTargetScope.Current);
	}
}
