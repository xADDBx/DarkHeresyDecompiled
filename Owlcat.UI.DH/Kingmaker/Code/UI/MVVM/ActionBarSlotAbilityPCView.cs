using System;
using System.Collections.Generic;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.DragNDrop;
using Owlcat.Runtime.Core.Utility;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarSlotAbilityPCView : ActionBarSlotAbilityView
{
	[Header("PCSlot")]
	[SerializeField]
	private ActionBarSlotPCView m_SlotPCView;

	[Header("Drag'n'Drop")]
	[SerializeField]
	private DragNDropHandler m_DragNDropHandler;

	[Header("Critical Effect")]
	[SerializeField]
	private Image m_RestrictedByCriticalEffectIcon;

	[SerializeField]
	private RectTransform m_RestrictedByCriticalEffectBlock;

	private VisibilityController m_RestrictedByCriticalEffectVisibility;

	private IDisposable m_DragNDropSubscription;

	protected override void Awake()
	{
		base.Awake();
		m_RestrictedByCriticalEffectVisibility = VisibilityController.Control(m_RestrictedByCriticalEffectBlock);
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_SlotPCView.Bind(base.ViewModel);
		SetupDragNDrop();
		if (m_RestrictedByCriticalEffectBlock != null)
		{
			base.ViewModel.RestrictedByCriticalEffectIcon.Subscribe(delegate(Sprite value)
			{
				m_RestrictedByCriticalEffectVisibility.SetVisible(value != null);
				m_RestrictedByCriticalEffectIcon.sprite = value;
				m_RestrictedByCriticalEffectIcon.SetHint($"Affected by: {base.ViewModel.RestrictedByCriticalEffectName}");
			}).AddTo(this);
		}
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_DragNDropSubscription?.Dispose();
		m_DragNDropSubscription = null;
		m_SlotPCView.Unbind();
	}

	private void SetupDragNDrop()
	{
		if ((bool)m_DragNDropHandler)
		{
			base.ViewModel.IsEmpty.Subscribe(delegate(bool empty)
			{
				BaseUnitEntity baseUnitEntity = base.ViewModel.MechanicActionBarSlot?.Unit;
				m_DragNDropHandler.CanDrag = !empty && baseUnitEntity != null && (baseUnitEntity.IsMyNetRole() || baseUnitEntity.InPartyAndControllable());
			}).AddTo(this);
			m_DragNDropSubscription = m_DragNDropHandler.OnDragEnd.Subscribe(OnDragEnd).AddTo(this);
			ObservableSubscribeExtensions.Subscribe(base.ViewModel.UpdateDragAndDropState, delegate
			{
				UpdateDragAndDropStateNet();
			}).AddTo(this);
		}
	}

	private void OnDragEnd(GameObject dropTarget)
	{
		ActionBarSlotAbilityPCView targetSlot = dropTarget.Or(null)?.GetComponent<ActionBarSlotAbilityPCView>();
		if ((bool)targetSlot)
		{
			EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
			{
				h.MoveSlot(base.ViewModel.MechanicActionBarSlot, base.Index, targetSlot.Index);
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
			{
				h.DeleteSlot(base.Index);
			});
		}
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

	public void UpdateDragAndDropStateNet()
	{
		m_DragNDropHandler.CanDrag = !base.ViewModel.IsEmpty.CurrentValue && base.ViewModel.MechanicActionBarSlot?.Unit != null && base.ViewModel.MechanicActionBarSlot.Unit.IsMyNetRole();
	}
}
