using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoFeaturesConsoleView : CharInfoFeaturesBaseView
{
	private Action<IConsoleEntity> m_RefreshParentFocus;

	private List<CharInfoFeatureGroupConsoleView> m_FeatureGroups = new List<CharInfoFeatureGroupConsoleView>();

	private ActionBarPartAbilitiesConsoleView m_ActionBarConsoleView;

	private readonly ReactiveProperty<bool> m_ActionBarActive = new ReactiveProperty<bool>();

	private ActionBarSlotAbilityConsoleView m_CurrentAbilitySlot;

	[Header("Console")]
	[SerializeField]
	private HintView m_ChangeTabHint;

	private CharInfoFeatureConsoleView m_MoveModeAbility;

	protected override void OnBind()
	{
		m_ActionBarConsoleView = m_ActionBarPartAbilitiesView as ActionBarPartAbilitiesConsoleView;
		base.OnBind();
		CreateActionBarManagement();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_CurrentAbilitySlot = null;
		m_MoveModeAbility = null;
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		Action<CharInfoFeatureConsoleView> onAbilityClick = (ActiveAbilitiesSelected.Value ? new Action<CharInfoFeatureConsoleView>(OnAbilityClick) : null);
		m_WidgetList.Entries.ForEach(delegate(IBindable e)
		{
			(e as CharInfoFeatureGroupConsoleView)?.SetupChooseModeActions(onAbilityClick, OnAbilityFocus);
		});
		m_ScrollRect.ScrollToTop();
	}

	private void CreateActionBarManagement()
	{
	}

	public void AddInput()
	{
	}

	private void OnAbilityClick(CharInfoFeatureConsoleView featureView)
	{
		Ability ability = featureView.ViewModel?.Ability;
		if (base.ViewModel.ChooseAbilityMode.CurrentValue)
		{
			EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
			{
				h.MoveSlot(ability, base.ViewModel.TargetSlotIndex);
			});
			base.ViewModel.SetChooseAbilityMode(chooseAbilityMode: false);
			return;
		}
		ActionBarSlotAbilityConsoleView targetSlot = m_ActionBarConsoleView.GetFirstEmptySlot();
		EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
		{
			h.MoveSlot(ability, targetSlot.Index);
		});
		base.ViewModel.ActionBarPartAbilitiesVM.SetMoveAbilityMode(on: true);
		m_MoveModeAbility = featureView;
		featureView.SetMoveState(state: true);
	}

	private void OnAbilityFocus(CharInfoFeatureConsoleView featureView)
	{
		ActionBarSlotVM actionBarSlotVM = base.ViewModel.ActionBarPartAbilitiesVM.Slots.ElementAtOrDefault(base.ViewModel.TargetSlotIndex);
		Ability ability = ((!(featureView != null)) ? null : featureView.GetViewModel()?.Ability);
		if (ability != null || actionBarSlotVM == null || actionBarSlotVM.IsEmpty.CurrentValue)
		{
			actionBarSlotVM?.OverrideIcon(ability?.Icon);
		}
	}

	private void OnFocusChanged(IConsoleEntity focus)
	{
		if (focus != null)
		{
			RectTransform targetRect = ((focus as MonoBehaviour) ?? (focus as IMonoBehaviour)?.MonoBehaviour)?.transform as RectTransform;
			m_ScrollRect.EnsureVisibleVertical(targetRect);
		}
	}

	private void OnActionBarFocused(IConsoleEntity focus)
	{
		m_ActionBarActive.Value = focus != null;
		m_CurrentAbilitySlot = focus as ActionBarSlotAbilityConsoleView;
	}

	private void ShowContextMenu()
	{
		m_CurrentAbilitySlot.ShowContextMenu(m_CurrentAbilitySlot.ContextMenuEntities);
	}

	private void ToggleAbilitiesTab()
	{
		SetActiveAbilitiesState(!ActiveAbilitiesSelected.Value);
	}
}
