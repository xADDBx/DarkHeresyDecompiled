using System.Collections.Generic;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.DragNDrop;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoFeaturePCView : CharInfoFeatureSimpleBaseView
{
	[Header("Tooltip")]
	[SerializeField]
	protected RectTransform m_LeftSideTooltipPlace;

	[Header("Drag'n'Drop")]
	[SerializeField]
	private DragNDropHandler m_DragNDropHandler;

	[SerializeField]
	private bool m_CanDrag = true;

	private List<Vector2> LeftSideTooltipPivot { get; } = new List<Vector2>
	{
		new Vector2(1f, 0.5f)
	};


	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetTooltip();
		SetupDragNDrop();
	}

	private void SetTooltip()
	{
		if (base.ViewModel.Tooltip != null)
		{
			AddDisposable(this.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, m_LeftSideTooltipPlace, 0, 0, 0, LeftSideTooltipPivot)));
		}
	}

	private void SetupDragNDrop()
	{
		if (!(m_DragNDropHandler == null))
		{
			MechanicEntity mechanicEntity = base.ViewModel.Ability?.Owner;
			bool flag = m_CanDrag && base.ViewModel.Ability != null && (mechanicEntity.IsMyNetRole() || mechanicEntity.InPartyAndControllable());
			m_DragNDropHandler.CanDrag = flag;
			if (flag)
			{
				AddDisposable(m_DragNDropHandler.OnDragEnd.Subscribe(OnDragEnd));
			}
		}
	}

	private void OnDragEnd(GameObject dropTarget)
	{
		ActionBarSlotAbilityPCView targetSlot = dropTarget.Or(null)?.GetComponent<ActionBarSlotAbilityPCView>();
		if ((bool)targetSlot)
		{
			EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
			{
				h.MoveSlot(base.ViewModel.Ability, targetSlot.Index);
			});
		}
	}
}
