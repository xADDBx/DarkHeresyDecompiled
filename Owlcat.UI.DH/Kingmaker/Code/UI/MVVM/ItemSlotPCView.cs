using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Blueprints.Items;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.DragNDrop;
using Kingmaker.UI.Events;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class ItemSlotPCView : ItemSlotBaseView, IDraggableElement
{
	[SerializeField]
	protected CanvasGroup m_FadeContainer;

	private bool m_BeginDrag;

	[HideInInspector]
	public bool IsDraggable = true;

	[Header("Debug")]
	private bool m_LogInfo;

	private TooltipConfig m_CompareConfig = new TooltipConfig(InfoCallPCMethod.None, InfoCallConsoleMethod.None);

	private TooltipConfig m_MainConfig = new TooltipConfig(InfoCallPCMethod.None, InfoCallConsoleMethod.None);

	private readonly ReactiveCommand<Unit> m_OnBeginDragCommand = new ReactiveCommand<Unit>();

	private readonly ReactiveCommand<Unit> m_OnEndDragCommand = new ReactiveCommand<Unit>();

	public Observable<Unit> OnSingleLeftClickAsObservable => from _ in m_MainButton.OnSingleLeftClickAsObservable()
		where !m_BeginDrag
		select _;

	public Observable<Unit> OnDoubleClickAsObservable => m_MainButton.OnLeftDoubleClickAsObservable();

	public Observable<Unit> OnRightClickAsObservable => m_MainButton.OnRightClickAsObservable();

	public Observable<Unit> OnLeftClickAsObservable => from _ in m_MainButton.OnLeftClickAsObservable()
		where !m_BeginDrag
		select _;

	public Observable<Unit> OnBeginDragCommand => m_OnBeginDragCommand;

	public Observable<Unit> OnEndDragCommand => m_OnEndDragCommand;

	protected override void OnBind()
	{
		base.OnBind();
		CreateTooltip();
		this.SetContextMenu(base.ViewModel.ContextMenu).AddTo(this);
		m_BeginDrag = false;
		SubscribeInteractions();
		UpdateSlotLayer();
	}

	protected override void OnUnbind()
	{
		if (m_BeginDrag)
		{
			DragNDropManager.Instance.Or(null)?.CancelDrag();
			m_BeginDrag = false;
		}
	}

	private void CreateTooltip()
	{
		TooltipPlaces componentInParent = GetComponentInParent<TooltipPlaces>();
		if (componentInParent == null)
		{
			m_MainConfig.TooltipPlace = GetParentContainer();
			m_CompareConfig.TooltipPlace = GetParentContainer();
			m_MainConfig.PriorityPivots = new List<Vector2>
			{
				new Vector2(0f, 0.5f),
				new Vector2(1f, 0.5f)
			};
		}
		else
		{
			m_MainConfig = componentInParent.GetMainTooltipConfig(m_MainConfig);
			m_CompareConfig = componentInParent.GetCompareTooltipConfig(m_CompareConfig);
		}
		this.SetTooltip(base.ViewModel.MainTooltips, base.ViewModel.CompareTooltips, m_MainConfig, m_CompareConfig).AddTo(this);
	}

	private void SubscribeInteractions()
	{
		OnSingleLeftClickAsObservable.Subscribe(OnClick).AddTo(this);
		OnDoubleClickAsObservable.Subscribe(OnDoubleClick).AddTo(this);
		if (IsDraggable)
		{
			this.OnBeginDragAsObservable().Subscribe(OnBeginDrag).AddTo(this);
			this.OnDragAsObservable().Subscribe(OnDrag).AddTo(this);
			this.OnEndDragAsObservable().Subscribe(OnEndDrag).AddTo(this);
			this.OnDropAsObservable().Subscribe(OnDrop).AddTo(this);
		}
	}

	public void SetMainButtonHoverSound(ButtonSoundsEnum soundType)
	{
		UISounds.Instance.SetHoverSound(m_MainButton, soundType);
	}

	private void OnClick()
	{
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnEndDrag(null);
		});
		base.ViewModel.UpdateTooltips(force: true);
		if (m_LogInfo)
		{
			BlueprintItem blueprint = base.ViewModel.Item.CurrentValue.Blueprint;
			Debug.Log(blueprint.AssetName + "\n" + blueprint.AssetGuid);
		}
	}

	private void OnDoubleClick()
	{
	}

	private void OnBeginDrag(PointerEventData eventData)
	{
		m_BeginDrag = true;
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnBeginDrag(eventData, base.gameObject);
		});
		m_OnBeginDragCommand.Execute(Unit.Default);
		UpdateSlotLayer();
	}

	private void OnDrag(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnDrag(eventData);
		});
	}

	private void OnEndDrag(PointerEventData eventData)
	{
		m_BeginDrag = false;
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnEndDrag(eventData);
		});
		m_OnEndDragCommand.Execute(Unit.Default);
		UpdateSlotLayer();
	}

	private void OnDrop(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnDrop(eventData, base.gameObject);
		});
	}

	public void StartDrag()
	{
		m_FadeContainer.DOFade(0.5f, 0.1f).SetUpdate(isIndependentUpdate: true);
		UISounds.Instance.PlayItemSound(SlotAction.Take, base.ViewModel.Item.CurrentValue, equipSound: false);
	}

	public void EndDrag(PointerEventData eventData)
	{
		m_FadeContainer.DOFade(1f, 0.1f).SetUpdate(isIndependentUpdate: true);
		GameObject dropTarget = DragNDropManager.DropTarget;
		if (dropTarget == null)
		{
			if (base.ViewModel is InsertableLootSlotVM)
			{
				ModalWindowsSounds.Instance.Loot.InsertableLootDrop.Play();
			}
			else
			{
				ServiceWindowsSounds.Instance.Inventory.ErrorEquip.Play();
			}
			return;
		}
		ItemSlotVM targetSlot = dropTarget.GetComponent<IItemSlotView>()?.SlotVM ?? dropTarget.GetComponentInParent<IItemSlotView>()?.SlotVM;
		EventBus.RaiseEvent(delegate(INewSlotsHandler h)
		{
			h.HandleTryMoveSlot(base.ViewModel, targetSlot);
		});
		bool isNotable = base.ViewModel.Item.CurrentValue.Blueprint.IsNotable;
		if (UIUtilityItem.GetEquipPosibility(base.ViewModel.Item.CurrentValue)[0] || isNotable || base.ViewModel.Item.CurrentValue is ItemEntitySimple || !(targetSlot is EquipSlotVM))
		{
			UISounds.Instance.PlayItemSound(SlotAction.Put, base.ViewModel.Item.CurrentValue, targetSlot is EquipSlotVM);
		}
		else
		{
			CombatSounds.Instance.Combat.CombatGridCantPerformActionClick.Play();
		}
	}

	public bool SetDragSlot(DragNDropManager slot)
	{
		if (!base.ViewModel.HasItem)
		{
			return false;
		}
		slot.Icon.sprite = base.ViewModel.Item.CurrentValue?.Icon.GetDefaultIfNull(DefaultImageType.Item);
		TextMeshProUGUI count = slot.Count;
		ItemEntity currentValue = base.ViewModel.Item.CurrentValue;
		count.text = ((currentValue == null || !currentValue.IsStackable) ? string.Empty : base.ViewModel.Item.CurrentValue?.Count.ToString());
		slot.OverideSize = new Vector2(96f, 96f);
		return true;
	}

	public void CancelDrag()
	{
		m_BeginDrag = false;
	}
}
