using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities;

public class AbilityPatternCache
{
	private List<MechanicEntity> m_UnitsInPattern = new List<MechanicEntity>();

	private AbilityData m_Ability;

	private TargetWrapper m_Target;

	private Vector3 m_CasterPosition;

	public IReadOnlyList<MechanicEntity> Get(AbilityData ability, Vector3 casterPosition, TargetWrapper target)
	{
		if (ability.IsSingleTarget)
		{
			return null;
		}
		float sqrMagnitude = (casterPosition - m_CasterPosition).sqrMagnitude;
		if (ability == m_Ability && target == m_Target && sqrMagnitude < float.Epsilon)
		{
			return m_UnitsInPattern;
		}
		m_UnitsInPattern.Clear();
		m_Ability = ability;
		m_Target = target;
		m_CasterPosition = casterPosition;
		foreach (GridNodeBase node in ability.GetPattern(target, casterPosition).Nodes)
		{
			BaseUnitEntity firstUnit = node.GetFirstUnit();
			if (firstUnit != null)
			{
				m_UnitsInPattern.Add(firstUnit);
			}
		}
		return m_UnitsInPattern;
	}
}
