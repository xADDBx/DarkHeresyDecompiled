using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Events;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ObsoleteCharacterInfoConsoleView : ObsoleteCharacterInfoPCView, IUpdateFocusHandler, ISubscriber, ICullFocusHandler, ISetCharInfoUnitPanelState
{
	[Header("Console")]
	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private RectTransform m_TooltipPlace;

	[SerializeField]
	private RectTransform m_TooltipPlaceForLeftPanel;

	[SerializeField]
	private CanvasSortingComponent m_CanvasSortingComponent;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private GridConsoleNavigationBehaviour m_NavigationPanelLeft;

	private GridConsoleNavigationBehaviour m_NavigationPanelRight;

	private InputLayer m_InputLayer;

	private readonly ReactiveProperty<bool> m_LeftPanelSelected = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_RightPanelSelected = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ShowTooltip = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasTooltip = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanConfirm = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanDecline = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_InUnitProgression = new ReactiveProperty<bool>();

	private readonly Dictionary<CharInfoComponentType, ICharInfoComponentConsoleView> m_ComponentConsoleViews = new Dictionary<CharInfoComponentType, ICharInfoComponentConsoleView>();

	private readonly List<GridConsoleNavigationBehaviour> m_IgnoreRightPanels = new List<GridConsoleNavigationBehaviour>();

	private IDisposable m_CanHookDeclineSubscription;

	private readonly List<CharInfoComponentType> m_LeftPanelViews = new List<CharInfoComponentType>
	{
		CharInfoComponentType.LevelClassScores,
		CharInfoComponentType.SkillsAndWeapons
	};

	private IConsoleHint m_ConfirmHint;

	private TooltipConfig m_RightPanelConfig;

	private TooltipConfig m_LeftPanelConfig;

	private IConsoleEntity m_CulledFocus;

	private ConsoleNavigationBehaviour m_PrevNavigation;

	public override void Initialize()
	{
		base.Initialize();
		foreach (var (key, charInfoComponentView2) in ComponentViews)
		{
			if (charInfoComponentView2 is ICharInfoComponentConsoleView value)
			{
				m_ComponentConsoleViews.Add(key, value);
			}
		}
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_RightPanelConfig = new TooltipConfig
		{
			TooltipPlace = m_TooltipPlace,
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			}
		};
		m_LeftPanelConfig = new TooltipConfig
		{
			TooltipPlace = m_TooltipPlaceForLeftPanel,
			PriorityPivots = new List<Vector2>
			{
				new Vector2(0f, 0.5f)
			}
		};
		CreateNavigation();
		base.ViewModel.PagesSelectionGroupRadioVM.SelectedEntity.Subscribe(delegate
		{
			RefreshInput();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.BiographyUpdated, delegate
		{
			RefreshInput();
		}).AddTo(this);
		m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusEntity).AddTo(this);
		(m_SkillsAndWeaponsView as CharInfoSkillsAndWeaponsConsoleView)?.SetFocusChangeAction(OnFocusEntity);
		EventBus.Subscribe(this).AddTo(this);
		TooltipHelper.HideTooltip();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_CanHookDeclineSubscription?.Dispose();
		m_CanHookDeclineSubscription = null;
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		m_NavigationPanelLeft = new GridConsoleNavigationBehaviour().AddTo(this);
		m_NavigationPanelRight = new GridConsoleNavigationBehaviour().AddTo(this);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "CharScreenView"
		});
		m_NavigationPanelLeft.Focus.Subscribe(delegate
		{
			ToggleLeftPanelFocus();
		}).AddTo(this);
		m_NavigationPanelRight.DeepestFocusAsObservable.Subscribe(ToggleRightCanvasFocus).AddTo(this);
		m_NavigationBehaviour.OnClickAsObservable().Subscribe(OnClick).AddTo(this);
		CreateInput();
	}

	private void CreateInput()
	{
		m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			Close();
		}, 9, m_CanDecline), UIStrings.Instance.CommonTexts.CloseWindow).AddTo(this);
		ReadOnlyReactiveProperty<bool> enabledHints = m_RightPanelSelected.And(m_InUnitProgression.Not()).ToReadOnlyReactiveProperty(initialValue: false);
		(m_CharInfoPagesMenu as CharInfoPagesMenuConsoleView)?.AddHints(m_InputLayer, enabledHints);
		if (m_NameAndPortraitView is CharInfoNameAndPortraitConsoleView charInfoNameAndPortraitConsoleView)
		{
			charInfoNameAndPortraitConsoleView.AddInput(m_InputLayer, m_ConsoleHintsWidget, m_LeftPanelSelected);
		}
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
		{
			ToggleTooltip();
		}, 19, m_HasTooltip, InputActionEventType.ButtonJustReleased);
		m_ConsoleHintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.Information).AddTo(this);
		inputBindStruct.AddTo(this);
		InputBindStruct inputBindStruct2 = m_InputLayer.AddButton(delegate
		{
		}, 8, m_CanConfirm);
		m_ConfirmHint = m_ConsoleHintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.CommonTexts.Select);
		m_ConfirmHint.AddTo(this);
		inputBindStruct2.AddTo(this);
		(m_SkillsAndWeaponsView as CharInfoSkillsAndWeaponsConsoleView)?.AddInput(m_InputLayer, m_ConsoleHintsWidget, m_InUnitProgression.Not().ToReadOnlyReactiveProperty(initialValue: false));
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
		m_CanvasSortingComponent.PushView().AddTo(this);
	}

	private void RefreshInput(bool addLeftPanel = true)
	{
		m_NavigationPanelLeft.Clear();
		m_NavigationBehaviour.Clear();
		m_NavigationPanelRight.Clear();
		m_IgnoreRightPanels.Clear();
		m_CanHookDeclineSubscription?.Dispose();
		m_CanHookDeclineSubscription = null;
		foreach (var (item, charInfoComponentConsoleView2) in m_ComponentConsoleViews)
		{
			if (!charInfoComponentConsoleView2.IsBinded)
			{
				continue;
			}
			if (m_LeftPanelViews.Contains(item))
			{
				if (addLeftPanel)
				{
					charInfoComponentConsoleView2.AddInput(ref m_InputLayer, ref m_NavigationPanelLeft, m_ConsoleHintsWidget);
				}
			}
			else
			{
				charInfoComponentConsoleView2.AddInput(ref m_InputLayer, ref m_NavigationPanelRight, m_ConsoleHintsWidget);
			}
			if (charInfoComponentConsoleView2 is ICharInfoIgnoreNavigationConsoleView charInfoIgnoreNavigationConsoleView)
			{
				m_IgnoreRightPanels.AddRange(charInfoIgnoreNavigationConsoleView.GetIgnoreNavigation());
			}
			if (charInfoComponentConsoleView2 is ICharInfoCanHookDecline charInfoCanHookDecline)
			{
				m_CanHookDeclineSubscription = charInfoCanHookDecline.GetCanHookDeclineProperty().Subscribe(delegate(bool value)
				{
					m_CanDecline.Value = !value;
				});
			}
			else
			{
				m_CanDecline.Value = true;
			}
		}
		m_NavigationPanelLeft.FocusOnEntityManual(m_NavigationPanelLeft.Entities.LastOrDefault());
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationPanelLeft);
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationPanelRight);
		m_NavigationBehaviour.FocusOnEntityManual(m_NavigationPanelRight);
		RefreshUnitProgressionState();
	}

	public void SetUnitPanelNavigationState(bool state)
	{
		RefreshInput(state);
	}

	private void RefreshUnitProgressionState()
	{
		m_InUnitProgression.Value = m_ProgressionView.IsBinded && m_ProgressionView.CurrentState == UnitProgressionWindowState.CareerPathProgression;
	}

	protected override void OnProgressionWindowStateChange(UnitProgressionWindowState state)
	{
		RefreshUnitProgressionState();
		m_ShowTooltip.Value = m_ShowTooltip.Value && state != UnitProgressionWindowState.CareerPathProgression;
	}

	private void ToggleTooltip()
	{
		TooltipHelper.HideTooltip();
		m_ShowTooltip.Value = !m_ShowTooltip.Value;
		OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		TooltipHelper.HideTooltip();
		m_CanConfirm.Value = (entity as IConfirmClickHandler)?.CanConfirmClick() ?? false;
		if (m_InUnitProgression.Value && m_ProgressionView is ICharInfoCanHookConfirm charInfoCanHookConfirm)
		{
			m_CanConfirm.Value = charInfoCanHookConfirm.GetCanHookConfirmProperty().CurrentValue;
		}
		string text = entity?.GetConfirmClickHint();
		m_ConfirmHint.SetLabel((!string.IsNullOrEmpty(text)) ? text : ((string)UIStrings.Instance.CommonTexts.Select));
		TooltipConfig config = ((m_RightPanelSelected.Value || m_IgnoreRightPanels.All((GridConsoleNavigationBehaviour p) => p.IsFocused)) ? m_RightPanelConfig : m_LeftPanelConfig);
		if (!(entity is IHasTooltipTemplate hasTooltipTemplate))
		{
			m_HasTooltip.Value = false;
			return;
		}
		TooltipBaseTemplate tooltipBaseTemplate = hasTooltipTemplate.TooltipTemplate();
		if (tooltipBaseTemplate is TooltipTemplateGlossary { GlossaryEntry: not null })
		{
			config.IsGlossary = true;
		}
		m_HasTooltip.Value = tooltipBaseTemplate != null && (m_LeftPanelSelected.Value || !m_InUnitProgression.Value);
		if (m_ShowTooltip.Value)
		{
			((entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour).ShowConsoleTooltip(tooltipBaseTemplate, m_NavigationBehaviour, config);
		}
	}

	public void OnClick()
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
		}, 3);
	}

	private void ToggleLeftPanelFocus()
	{
		m_LeftPanelSelected.Value = m_NavigationPanelLeft.IsFocused;
	}

	private void ToggleRightCanvasFocus(IConsoleEntity entity)
	{
		if (entity == null && !base.ViewModel.PageCanHaveNoEntities)
		{
			m_RightPanelSelected.Value = false;
			return;
		}
		m_RightPanelSelected.Value = !m_IgnoreRightPanels.SelectMany((GridConsoleNavigationBehaviour nb) => nb.NestedFocuses).Contains(entity);
	}

	private void Close()
	{
		TooltipHelper.HideTooltip();
		if (m_HasTooltip.Value && m_ShowTooltip.Value)
		{
			m_ShowTooltip.Value = false;
			return;
		}
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleCloseAll();
		});
	}

	public void HandleFocus()
	{
		OnClick();
	}

	public void HandleRemoveFocus()
	{
		m_PrevNavigation = (m_LeftPanelSelected.Value ? m_NavigationPanelLeft : m_NavigationPanelRight);
		m_CulledFocus = m_NavigationBehaviour.DeepestNestedFocus;
		m_NavigationBehaviour.UnFocusCurrentEntity();
	}

	public void HandleRestoreFocus()
	{
		if (m_CulledFocus != null)
		{
			m_PrevNavigation.FocusOnEntityManual(m_CulledFocus);
			m_NavigationBehaviour.FocusOnEntityManual(m_PrevNavigation);
			m_NavigationBehaviour.UpdateDeepestFocusObserve();
		}
		m_CulledFocus = null;
	}
}
