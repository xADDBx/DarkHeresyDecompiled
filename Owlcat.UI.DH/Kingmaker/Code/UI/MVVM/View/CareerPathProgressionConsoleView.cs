using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CareerPathProgressionConsoleView : CareerPathProgressionCommonView, ICharInfoCanHookConfirm
{
	[Header("Console")]
	[SerializeField]
	protected CharInfoSkillsAndWeaponsConsoleView m_SkillsAndWeaponsView;

	[SerializeField]
	private ConsoleHintsWidget m_InfoTooltipHintsWidget;

	[SerializeField]
	private ConsoleHint m_ExpandHint;

	private readonly ReactiveProperty<bool> m_CanReset = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanConfirm = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsInScreenNavigation = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanShowInfo = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsOnCareerItem = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsOnRightPanel = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanHideUnavailable = new ReactiveProperty<bool>();

	private readonly CompositeDisposable m_DelayedInvokes = new CompositeDisposable();

	private readonly CompositeDisposable m_InfoDisposables = new CompositeDisposable();

	private readonly CompositeDisposable m_Disposable = new CompositeDisposable();

	private bool m_InputAdded;

	private ConsoleHintDescription m_DeclineHint;

	private ConsoleHintDescription m_ConfirmHint;

	private ConsoleHintDescription m_Func01Hint;

	private ConsoleHintDescription m_OptionsHint;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private GridConsoleNavigationBehaviour m_ScreenNavigation;

	private GridConsoleNavigationBehaviour m_UnitPanelNavigation;

	private InputLayer m_InfoTooltipInputLayer;

	private IConsoleNavigationOwner m_NavigationOwner;

	private readonly Dictionary<ConsoleNavigationBehaviour, IConsoleEntity> m_PreviousEntities = new Dictionary<ConsoleNavigationBehaviour, IConsoleEntity>();

	private ConsoleNavigationBehaviour m_PreviousBehaviour;

	private bool m_HasTooltip;

	private bool m_ShowTooltip = true;

	private TooltipConfig m_TooltipConfig;

	private IConsoleEntity m_ContentEntity;

	private CareerPathSelectionTabsConsoleView CareerPathSelectionTabsConsoleView => m_CareerPathSelectionPartCommonView as CareerPathSelectionTabsConsoleView;

	private ConsoleNavigationBehaviour LeftNavigation => m_CareerPathRoundProgressionCommonView.GetNavigationBehaviour();

	private GridConsoleNavigationBehaviour RightNavigation => CareerPathSelectionTabsConsoleView.GetNavigationBehaviour();

	protected override void OnBind()
	{
		base.OnBind();
		m_TooltipConfig = new TooltipConfig
		{
			TooltipPlace = m_InfoSection.GetComponent<RectTransform>(),
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			}
		};
		m_IsShown.And(base.ViewModel.HasNewValidSelections).Subscribe(delegate(bool value)
		{
			m_CanReset.Value = value;
		}).AddTo(this);
		base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.Subscribe(delegate(IRankEntrySelectItem value)
		{
			if (value != null)
			{
				value.HasUnavailableFeatures.Subscribe(delegate(bool hasUnavailable)
				{
					m_CanHideUnavailable.Value = hasUnavailable;
				}).AddTo(this);
			}
			else
			{
				m_CanHideUnavailable.Value = false;
			}
			UpdateShowUnavailableHintLabel();
			if (value != null && value.CanSelect())
			{
				FocusOnRightPanel();
			}
			else if (base.ViewModel.CanCommit.CurrentValue && base.ViewModel.AllVisited.CurrentValue && value == null)
			{
				LeftNavigation.FocusOnEntityManual(LeftNavigation.Entities.First());
			}
		}).AddTo(this);
		base.ViewModel.UnitProgressionVM.OnRepeatedCurrentRankEntryItem.Subscribe(delegate
		{
			SwitchDescriptionShowed(true);
			FocusOnRightPanel();
		}).AddTo(this);
		base.ViewModel.IsDescriptionShowed.Subscribe(delegate(bool value)
		{
			ConsoleNavigationBehaviour previousBehaviour = m_PreviousBehaviour;
			IConsoleEntity value2 = null;
			if (previousBehaviour != null)
			{
				m_PreviousEntities.TryGetValue(m_PreviousBehaviour, out value2);
			}
			EventBus.RaiseEvent(delegate(ISetCharInfoUnitPanelState h)
			{
				h.SetUnitPanelNavigationState(!value);
			});
			if (previousBehaviour != null)
			{
				m_PreviousBehaviour = previousBehaviour;
				m_PreviousEntities[m_PreviousBehaviour] = value2;
				RefreshPreviousFocus();
			}
		}).AddTo(this);
		base.ViewModel.IsDescriptionShowed.Skip(1).Subscribe(delegate
		{
			EventBus.RaiseEvent(delegate(IUpdateFocusHandler h)
			{
				h.HandleFocus();
			});
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_InputAdded = false;
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour = null;
		m_DelayedInvokes?.Clear();
		m_InfoDisposables?.Clear();
		m_Disposable?.Clear();
		m_PreviousBehaviour = null;
		m_PreviousEntities.Clear();
	}

	private void CreateNavigation(IConsoleNavigationOwner owner)
	{
		m_NavigationOwner = owner;
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour(owner).AddTo(this);
		m_ScreenNavigation = new GridConsoleNavigationBehaviour().AddTo(this);
		m_PreviousEntities.Add(LeftNavigation, null);
		m_PreviousEntities.Add(RightNavigation, null);
		m_ScreenNavigation.AddColumn<ConsoleNavigationBehaviour>(LeftNavigation);
		m_ScreenNavigation.AddColumn<GridConsoleNavigationBehaviour>(RightNavigation);
		m_ScreenNavigation.FocusOnEntityManual(LeftNavigation);
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_ScreenNavigation);
		m_NavigationBehaviour.FocusOnEntityManual(m_ScreenNavigation);
		m_IsInScreenNavigation.Value = true;
		LeftNavigation.DeepestFocusAsObservable.Subscribe(OnFocusToLeftNavigation).AddTo(this);
		RightNavigation.DeepestFocusAsObservable.Subscribe(OnFocusToRightNavigation).AddTo(this);
		CreateInfoTooltipInputLayer();
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour(IConsoleNavigationOwner owner)
	{
		if (m_NavigationBehaviour == null)
		{
			CreateNavigation(owner);
		}
		return m_NavigationBehaviour;
	}

	private void FocusOnRightPanel()
	{
		m_DelayedInvokes.Add(DelayedInvoker.InvokeInFrames(delegate
		{
			m_ScreenNavigation.SetCurrentEntity(RightNavigation);
			m_NavigationBehaviour.SetCurrentEntity(m_ScreenNavigation);
			m_NavigationBehaviour.FocusOnEntityManual(m_ScreenNavigation);
			RightNavigation.FocusOnCurrentEntity();
			LeftNavigation.UnFocusCurrentEntity();
			m_NavigationBehaviour.UpdateDeepestFocusObserve();
			EventBus.RaiseEvent(delegate(IUpdateFocusHandler h)
			{
				h.HandleFocus();
			});
		}, 3));
	}

	private void FocusOnLeftPanel()
	{
		m_ScreenNavigation.SetCurrentEntity(LeftNavigation);
		m_NavigationBehaviour.SetCurrentEntity(m_ScreenNavigation);
		m_NavigationBehaviour.FocusOnEntityManual(m_ScreenNavigation);
		LeftNavigation.FocusOnCurrentEntity();
		RightNavigation.UnFocusCurrentEntity();
		m_NavigationBehaviour.UpdateDeepestFocusObserve();
		EventBus.RaiseEvent(delegate(IUpdateFocusHandler h)
		{
			h.HandleFocus();
		});
	}

	private void ToggleInfoNavigation()
	{
		TooltipHelper.HideTooltip();
		m_NavigationBehaviour.Clear();
		if (m_IsInScreenNavigation.Value)
		{
			GridConsoleNavigationBehaviour navigationBehaviour = m_InfoSection.GetNavigationBehaviour();
			m_InfoDisposables.Add(navigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusToInfo));
			m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(navigationBehaviour);
			m_NavigationBehaviour.FocusOnFirstValidEntity();
			m_DeclineHint?.SetLabel(UIStrings.Instance.CommonTexts.Back);
			m_ConfirmHint?.SetLabel(UIStrings.Instance.CommonTexts.Information);
			GamePad.Instance.PushLayer(m_InfoTooltipInputLayer).AddTo(this);
		}
		else
		{
			bool flag = m_PreviousBehaviour == RightNavigation;
			m_InfoDisposables.Clear();
			GamePad.Instance.PopLayer(m_InfoTooltipInputLayer);
			foreach (var (consoleNavigationBehaviour2, currentEntity) in m_PreviousEntities)
			{
				consoleNavigationBehaviour2.SetCurrentEntity(currentEntity);
			}
			m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_ScreenNavigation);
			m_NavigationBehaviour.SetCurrentEntity(m_ScreenNavigation);
			RefreshPreviousFocus();
			if (flag)
			{
				m_ScreenNavigation.SetCurrentEntity(RightNavigation);
				m_NavigationBehaviour.FocusOnEntityManual(m_ScreenNavigation);
			}
			else
			{
				RightNavigation.UnFocusCurrentEntity();
			}
		}
		m_IsInScreenNavigation.Value = !m_IsInScreenNavigation.Value;
		EventBus.RaiseEvent(delegate(IUpdateFocusHandler h)
		{
			h.HandleFocus();
		});
	}

	private void RefreshPreviousFocus()
	{
		if (m_PreviousBehaviour != null && m_PreviousEntities.TryGetValue(m_PreviousBehaviour, out var value))
		{
			m_PreviousBehaviour.SetCurrentEntity(m_PreviousEntities[m_PreviousBehaviour]);
			m_ScreenNavigation.SetCurrentEntity(m_PreviousBehaviour);
			m_NavigationBehaviour.FocusOnEntityManual(m_ScreenNavigation);
			m_PreviousBehaviour.FocusOnEntityManual(value);
		}
	}

	private void OnFocusToLeftNavigation(IConsoleEntity entity)
	{
		if (entity == null)
		{
			m_IsOnCareerItem.Value = false;
			return;
		}
		if (m_ScreenNavigation.CurrentEntity != LeftNavigation)
		{
			m_ScreenNavigation.SetCurrentEntity(LeftNavigation);
		}
		m_PreviousEntities[LeftNavigation] = entity;
		m_PreviousBehaviour = LeftNavigation;
		m_CanConfirm.Value = (entity as IConfirmClickHandler)?.CanConfirmClick() ?? false;
		m_DelayedInvokes.Add(DelayedInvoker.InvokeInFrames(delegate
		{
			m_NavigationOwner?.EntityFocused(entity);
		}, 1));
		m_IsOnCareerItem.Value = entity == LeftNavigation.Entities.First();
		string label = (m_IsOnCareerItem.Value ? UIStrings.Instance.CharacterSheet.BackToCareersList : UIStrings.Instance.CommonTexts.Back);
		m_DeclineHint?.SetLabel(label);
	}

	private void OnFocusToRightNavigation(IConsoleEntity entity)
	{
		m_IsOnRightPanel.Value = entity != null;
		if (entity == null)
		{
			if (!RightNavigation.IsFocused && m_IsInScreenNavigation.Value)
			{
				FocusOnLeftPanel();
			}
			return;
		}
		if (m_ScreenNavigation.CurrentEntity != RightNavigation)
		{
			m_ScreenNavigation.SetCurrentEntity(RightNavigation);
		}
		m_PreviousEntities[RightNavigation] = entity;
		m_PreviousBehaviour = RightNavigation;
		m_DeclineHint?.SetLabel(UIStrings.Instance.CommonTexts.Back);
		m_CanConfirm.Value = (entity as IConfirmClickHandler)?.CanConfirmClick() ?? false;
		if (entity is TMPLinkNavigationEntity)
		{
			m_CanConfirm.Value = false;
		}
		m_IsOnCareerItem.Value = false;
		m_DelayedInvokes.Add(DelayedInvoker.InvokeInFrames(delegate
		{
			m_NavigationOwner?.EntityFocused(entity);
		}, 1));
	}

	private void OnFocusToInfo(IConsoleEntity entity)
	{
		TooltipHelper.HideTooltip();
		TooltipBaseTemplate tooltipBaseTemplate = (entity as IHasTooltipTemplate)?.TooltipTemplate();
		m_HasTooltip = tooltipBaseTemplate != null;
		m_CanShowInfo.Value = m_HasTooltip && !m_ScreenNavigation.IsFocused;
		MonoBehaviour monoBehaviour = (entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour;
		if (m_ShowTooltip && (bool)monoBehaviour && tooltipBaseTemplate != null)
		{
			monoBehaviour.ShowConsoleTooltip(tooltipBaseTemplate, m_NavigationBehaviour, m_TooltipConfig);
		}
		if (entity is IPrerequisiteLinkEntity prerequisiteLinkEntity)
		{
			m_CanConfirm.Value = base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.CurrentValue.ContainsFeature(prerequisiteLinkEntity.LinkId);
		}
		else
		{
			m_CanConfirm.Value = false;
		}
	}

	public void AddInput(ref InputLayer inputLayer, ref ConsoleHintsWidget hintsWidget)
	{
		if (m_InputAdded)
		{
			return;
		}
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			OnDeclinePressed();
		}, 9);
		m_DeclineHint = (hintsWidget.BindHint(inputBindStruct) as ConsoleHintDescription).AddTo(this);
		inputBindStruct.AddTo(this);
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			base.ViewModel.Commit();
		}, 8, base.ViewModel.CanCommit.CombineLatest(base.ViewModel.AllVisited, m_IsOnRightPanel, (bool canCommit, bool allVisited, bool onRightPanel) => canCommit && allVisited && !onRightPanel).And(m_IsOnCareerItem).And(m_IsInScreenNavigation)
			.ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustLongPressed);
		hintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.CharGen.Complete).AddTo(this);
		inputBindStruct2.AddTo(this);
		inputLayer.AddButton(delegate
		{
			OnConfirmClick();
		}, 8, m_IsInScreenNavigation.And(m_CanConfirm).ToReadOnlyReactiveProperty(initialValue: false)).AddTo(this);
		inputLayer.AddButton(delegate
		{
			OnConfirmClick();
		}, 8, base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.Select((IRankEntrySelectItem item) => item is RankEntryFeatureItemVM).ToReadOnlyReactiveProperty(initialValue: false)).AddTo(this);
		CreateInformationBind(inputLayer, hintsWidget);
		InputBindStruct inputBindStruct3 = inputLayer.AddButton(delegate
		{
			ToggleTooltip();
		}, 8, m_CanShowInfo);
		hintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.CommonTexts.Information).AddTo(this);
		inputBindStruct3.AddTo(this);
		inputLayer.AddAxis(Scroll, 3, m_IsInScreenNavigation.And(base.ViewModel.IsDescriptionShowed).ToReadOnlyReactiveProperty(initialValue: false)).AddTo(this);
		CareerPathSelectionTabsConsoleView.AddInput(ref inputLayer, ref hintsWidget);
		if ((bool)m_ExpandHint && m_CanMove)
		{
			InputBindStruct inputBindStruct4 = inputLayer.AddButton(delegate
			{
				base.ViewModel.SwitchDescriptionShowed(true);
			}, 8, m_IsInScreenNavigation.CombineLatest(m_IsOnRightPanel, base.ViewModel.IsDescriptionShowed, (bool isOnScreen, bool onRightPanel, bool descShowed) => isOnScreen && onRightPanel && !descShowed).And(base.ViewModel.IsDescriptionShowed).ToReadOnlyReactiveProperty(initialValue: false));
			m_ExpandHint.Bind(inputBindStruct4).AddTo(this);
			inputBindStruct4.AddTo(this);
			m_ExpandHint.SetLabel(UIStrings.Instance.CommonTexts.Expand);
		}
		InputBindStruct inputBindStruct5 = inputLayer.AddButton(delegate
		{
			ToggleUnavailableFeatures();
		}, 16, m_CanHideUnavailable);
		m_OptionsHint = (hintsWidget.BindHint(inputBindStruct5) as ConsoleHintDescription).AddTo(this);
		inputBindStruct5.AddTo(this);
		if ((bool)m_SkillsAndWeaponsView)
		{
			CompositeDisposable item2 = m_SkillsAndWeaponsView.AddInput(inputLayer, hintsWidget, base.ViewModel.IsDescriptionShowed.Not().ToReadOnlyReactiveProperty(initialValue: false));
			m_Disposable.Add(item2);
		}
		InputBindStruct inputBindStruct6 = inputLayer.AddButton(delegate
		{
			SwitchDescriptionShowed(false);
		}, 11, base.ViewModel.IsDescriptionShowed);
		hintsWidget.BindHint(inputBindStruct6, UIStrings.Instance.CharacterSheet.ShowUnitPanel).AddTo(this);
		inputBindStruct6.AddTo(this);
		if (m_NavigationBehaviour != null)
		{
			OnFocusToLeftNavigation(m_NavigationBehaviour.DeepestNestedFocus);
		}
		m_InputAdded = true;
	}

	private void CreateInformationBind(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		if (m_CanMove)
		{
			InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
			{
				if (base.ViewModel.IsDescriptionShowed.CurrentValue)
				{
					ToggleInfoNavigation();
				}
				else
				{
					base.ViewModel.SwitchDescriptionShowed(true);
				}
			}, 10, m_IsInScreenNavigation);
			m_Func01Hint = (hintsWidget.BindHint(inputBindStruct) as ConsoleHintDescription).AddTo(this);
			inputBindStruct.AddTo(this);
			base.ViewModel.IsDescriptionShowed.Subscribe(delegate(bool value)
			{
				LocalizedString localizedString = (value ? UIStrings.Instance.CommonTexts.Information : UIStrings.Instance.CharacterSheet.ShowTooltip);
				m_Func01Hint.SetLabel(localizedString);
			}).AddTo(this);
		}
		else
		{
			InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
			{
				ToggleInfoNavigation();
			}, 10, m_IsInScreenNavigation);
			m_Func01Hint = (hintsWidget.BindHint(inputBindStruct2) as ConsoleHintDescription).AddTo(this);
			inputBindStruct2.AddTo(this);
			m_Func01Hint.SetLabel(UIStrings.Instance.CommonTexts.Information);
		}
	}

	private void CreateInfoTooltipInputLayer()
	{
		m_InfoTooltipInputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "InfoTooltip"
		});
		InputBindStruct inputBindStruct = m_InfoTooltipInputLayer.AddButton(delegate
		{
			OnDeclinePressed();
		}, 9);
		m_InfoTooltipHintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.Back).AddTo(this);
		inputBindStruct.AddTo(this);
		InputBindStruct inputBindStruct2 = m_InfoTooltipInputLayer.AddButton(delegate
		{
			if (m_PreviousBehaviour == RightNavigation)
			{
				m_PreviousEntities.Remove(RightNavigation);
			}
			m_DelayedInvokes.Add(DelayedInvoker.InvokeInFrames(ToggleInfoNavigation, 2));
		}, 8, m_CanConfirm);
		m_InfoTooltipHintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.Tooltips.ToCurrentPrerequisiteFeature).AddTo(this);
		inputBindStruct2.AddTo(this);
	}

	private void OnDeclinePressed()
	{
		if (!m_IsInScreenNavigation.Value)
		{
			ToggleInfoNavigation();
			EventBus.RaiseEvent(delegate(IUpdateFocusHandler h)
			{
				h.HandleFocus();
			});
			return;
		}
		if (m_ScreenNavigation.CurrentEntity == RightNavigation)
		{
			if (base.ViewModel.IsInLevelupProcess && base.ViewModel.UnitProgressionVM?.CurrentRankEntryItem.CurrentValue != null && base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.CurrentValue.CanSelect())
			{
				if (base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.CurrentValue == null)
				{
					FocusOnLeftPanel();
				}
				else
				{
					base.ViewModel.SelectPreviousItem();
					if (base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.CurrentValue == null)
					{
						FocusOnLeftPanel();
					}
				}
			}
			else
			{
				RightNavigation.UnFocusCurrentEntity();
				m_ScreenNavigation.FocusOnEntityManual(LeftNavigation);
				m_NavigationBehaviour.FocusOnEntityManual(m_ScreenNavigation);
				EventBus.RaiseEvent(delegate(IRankEntryFocusHandler h)
				{
					h.SetFocusOn(null);
				});
			}
		}
		else if (m_IsOnCareerItem.Value)
		{
			HandleReturn();
		}
		else
		{
			base.ViewModel.SetRankEntry(null);
			LeftNavigation.FocusOnEntityManual(LeftNavigation.Entities.First());
		}
		if (m_NavigationBehaviour != null)
		{
			OnFocusToLeftNavigation(m_NavigationBehaviour.DeepestNestedFocus);
		}
		EventBus.RaiseEvent(delegate(IUpdateFocusHandler h)
		{
			h.HandleFocus();
		});
	}

	private void OnConfirmClick()
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			if (m_ScreenNavigation.IsFocused && base.ViewModel.IsInLevelupProcess && base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.CurrentValue == null)
			{
				FocusOnLeftPanel();
			}
		}, 1);
	}

	private void ToggleTooltip()
	{
		TooltipHelper.HideTooltip();
		m_ShowTooltip = !m_ShowTooltip;
		OnFocusToInfo(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void Scroll(InputActionEventData obj, float value)
	{
		m_InfoSection.Scroll(value);
	}

	public ReadOnlyReactiveProperty<bool> GetCanHookConfirmProperty()
	{
		return m_CanConfirm.And(m_IsInScreenNavigation).And(m_IsOnCareerItem.And(base.ViewModel.CanCommit).And(base.ViewModel.AllVisited).Not()).ToReadOnlyReactiveProperty(initialValue: false);
	}

	private void ToggleUnavailableFeatures()
	{
		IRankEntrySelectItem rankEntrySelectItem = base.ViewModel.UnitProgressionVM?.CurrentRankEntryItem?.CurrentValue;
		if (rankEntrySelectItem != null)
		{
			bool isFocused = RightNavigation.IsFocused;
			rankEntrySelectItem.ToggleShowUnavailableFeatures();
			UpdateShowUnavailableHintLabel();
			if (isFocused)
			{
				RightNavigation.FocusOnFirstValidEntity();
				FocusOnRightPanel();
			}
		}
	}

	private void UpdateShowUnavailableHintLabel()
	{
		LocalizedString localizedString = (Game.Instance.Player.UISettings.ShowUnavailableFeatures ? UIStrings.Instance.CharacterSheet.HideUnavailableFeatures : UIStrings.Instance.CharacterSheet.ShowUnavailableFeatures);
		m_OptionsHint?.SetLabel(localizedString);
	}
}
