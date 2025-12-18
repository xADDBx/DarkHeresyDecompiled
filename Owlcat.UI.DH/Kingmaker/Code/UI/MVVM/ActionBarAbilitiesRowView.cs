using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarAbilitiesRowView : MonoBehaviour
{
	[SerializeField]
	private WidgetList m_WidgetList;

	private IDisposable m_Disposes;

	private RectTransform m_TooltipCustomPosition;

	private List<Vector2> m_TooltipCustomPivots;

	public void Initialize(RectTransform tooltipPosition, List<Vector2> tooltipPivots)
	{
		m_TooltipCustomPosition = tooltipPosition;
		m_TooltipCustomPivots = tooltipPivots;
	}

	public IDisposable DrawEntries(List<ActionBarSlotVM> slots, ActionBarSlotAbilityView prefab)
	{
		m_Disposes?.Dispose();
		m_Disposes = m_WidgetList.DrawEntries(slots.ToArray(), prefab, unused: true);
		SetActionBarSlotsTooltipCustomPosition();
		return m_Disposes;
	}

	public List<MonoBehaviour> GetSlots()
	{
		return m_WidgetList.Entries.Cast<MonoBehaviour>().ToList();
	}

	public List<IConsoleNavigationEntity> GetConsoleEntities()
	{
		return m_WidgetList.Entries.Cast<IConsoleNavigationEntity>().ToList();
	}

	public IConsoleNavigationEntity GetFirstValidEntity()
	{
		return GetConsoleEntities().FirstOrDefault((IConsoleNavigationEntity e) => e?.IsValid() ?? false);
	}

	private void SetActionBarSlotsTooltipCustomPosition()
	{
		if (m_TooltipCustomPosition == null || m_TooltipCustomPivots == null)
		{
			return;
		}
		foreach (IBindable entry in m_WidgetList.Entries)
		{
			if (entry is ActionBarSlotAbilityView actionBarSlotAbilityView)
			{
				actionBarSlotAbilityView.Initialize();
				actionBarSlotAbilityView.SetTooltipCustomPosition(m_TooltipCustomPosition, m_TooltipCustomPivots);
			}
		}
	}
}
