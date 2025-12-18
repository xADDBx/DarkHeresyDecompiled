using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarWeaponSetPCView : View<ActionBarPartWeaponSetVM>
{
	[SerializeField]
	private OwlcatButton m_SwitchWeaponButton;

	[SerializeField]
	private ActionBarSlotWeaponPCView m_MainHandWeapon;

	[SerializeField]
	private ActionBarSlotWeaponPCView m_OffHandWeapon;

	[SerializeField]
	private ActionBarSlotWeaponAbilityPCView m_SlotPCView;

	[SerializeField]
	private WidgetList m_MainHandWidgetList;

	[SerializeField]
	private WidgetList m_OffHandWidgetList;

	[SerializeField]
	private WidgetList m_ComboHandWidgetList;

	[SerializeField]
	private Image m_CurrentSetImage;

	private Action m_SwitchButtonCallback;

	[SerializeField]
	private RectTransform m_LeftSideTooltipPlace;

	private bool m_SetKeyBindings;

	private List<ActionBarSlotWeaponAbilityPCView> m_BoundSlots;

	public bool IsCurrent => base.ViewModel?.IsCurrent.CurrentValue ?? false;

	private List<Vector2> m_LeftSideTooltipPivots { get; } = new List<Vector2>
	{
		new Vector2(1f, 0f),
		new Vector2(0.9f, 0f),
		new Vector2(0.8f, 0f),
		new Vector2(0.7f, 0f),
		new Vector2(0.6f, 0f)
	};


	public void Initialize(bool setKeyBindings)
	{
		m_SetKeyBindings = setKeyBindings;
	}

	public void SetSwitchButtonCallback(Action callback)
	{
		m_SwitchButtonCallback = callback;
	}

	protected override void OnBind()
	{
		m_MainHandWeapon.SetTooltipCustomPosition(m_LeftSideTooltipPlace, m_LeftSideTooltipPivots);
		m_OffHandWeapon.SetTooltipCustomPosition(m_LeftSideTooltipPlace, m_LeftSideTooltipPivots);
		base.ViewModel.IsCurrent.Subscribe(delegate(bool val)
		{
			m_CurrentSetImage.Or(null)?.gameObject.SetActive(val);
		}).AddTo(this);
		base.ViewModel.MainHandWeapon.CombineLatest(base.ViewModel.OffHandWeapon, (ItemSlotVM _, ItemSlotVM _) => true).Subscribe(delegate
		{
			if (base.ViewModel.IsTwoHanded)
			{
				m_MainHandWeapon.Bind(base.ViewModel.MainHandWeapon.CurrentValue);
				m_OffHandWeapon.Bind(base.ViewModel.MainHandWeapon.CurrentValue);
			}
			else
			{
				m_MainHandWeapon.Bind(base.ViewModel.MainHandWeapon.CurrentValue);
				m_OffHandWeapon.Bind(base.ViewModel.OffHandWeapon.CurrentValue);
			}
			m_OffHandWeapon.SetFakeMode(base.ViewModel.IsTwoHanded);
		}).AddTo(this);
		OwlcatR3UnitExtensions.Subscribe(base.ViewModel.SlotsUpdated, delegate
		{
			DrawEntries();
		}).AddTo(this);
		if (m_SwitchWeaponButton != null)
		{
			m_SwitchWeaponButton.OnLeftClickAsObservable().Subscribe(OnClick).AddTo(this);
		}
		DrawEntries();
	}

	private void OnClick()
	{
		m_SwitchButtonCallback?.Invoke();
	}

	private void DrawEntries()
	{
		m_MainHandWidgetList.Clear();
		m_OffHandWidgetList.Clear();
		m_ComboHandWidgetList.Clear();
		m_MainHandWidgetList.DrawEntries(base.ViewModel.MainHandSlots, m_SlotPCView).AddTo(this);
		m_OffHandWidgetList.DrawEntries(base.ViewModel.OffHandSlots, m_SlotPCView).AddTo(this);
		m_ComboHandWidgetList.DrawEntries(base.ViewModel.ComboHandsSlots, m_SlotPCView).AddTo(this);
		SetActionBarSlotsTooltipCustomPosition(m_MainHandWidgetList);
		SetActionBarSlotsTooltipCustomPosition(m_OffHandWidgetList);
		SetActionBarSlotsTooltipCustomPosition(m_ComboHandWidgetList);
		BindSlots();
	}

	private void BindSlots()
	{
		ClearSlots();
		GetSlots(ref m_BoundSlots);
		for (int i = 0; i < m_BoundSlots.Count; i++)
		{
			m_BoundSlots[i].SetInteractable(m_SetKeyBindings);
			if (m_SetKeyBindings)
			{
				m_BoundSlots[i].SetKeyBinding(i);
			}
		}
	}

	private void ClearSlots()
	{
		if (m_BoundSlots == null)
		{
			return;
		}
		foreach (ActionBarSlotWeaponAbilityPCView boundSlot in m_BoundSlots)
		{
			boundSlot.ClearKeyBinding();
		}
	}

	private void SetActionBarSlotsTooltipCustomPosition(WidgetList widgetList)
	{
		foreach (IBindable entry in widgetList.Entries)
		{
			if (entry is ActionBarSlotWeaponAbilityPCView actionBarSlotWeaponAbilityPCView)
			{
				actionBarSlotWeaponAbilityPCView.SetTooltipCustomPosition(m_LeftSideTooltipPlace, m_LeftSideTooltipPivots);
			}
		}
	}

	protected override void OnUnbind()
	{
		m_MainHandWidgetList.Clear();
		m_OffHandWidgetList.Clear();
		m_ComboHandWidgetList.Clear();
		ClearSlots();
	}

	public void SwitchWeapon()
	{
		base.ViewModel.SwitchWeapon();
	}

	private void GetSlots(ref List<ActionBarSlotWeaponAbilityPCView> slots)
	{
		slots?.Clear();
		if (slots == null)
		{
			slots = new List<ActionBarSlotWeaponAbilityPCView>();
		}
		if ((m_MainHandWidgetList.Or(null)?.Entries?.Any()).GetValueOrDefault())
		{
			slots.AddRange(m_MainHandWidgetList.Entries.Select((IBindable e) => e as ActionBarSlotWeaponAbilityPCView));
		}
		if ((m_OffHandWidgetList.Or(null)?.Entries?.Any()).GetValueOrDefault())
		{
			slots.AddRange(m_OffHandWidgetList.Entries.Select((IBindable e) => e as ActionBarSlotWeaponAbilityPCView));
		}
		if ((m_ComboHandWidgetList.Or(null)?.Entries?.Any()).GetValueOrDefault())
		{
			slots.AddRange(m_ComboHandWidgetList.Entries.Select((IBindable e) => e as ActionBarSlotWeaponAbilityPCView));
		}
	}
}
