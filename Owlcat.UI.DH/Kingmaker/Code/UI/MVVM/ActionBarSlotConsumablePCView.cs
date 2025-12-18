using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarSlotConsumablePCView : ActionBarSlotConsumableView
{
	[Header("PCSlot")]
	[SerializeField]
	private ActionBarSlotPCView m_SlotPCView;

	public void SetKeyBinding(int index)
	{
		m_SlotPCView.SetKeyBinding(index);
	}

	public override void SetTooltipCustomPosition(RectTransform rectTransform, List<Vector2> pivots = null)
	{
		m_SlotPCView.SetTooltipCustomPosition(rectTransform, pivots);
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_SlotPCView.Bind(base.ViewModel);
	}

	protected override void OnUnbind()
	{
		m_SlotPCView.Unbind();
	}
}
