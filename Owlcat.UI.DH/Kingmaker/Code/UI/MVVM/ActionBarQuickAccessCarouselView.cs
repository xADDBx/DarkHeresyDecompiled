using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarQuickAccessCarouselView : MonoBehaviour, IDisposable
{
	[SerializeField]
	private OwlcatMultiSelectable m_ActiveButton;

	[SerializeField]
	private ActionBarBaseSlotView m_MainSlotView;

	[SerializeField]
	private ActionBarBaseSlotView m_NextSlotView;

	[SerializeField]
	private ActionBarBaseSlotView m_PreviousSlotView;

	private readonly ReactiveProperty<bool> m_IsActive = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasSlots = new ReactiveProperty<bool>();

	private List<ActionBarSlotVM> m_SlotVMs;

	private ReactiveProperty<ActionBarSlotVM> m_MainSlotVM;

	private readonly ReactiveProperty<ActionBarSlotVM> m_NextSlotVM = new ReactiveProperty<ActionBarSlotVM>();

	private readonly ReactiveProperty<ActionBarSlotVM> m_PreviousSlotVM = new ReactiveProperty<ActionBarSlotVM>();

	private CompositeDisposable m_Disposable = new CompositeDisposable();

	private bool m_FindNextFirst = true;

	public ReadOnlyReactiveProperty<bool> IsActive => m_IsActive;

	public ReadOnlyReactiveProperty<bool> HasSlots => m_HasSlots;

	public IDisposable Initialize(ReactiveProperty<ActionBarSlotVM> mainSlotVM)
	{
		m_MainSlotView.Initialize();
		m_MainSlotView.SetVisible(visible: false);
		m_NextSlotView.Initialize();
		m_PreviousSlotView.Initialize();
		m_MainSlotVM = mainSlotVM;
		m_Disposable.Add(IsActive.Subscribe(OnActive));
		m_Disposable.Add(m_MainSlotVM.Subscribe(OnMainSlotVMChanged));
		m_Disposable.Add(m_NextSlotVM.Subscribe(m_NextSlotView.Bind));
		m_Disposable.Add(m_PreviousSlotVM.Subscribe(m_PreviousSlotView.Bind));
		return this;
	}

	private void OnActive(bool active)
	{
		m_ActiveButton.SetActiveLayer(active ? 1 : 0);
		if (!active)
		{
			m_MainSlotVM.Value = null;
		}
	}

	public void SetSlots(List<ActionBarSlotVM> slotVMs)
	{
		m_SlotVMs = slotVMs.Where((ActionBarSlotVM s) => !s.IsEmpty.CurrentValue && s.AbilityData != null).ToList();
		m_HasSlots.Value = !m_SlotVMs.Empty();
		m_FindNextFirst = true;
		m_MainSlotVM.Value = null;
		m_MainSlotVM.ForceNotify();
	}

	private void OnMainSlotVMChanged(ActionBarSlotVM slotVM)
	{
		m_MainSlotView.ViewModel?.OnHoverOff();
		if (slotVM != null)
		{
			List<ActionBarSlotVM> slotVMs = m_SlotVMs;
			if (slotVMs == null || !slotVMs.Contains(slotVM))
			{
				goto IL_0043;
			}
		}
		m_MainSlotView.Bind(slotVM);
		slotVM?.OnHoverOn();
		goto IL_0043;
		IL_0043:
		ActionBarSlotVM actionBarSlotVM;
		ActionBarSlotVM actionBarSlotVM2;
		if (m_FindNextFirst)
		{
			actionBarSlotVM = GetNearSlotVM(next: true);
			if (actionBarSlotVM == m_MainSlotVM.Value)
			{
				actionBarSlotVM = null;
			}
			actionBarSlotVM2 = GetNearSlotVM(next: false);
			if (actionBarSlotVM2 == m_MainSlotVM.Value || actionBarSlotVM2 == actionBarSlotVM)
			{
				actionBarSlotVM2 = null;
			}
		}
		else
		{
			actionBarSlotVM2 = GetNearSlotVM(next: false);
			if (actionBarSlotVM2 == m_MainSlotVM.Value)
			{
				actionBarSlotVM2 = null;
			}
			actionBarSlotVM = GetNearSlotVM(next: true);
			if (actionBarSlotVM == m_MainSlotVM.Value || actionBarSlotVM == actionBarSlotVM2)
			{
				actionBarSlotVM = null;
			}
		}
		m_NextSlotVM.Value = actionBarSlotVM;
		m_PreviousSlotVM.Value = actionBarSlotVM2;
	}

	private ActionBarSlotVM GetNearSlotVM(bool next)
	{
		if (m_SlotVMs.Empty())
		{
			return null;
		}
		int num = m_SlotVMs.IndexOf(m_MainSlotVM.Value);
		if (num == -1 && !next)
		{
			num = 0;
		}
		return m_SlotVMs[GetNearIndex(num, m_SlotVMs.Count, next)];
	}

	private static int GetNearIndex(int current, int count, bool next)
	{
		return (current + count + (next ? 1 : (-1))) % count;
	}

	public void ClickNext()
	{
		if (IsActive.CurrentValue)
		{
			m_FindNextFirst = false;
			m_MainSlotVM.Value = GetNearSlotVM(next: true);
			ButtonsSounds.Instance.Default.Hover.Play();
		}
	}

	public void ClickPrevious()
	{
		if (IsActive.CurrentValue)
		{
			m_FindNextFirst = true;
			m_MainSlotVM.Value = GetNearSlotVM(next: false);
			ButtonsSounds.Instance.Default.Hover.Play();
		}
	}

	public void Dispose()
	{
		m_Disposable.Clear();
	}

	public void HandleFocusState(bool shouldShowFocus)
	{
		m_ActiveButton.SetActiveLayer((shouldShowFocus && IsActive.CurrentValue) ? 1 : 0);
	}
}
