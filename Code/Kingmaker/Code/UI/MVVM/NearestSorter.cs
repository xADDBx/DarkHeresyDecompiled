using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

internal class NearestSorter : IComparer<Entity>
{
	private BaseUnitEntity m_Unit;

	public void Reset(BaseUnitEntity unit)
	{
		m_Unit = unit;
	}

	public int Compare(Entity x, Entity y)
	{
		return GetScore(y).CompareTo(GetScore(x));
	}

	private float GetScore(Entity entity)
	{
		Vector3 vector = entity.Position - m_Unit.Position;
		if (vector == Vector3.zero)
		{
			return float.PositiveInfinity;
		}
		float num = Vector3.Dot(m_Unit.Rotation * Vector3.forward, vector);
		if (num < 0f)
		{
			return float.NegativeInfinity;
		}
		return num / vector.sqrMagnitude;
	}
}
