using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarSlotWeaponAbilityPCView : ActionBarSlotWeaponAbilityView
{
	[Header("PCSlot")]
	[SerializeField]
	private ActionBarSlotPCView m_SlotPCView;

	protected override void OnBind()
	{
		base.OnBind();
		m_SlotPCView.Bind(base.ViewModel);
	}

	public void SetKeyBinding(int index)
	{
		m_SlotPCView.SetKeyBinding(index);
	}

	public void ClearKeyBinding()
	{
		m_SlotPCView.ClearKeyBinding();
	}

	public override void SetTooltipCustomPosition(RectTransform rectTransform, List<Vector2> pivots = null)
	{
		m_SlotPCView.SetTooltipCustomPosition(rectTransform, pivots);
	}
}
