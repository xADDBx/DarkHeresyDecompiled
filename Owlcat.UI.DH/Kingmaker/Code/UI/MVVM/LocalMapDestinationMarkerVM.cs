using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UI.Pointer;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LocalMapDestinationMarkerVM : LocalMapMarkerVM
{
	private readonly BaseUnitEntity m_Unit;

	public LocalMapDestinationMarkerVM(BaseUnitEntity unit)
	{
		m_Unit = unit;
		base.MarkerType = LocalMapMarkType.DestinationMark;
		m_IsVisible.Value = false;
		m_Position.Value = m_Unit.Position;
	}

	protected override void OnUpdateHandler()
	{
		BaseUnitEntity unit = m_Unit;
		if (unit != null && !unit.IsDisposed)
		{
			Dictionary<BaseUnitEntity, Vector3> unitMarksLocalMap = ClickPointerManager.Instance.UnitMarksLocalMap;
			m_IsVisible.Value = unitMarksLocalMap.ContainsKey(m_Unit);
			if (base.IsVisible.CurrentValue)
			{
				m_Position.Value = unitMarksLocalMap[m_Unit];
			}
		}
	}

	public override Entity GetEntity()
	{
		return m_Unit;
	}
}
