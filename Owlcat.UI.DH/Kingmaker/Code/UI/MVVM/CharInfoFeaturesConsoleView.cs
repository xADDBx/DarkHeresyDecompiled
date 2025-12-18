using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoFeaturesConsoleView : CharInfoFeaturesBaseView, ICharInfoComponentConsoleView, ICharInfoComponentView, ICharInfoIgnoreNavigationConsoleView
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private GridConsoleNavigationBehaviour m_ActionBarNavigation;

	private GridConsoleNavigationBehaviour m_ScrollRectNavigation;

	private Action<IConsoleEntity> m_RefreshParentFocus;

	private List<CharInfoFeatureGroupConsoleView> m_FeatureGroups = new List<CharInfoFeatureGroupConsoleView>();

	private ActionBarPartAbilitiesConsoleView m_ActionBarConsoleView;

	private readonly ReactiveProperty<bool> m_ActionBarActive = new ReactiveProperty<bool>();

	private ActionBarSlotAbilityConsoleView m_CurrentAbilitySlot;

	[Header("Console")]
	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private ConsoleHint m_ChangeTabHint;

	private InputLayer m_ChooseAbilityLayer;

	private CharInfoFeatureConsoleView m_MoveModeAbility;

	protected override void OnBind()
	{
		m_ActionBarConsoleView = m_ActionBarPartAbilitiesView as ActionBarPartAbilitiesConsoleView;
		CreateNavigation();
		base.OnBind();
		m_ActionBarNavigation = m_ActionBarConsoleView.Or(null)?.NavigationBehaviour.AddTo(this);
		m_ActionBarNavigation?.DeepestFocusAsObservable.Subscribe(OnActionBarFocused).AddTo(this);
		m_NavigationBehaviour.AddRow<GridConsoleNavigationBehaviour>(m_ActionBarNavigation);
		RefreshFocus();
		CreateActionBarManagement();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_CurrentAbilitySlot = null;
		m_MoveModeAbility = null;
		m_NavigationBehaviour.RemoveEntity(m_ActionBarNavigation);
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		Action<CharInfoFeatureConsoleView> onAbilityClick = (ActiveAbilitiesSelected.Value ? new Action<CharInfoFeatureConsoleView>(OnAbilityClick) : null);
		m_WidgetList.Entries.ForEach(delegate(IBindable e)
		{
			(e as CharInfoFeatureGroupConsoleView)?.SetupChooseModeActions(onAbilityClick, OnAbilityFocus);
		});
		UpdateNavigation();
		m_ScrollRect.ScrollToTop();
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		m_ScrollRectNavigation = new GridConsoleNavigationBehaviour().AddTo(this);
		m_ScrollRectNavigation.DeepestFocusAsObservable.Subscribe(OnFocusChanged).AddTo(this);
		m_NavigationBehaviour.AddRow<GridConsoleNavigationBehaviour>(m_ScrollRectNavigation);
		m_NavigationBehaviour.FocusOnFirstValidEntity();
	}

	private void CreateActionBarManagement()
	{
		m_ChooseAbilityLayer = m_ScrollRectNavigation.GetInputLayer(new InputLayer
		{
			ContextName = "ChooseAbility"
		});
		ReadOnlyReactiveProperty<bool> readOnlyReactiveProperty = OwlcatR3BooleanExtensions.Or(base.ViewModel.ChooseAbilityMode, base.ViewModel.ActionBarPartAbilitiesVM.MoveAbilityMode).ToReadOnlyReactiveProperty(initialValue: false);
		m_ConsoleHintsWidget.BindHint(m_ChooseAbilityLayer.AddButton(delegate
		{
			base.ViewModel.SetChooseAbilityMode(chooseAbilityMode: false);
		}, 9, readOnlyReactiveProperty), UIStrings.Instance.CommonTexts.Cancel).AddTo(this);
		m_ConsoleHintsWidget.BindHint(m_ChooseAbilityLayer.AddButton(delegate
		{
		}, 8, base.ViewModel.ChooseAbilityMode), UIStrings.Instance.CommonTexts.Select).AddTo(this);
		base.ViewModel.ChooseAbilityMode.Subscribe(delegate(bool on)
		{
			if (on)
			{
				GamePad.Instance.PushLayer(m_ChooseAbilityLayer).AddTo(this);
				m_NavigationBehaviour.UnFocusCurrentEntity();
				m_ScrollRectNavigation.FocusOnFirstValidEntity();
				ObservableSubscribeExtensions.Subscribe(Observable.TimerFrame(0, UnityFrameProvider.PostLateUpdate), delegate
				{
					OnAbilityFocus(m_ScrollRectNavigation.DeepestNestedFocus as CharInfoFeatureConsoleView);
				}).AddTo(this);
				EventBus.RaiseEvent(delegate(ICharInfoAbilitiesChooseModeHandler h)
				{
					h.HandleChooseMode(active: true);
				});
			}
			else
			{
				OnAbilityFocus(null);
				GamePad.Instance.PopLayer(m_ChooseAbilityLayer);
				m_ScrollRectNavigation.UnFocusCurrentEntity();
				m_NavigationBehaviour.FocusOnCurrentEntity();
				EventBus.RaiseEvent(delegate(ICharInfoAbilitiesChooseModeHandler h)
				{
					h.HandleChooseMode(active: false);
				});
				EventBus.RaiseEvent(delegate(IUpdateFocusHandler h)
				{
					h.HandleFocus();
				});
			}
		}).AddTo(this);
		base.ViewModel.ActionBarPartAbilitiesVM.MoveAbilityMode.Subscribe(delegate(bool on)
		{
			if (!on)
			{
				m_ActionBarNavigation.UnFocusCurrentEntity();
				m_NavigationBehaviour.FocusOnCurrentEntity();
				EventBus.RaiseEvent(delegate(IUpdateFocusHandler h)
				{
					h.HandleFocus();
				});
				if ((bool)m_MoveModeAbility)
				{
					m_MoveModeAbility.SetMoveState(state: false);
					m_MoveModeAbility = null;
				}
			}
		}).AddTo(this);
	}

	private void UpdateNavigation()
	{
		m_ScrollRectNavigation.Clear();
		if (m_WidgetList.Entries == null)
		{
			return;
		}
		foreach (IBindable entry in m_WidgetList.Entries)
		{
			if (entry is CharInfoFeatureGroupConsoleView charInfoFeatureGroupConsoleView)
			{
				m_FeatureGroups.Add(charInfoFeatureGroupConsoleView);
				m_ScrollRectNavigation.AddRow<GridConsoleNavigationBehaviour>(charInfoFeatureGroupConsoleView.GetNavigation());
			}
		}
	}

	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		navigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationBehaviour);
		InputBindStruct inputBindStruct = inputLayer.AddButton(ShowContextMenu, 10, m_ActionBarActive);
		hintsWidget.BindHint(inputBindStruct, UIStrings.Instance.ContextMenu.ContextMenu).AddTo(this);
		inputBindStruct.AddTo(this);
		m_ActionBarConsoleView.AddInputToPages(inputLayer, m_ActionBarActive);
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			ToggleAbilitiesTab();
		}, 18);
		m_ChangeTabHint.Bind(inputBindStruct2).AddTo(this);
		inputBindStruct2.AddTo(this);
	}

	public List<GridConsoleNavigationBehaviour> GetIgnoreNavigation()
	{
		return new List<GridConsoleNavigationBehaviour> { m_ActionBarNavigation };
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
		m_NavigationBehaviour.UnFocusCurrentEntity();
		EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
		{
			h.MoveSlot(ability, targetSlot.Index);
		});
		m_ActionBarNavigation.FocusOnEntityManual(targetSlot);
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

	private void ShowContextMenu(InputActionEventData data)
	{
		m_CurrentAbilitySlot.ShowContextMenu(m_CurrentAbilitySlot.ContextMenuEntities);
	}

	private void ToggleAbilitiesTab()
	{
		SetActiveAbilitiesState(!ActiveAbilitiesSelected.Value);
		RefreshFocus();
	}

	private void RefreshFocus()
	{
		DelayedInvoker.InvokeAtTheEndOfFrameOnlyOnes(delegate
		{
			m_NavigationBehaviour.FocusOnFirstValidEntity();
			foreach (CharInfoFeatureGroupConsoleView featureGroup in m_FeatureGroups)
			{
				CharInfoFeatureGroupVM.FeatureGroupType groupType = featureGroup.GroupType;
				if (groupType != 0 && (groupType != CharInfoFeatureGroupVM.FeatureGroupType.Abilities || ActiveAbilitiesSelected.Value))
				{
					IConsoleEntity firstFeature = featureGroup.GetFirstFeature();
					if (firstFeature != null)
					{
						m_NavigationBehaviour.FocusOnEntityManual(firstFeature);
						break;
					}
				}
			}
		});
		m_ScrollRect.ScrollToTop();
	}
}
