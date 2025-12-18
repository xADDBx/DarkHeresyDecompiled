using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarPartConsumablesPCView : View<ActionBarPartConsumablesVM>
{
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private ActionBarSlotConsumablePCView m_SlotPCView;

	[SerializeField]
	private RectTransform m_LeftSideTooltipPlace;

	private List<Vector2> m_LeftSideTooltipPivots { get; } = new List<Vector2>
	{
		new Vector2(1f, 0f),
		new Vector2(0.9f, 0f),
		new Vector2(0.8f, 0f),
		new Vector2(0.7f, 0f),
		new Vector2(0.6f, 0f)
	};


	public void Initialize()
	{
	}

	protected override void OnBind()
	{
		OwlcatR3UnitExtensions.Subscribe(base.ViewModel.UnitChanged, delegate
		{
			DrawEntries();
		}).AddTo(this);
		DrawEntries();
	}

	private void DrawEntries()
	{
		m_WidgetList.DrawEntries(base.ViewModel.Slots.ToArray(), m_SlotPCView).AddTo(this);
		SetActionBarSlotsTooltipCustomPosition();
		SetBindings();
	}

	private void SetActionBarSlotsTooltipCustomPosition()
	{
		foreach (IBindable entry in m_WidgetList.Entries)
		{
			if (entry is ActionBarSlotConsumablePCView actionBarSlotConsumablePCView)
			{
				actionBarSlotConsumablePCView.SetTooltipCustomPosition(m_LeftSideTooltipPlace, m_LeftSideTooltipPivots);
			}
		}
	}

	private void SetBindings()
	{
		for (int i = 0; i < m_WidgetList.Entries?.Count; i++)
		{
			(m_WidgetList.Entries[i] as ActionBarSlotConsumablePCView).Or(null)?.SetKeyBinding(i);
		}
	}

	protected override void OnUnbind()
	{
		m_WidgetList.Clear();
	}
}
