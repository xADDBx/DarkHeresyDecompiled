using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using UnityEngine;

namespace Kingmaker.View;

public class UnitLocalMapMarker : ILocalMapMarker
{
	private readonly UnitEntityView m_Unit;

	public bool IsDisposed
	{
		get
		{
			UnitEntityView unit = m_Unit;
			int num;
			if ((object)unit != null)
			{
				BaseUnitEntity entityData = unit.EntityData;
				if (entityData != null)
				{
					num = ((!entityData.IsDisposed) ? 1 : 0);
					goto IL_0020;
				}
			}
			num = 0;
			goto IL_0020;
			IL_0020:
			return num == 0;
		}
	}

	public UnitLocalMapMarker(UnitEntityView unit)
	{
		m_Unit = unit;
	}

	public LocalMapMarkType GetMarkerType()
	{
		if (m_Unit == null)
		{
			return LocalMapMarkType.Invalid;
		}
		if (m_Unit.EntityData.IsDeadAndHasLoot)
		{
			return LocalMapMarkType.Loot;
		}
		if (m_Unit.EntityData.Portrait.HasPortrait)
		{
			return LocalMapMarkType.Unit;
		}
		return LocalMapMarkType.VeryImportantPerson;
	}

	public string GetDescription()
	{
		return m_Unit.EntityData.CharacterName;
	}

	public Vector3 GetPosition()
	{
		if (!(m_Unit != null))
		{
			return Vector3.zero;
		}
		return m_Unit.ViewTransform.position;
	}

	public bool IsVisible()
	{
		return m_Unit.IsVisible;
	}

	public bool IsMapObject()
	{
		return false;
	}

	public Entity GetEntity()
	{
		return m_Unit.Data;
	}
}
