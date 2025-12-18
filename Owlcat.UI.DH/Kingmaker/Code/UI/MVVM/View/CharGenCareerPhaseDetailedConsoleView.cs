using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenCareerPhaseDetailedConsoleView : CharGenCareerPhaseDetailedView, IUpdateFocusHandler, ISubscriber
{
	[SerializeField]
	private ConsoleHint m_SelectHint;

	private readonly ReactiveProperty<ActivePhaseNavigation> m_ActivePhaseNavigation = new ReactiveProperty<ActivePhaseNavigation>(ActivePhaseNavigation.Content);

	private readonly ReactiveProperty<bool> m_CanConfirm = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanDecline = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanFunc02 = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanShowInfo = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsInContent = new ReactiveProperty<bool>();

	private IConsoleEntity m_ContentEntity;

	private IDisposable m_DelayedNestedFocusUpdate;

	private bool m_HasTooltip;

	private GridConsoleNavigationBehaviour m_Navigation;

	private TooltipConfig m_TooltipConfig;

	private GridConsoleNavigationBehaviour m_UnitProgressionNavigation;

	private IUpdateFocusHandler m_UpdateFocusHandlerImplementation;

	private UnitProgressionConsoleView UnitProgressionConsoleView => m_UnitProgressionView as UnitProgressionConsoleView;

	public void HandleFocus()
	{
		ObservableSubscribeExtensions.Subscribe(Observable.TimerFrame(3), delegate
		{
			OnFocusEntity(m_Navigation.DeepestNestedFocus);
		}).AddTo(this);
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_TooltipConfig = new TooltipConfig
		{
			TooltipPlace = m_InfoView.GetComponent<RectTransform>(),
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			}
		};
		m_UnitProgressionNavigation = new GridConsoleNavigationBehaviour().AddTo(this);
		m_CanDecline.Subscribe(delegate(bool value)
		{
			CanGoBackOnDecline.Value = !value;
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		TooltipHelper.HideTooltip();
		m_DelayedNestedFocusUpdate?.Dispose();
		m_DelayedNestedFocusUpdate = null;
	}

	public override void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, ReadOnlyReactiveProperty<bool> isMainCharacter)
	{
		m_Navigation = navigationBehaviour;
		m_ActivePhaseNavigation.Subscribe(UpdateActiveNavigation).AddTo(this);
		m_Navigation.DeepestFocusAsObservable.Subscribe(OnFocusEntity).AddTo(this);
		UnitProgressionConsoleView.NavigationBehaviour.DeepestFocusAsObservable.Subscribe().AddTo(this);
		inputLayer.AddAxis(Scroll, 3).AddTo(this);
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			RefreshFocus();
		}, 8, m_CanConfirm.And(isMainCharacter).ToReadOnlyReactiveProperty(initialValue: false));
		m_SelectHint.Bind(inputBindStruct).AddTo(this);
		inputBindStruct.AddTo(this);
		m_SelectHint.SetLabel(UIStrings.Instance.CommonTexts.Select);
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			SwitchNavigation();
		}, 10, m_IsInContent.CombineLatest(base.ViewModel.UnitProgressionVM.State, (bool content, UnitProgressionWindowState state) => content && state == UnitProgressionWindowState.CareerPathList).ToReadOnlyReactiveProperty(initialValue: false));
		hintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.CommonTexts.Information).AddTo(this);
		inputBindStruct2.AddTo(this);
		InputBindStruct inputBindStruct3 = inputLayer.AddButton(delegate
		{
			RefreshFocus();
		}, 11, m_CanFunc02);
		hintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.CharGen.InspectCareer).AddTo(this);
		inputBindStruct3.AddTo(this);
		InputBindStruct inputBindStruct4 = inputLayer.AddButton(delegate
		{
			OnDeclineClick();
		}, 9, m_CanDecline);
		hintsWidget.BindHint(inputBindStruct4, UIStrings.Instance.CommonTexts.Back).AddTo(this);
		inputBindStruct4.AddTo(this);
		InputBindStruct inputBindStruct5 = inputLayer.AddButton(delegate
		{
			ToggleTooltip();
		}, 8, m_CanShowInfo);
		hintsWidget.BindHint(inputBindStruct5, UIStrings.Instance.CommonTexts.Information).AddTo(this);
		inputBindStruct5.AddTo(this);
		UnitProgressionConsoleView.AddInput(ref inputLayer, ref m_UnitProgressionNavigation, hintsWidget);
		base.ViewModel.UnitProgressionVM.PreselectedCareer.Subscribe(delegate
		{
			OnFocusEntity(UnitProgressionConsoleView.NavigationBehaviour.DeepestNestedFocus);
		}).AddTo(this);
		m_Navigation.FocusOnFirstValidEntity();
	}

	private void SwitchNavigation()
	{
		switch (m_ActivePhaseNavigation.Value)
		{
		case ActivePhaseNavigation.Content:
			m_ActivePhaseNavigation.Value = ActivePhaseNavigation.SecondaryInfo;
			break;
		case ActivePhaseNavigation.SecondaryInfo:
			m_ActivePhaseNavigation.Value = ActivePhaseNavigation.Content;
			break;
		}
	}

	private void UpdateActiveNavigation(ActivePhaseNavigation activeNavigation)
	{
		TooltipHelper.HideTooltip();
		switch (activeNavigation)
		{
		case ActivePhaseNavigation.Content:
			SetContentNavigation();
			break;
		case ActivePhaseNavigation.SecondaryInfo:
			SetSecondaryInfoNavigation();
			break;
		}
		m_IsInContent.Value = m_ActivePhaseNavigation.Value == ActivePhaseNavigation.Content;
		m_CanDecline.Value = activeNavigation == ActivePhaseNavigation.SecondaryInfo;
	}

	private void SetContentNavigation()
	{
		IConsoleEntity contentEntity = m_ContentEntity;
		m_Navigation.Clear();
		m_Navigation.AddColumn<GridConsoleNavigationBehaviour>(m_UnitProgressionNavigation);
		if (contentEntity != null)
		{
			m_UnitProgressionNavigation.FocusOnEntityManual(contentEntity);
			m_Navigation.FocusOnEntityManual(m_UnitProgressionNavigation);
		}
		else
		{
			m_Navigation.FocusOnFirstValidEntity();
		}
	}

	private void SetSecondaryInfoNavigation()
	{
		m_UnitProgressionNavigation.FocusOnEntityManual(m_Navigation.DeepestNestedFocus);
		m_Navigation.Clear();
		m_Navigation.AddColumn<GridConsoleNavigationBehaviour>(m_InfoView.GetNavigationBehaviour());
		m_Navigation.FocusOnFirstValidEntity();
		OnFocusEntity(m_Navigation.DeepestNestedFocus);
	}

	private void RefreshFocus()
	{
		OnFocusEntity(m_Navigation.DeepestNestedFocus);
	}

	private void Scroll(InputActionEventData data, float y)
	{
		if (!(Mathf.Abs(data.player.GetAxis(3)) < Mathf.Abs(data.player.GetAxis(2))) && (!(y > 0f) || !m_InfoView.ScrollbarOnTop) && (!(y < 0f) || !m_InfoView.ScrollbarOnBottom))
		{
			m_InfoView.Scroll(y);
		}
	}

	private void OnDeclineClick()
	{
		SwitchNavigation();
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		bool flag = base.ViewModel.UnitProgressionVM.State.CurrentValue == UnitProgressionWindowState.CareerPathList;
		bool flag2 = flag && base.ViewModel.UnitProgressionVM.AllCareerPaths.Any((CareerPathVM path) => path.IsSelected.Value);
		bool flag3 = IsSelectedOrHighTier(entity);
		flag2 = flag2 && flag3;
		m_CanConfirm.Value = m_ActivePhaseNavigation.Value == ActivePhaseNavigation.Content && !flag2 && !(flag && flag3) && ((entity as IConfirmClickHandler)?.CanConfirmClick() ?? false);
		if (entity is TMPLinkNavigationEntity)
		{
			m_CanConfirm.Value = false;
		}
		m_CanFunc02.Value = (entity as IFunc02ClickHandler)?.CanFunc02Click() ?? false;
		CanGoNextOnConfirm.Value = flag2 && m_ActivePhaseNavigation.Value == ActivePhaseNavigation.Content;
		CanGoBackOnDecline.Value = flag && m_ActivePhaseNavigation.Value == ActivePhaseNavigation.Content;
		TooltipBaseTemplate tooltipBaseTemplate = (entity as IHasTooltipTemplate)?.TooltipTemplate();
		m_HasTooltip = tooltipBaseTemplate != null;
		m_CanShowInfo.Value = m_HasTooltip && m_ActivePhaseNavigation.Value == ActivePhaseNavigation.SecondaryInfo;
		if (flag)
		{
			TooltipHelper.HideTooltip();
			if (m_ActivePhaseNavigation.Value == ActivePhaseNavigation.Content)
			{
				base.ViewModel.InfoVM.SetTemplate(tooltipBaseTemplate);
				m_ContentEntity = entity;
			}
			else if (CharGenConsoleView.ShowTooltip && m_ActivePhaseNavigation.Value == ActivePhaseNavigation.SecondaryInfo)
			{
				MonoBehaviour monoBehaviour = (entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour;
				if ((bool)monoBehaviour)
				{
					monoBehaviour.ShowConsoleTooltip(tooltipBaseTemplate, m_Navigation, m_TooltipConfig);
				}
			}
		}
		else if (base.ViewModel.InfoVM.CurrentTooltip != null)
		{
			base.ViewModel.InfoVM.SetTemplate(null);
		}
		string confirmClickHint = entity.GetConfirmClickHint();
		m_SelectHint.SetLabel((!string.IsNullOrEmpty(confirmClickHint)) ? confirmClickHint : ((string)UIStrings.Instance.CommonTexts.Select));
	}

	private void ToggleTooltip()
	{
		CharGenConsoleView.ShowTooltip = !CharGenConsoleView.ShowTooltip;
		OnFocusEntity(m_Navigation.DeepestNestedFocus);
	}

	private bool IsEntitySelected(IConsoleEntity entity)
	{
		if (!(entity is CareerPathListItemCommonView careerPathListItemCommonView))
		{
			return false;
		}
		return careerPathListItemCommonView.ViewModel.IsSelected.Value;
	}

	private bool IsSelectedOrHighTier(IConsoleEntity entity)
	{
		if (!(entity is CareerPathListItemCommonView { ViewModel: var viewModel }))
		{
			return false;
		}
		if (viewModel == null || viewModel.CareerPath.Tier != 0 || !viewModel.IsSelected.Value)
		{
			if (viewModel == null)
			{
				return true;
			}
			return viewModel.CareerPath.Tier != CareerPathTier.One;
		}
		return true;
	}
}
