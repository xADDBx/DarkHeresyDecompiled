using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenBackgroundBasePhaseDetailedView : CharGenPhaseDetailedView<CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>>
{
	[Header("Description")]
	[SerializeField]
	private InfoSectionView m_InfoView;

	[SerializeField]
	private GameObject m_SecondaryInfoViewContainer;

	[SerializeField]
	private InfoSectionView m_SecondaryInfoView;

	[Header("Selector")]
	[SerializeField]
	private CharGenBackgroundBasePhaseSelectorView m_CharGenSelectionsCommonPhaseSelectorView;

	private readonly ReactiveProperty<ActivePhaseNavigation> m_ActivePhaseNavigation = new ReactiveProperty<ActivePhaseNavigation>(ActivePhaseNavigation.Menu);

	private readonly ReactiveProperty<bool> m_CanConfirm = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanDecline = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanShowInfo = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanSwitchNavigation = new ReactiveProperty<bool>();

	private IConsoleEntity m_ContentEntity;

	private bool m_HasTooltip;

	private GridConsoleNavigationBehaviour m_MenuNavigation;

	private GridConsoleNavigationBehaviour m_Navigation;

	private TooltipConfig m_TooltipConfig;

	private bool m_VerticalEntitiesAdded;

	protected override bool HasYScrollBindInternal => m_InfoView.IsScrollActive;

	public override void Initialize()
	{
		base.Initialize();
		m_SecondaryInfoViewContainer.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_VerticalEntitiesAdded = false;
		m_TooltipConfig = new TooltipConfig
		{
			TooltipPlace = m_SecondaryInfoView.GetComponent<RectTransform>(),
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			}
		};
		m_InfoView.Bind(base.ViewModel.InfoVM);
		m_SecondaryInfoView.Bind(base.ViewModel.SecondaryInfoVM);
		m_CharGenSelectionsCommonPhaseSelectorView.Bind(base.ViewModel.SelectionGroup);
		base.ViewModel.IsCompletedAndAvailable.Subscribe(delegate(bool value)
		{
			CanGoNextOnConfirm.Value = value;
		}).AddTo(this);
		m_CanDecline.Subscribe(delegate(bool value)
		{
			CanGoBackOnDecline.Value = !value;
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_VerticalEntitiesAdded = false;
		base.OnUnbind();
		TooltipHelper.HideTooltip();
		TooltipHelper.HideInfo();
	}

	public override void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, ReadOnlyReactiveProperty<bool> isMainCharacter)
	{
		m_Navigation = navigationBehaviour;
		m_MenuNavigation = new GridConsoleNavigationBehaviour().AddTo(this);
		isMainCharacter.Subscribe(delegate(bool value)
		{
			if (value && !m_VerticalEntitiesAdded)
			{
				m_MenuNavigation.SetEntitiesVertical(m_CharGenSelectionsCommonPhaseSelectorView.GetNavigationEntities());
				m_VerticalEntitiesAdded = true;
			}
		}).AddTo(this);
		if (isMainCharacter.CurrentValue)
		{
			m_MenuNavigation.SetEntitiesVertical(m_CharGenSelectionsCommonPhaseSelectorView.GetNavigationEntities());
			m_VerticalEntitiesAdded = true;
		}
		m_ActivePhaseNavigation.Subscribe(UpdateActiveNavigation).AddTo(this);
		inputLayer.AddAxis(Scroll, 3).AddTo(this);
		m_Navigation.DeepestFocusAsObservable.Subscribe(OnFocusChanged).AddTo(this);
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			SwitchNavigation();
		}, 10, m_CanSwitchNavigation);
		hintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.Information).AddTo(this);
		inputBindStruct.AddTo(this);
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			OnDeclineClick();
		}, 9, m_CanDecline);
		hintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.CommonTexts.Back).AddTo(this);
		inputBindStruct2.AddTo(this);
		InputBindStruct inputBindStruct3 = inputLayer.AddButton(delegate
		{
			SwitchNavigation();
		}, 8, m_CanConfirm);
		hintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.CommonTexts.Information).AddTo(this);
		inputBindStruct3.AddTo(this);
		InputBindStruct inputBindStruct4 = inputLayer.AddButton(delegate
		{
			ToggleTooltip();
		}, 8, m_CanShowInfo);
		hintsWidget.BindHint(inputBindStruct4, UIStrings.Instance.CommonTexts.Information).AddTo(this);
		inputBindStruct4.AddTo(this);
		AddInputToPaperHints(ref inputLayer, ref isMainCharacter);
	}

	private void AddInputToPaperHints(ref InputLayer inputLayer, ref ReadOnlyReactiveProperty<bool> isMainCharacter)
	{
		if (PaperHints != null)
		{
			InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
			{
				base.ViewModel.GoPrevPage();
				RefreshMenuFocus();
			}, 12, base.ViewModel.CurrentPageIsFirst.CombineLatest(m_ActivePhaseNavigation, (bool isFirst, ActivePhaseNavigation navigation) => !isFirst && navigation == ActivePhaseNavigation.Menu).And(isMainCharacter).ToReadOnlyReactiveProperty(initialValue: false));
			PaperHints.PageUpHint.Bind(inputBindStruct).AddTo(this);
			inputBindStruct.AddTo(this);
			InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
			{
				base.ViewModel.GoNextPage();
				RefreshMenuFocus();
			}, 13, base.ViewModel.CurrentPageIsLast.CombineLatest(m_ActivePhaseNavigation, (bool isLast, ActivePhaseNavigation navigation) => !isLast && navigation == ActivePhaseNavigation.Menu).And(isMainCharacter).ToReadOnlyReactiveProperty(initialValue: false));
			PaperHints.PageDownHint.Bind(inputBindStruct2).AddTo(this);
			inputBindStruct2.AddTo(this);
		}
	}

	private void SetMenuNavigation()
	{
		m_Navigation.Clear();
		m_Navigation.AddColumn<GridConsoleNavigationBehaviour>(m_MenuNavigation);
		m_MenuNavigation.FocusOnEntityManual(m_CharGenSelectionsCommonPhaseSelectorView.GetSelectedEntity());
		m_Navigation.FocusOnEntityManual(m_MenuNavigation);
		m_ContentEntity = null;
		m_InfoView.ScrollRectExtended.ScrollToTop();
	}

	private void RefreshMenuFocus()
	{
		m_MenuNavigation.FocusOnEntityManual(m_CharGenSelectionsCommonPhaseSelectorView.GetSelectedEntity());
	}

	private void SetContentNavigation()
	{
		IConsoleEntity contentEntity = m_ContentEntity;
		m_Navigation.Clear();
		m_Navigation.AddColumn<GridConsoleNavigationBehaviour>(m_InfoView.GetNavigationBehaviour());
		if (contentEntity != null)
		{
			m_Navigation.FocusOnEntityManual(contentEntity);
		}
		else
		{
			m_Navigation.FocusOnFirstValidEntity();
		}
	}

	private void SetSecondaryInfoNavigation()
	{
		m_Navigation.Clear();
		m_Navigation.AddColumn<GridConsoleNavigationBehaviour>(m_SecondaryInfoView.GetNavigationBehaviour());
		m_Navigation.FocusOnFirstValidEntity();
		OnFocusChanged(m_Navigation.DeepestNestedFocus);
	}

	private void UpdateActiveNavigation(ActivePhaseNavigation activeNavigation)
	{
		TooltipHelper.HideTooltip();
		switch (activeNavigation)
		{
		case ActivePhaseNavigation.Menu:
			SetMenuNavigation();
			break;
		case ActivePhaseNavigation.Content:
			SetContentNavigation();
			break;
		case ActivePhaseNavigation.SecondaryInfo:
			SetSecondaryInfoNavigation();
			break;
		}
		bool active = activeNavigation == ActivePhaseNavigation.Content || activeNavigation == ActivePhaseNavigation.SecondaryInfo;
		m_SecondaryInfoViewContainer.SetActive(active);
		m_CanDecline.Value = activeNavigation == ActivePhaseNavigation.Content || activeNavigation == ActivePhaseNavigation.SecondaryInfo;
		m_CanSwitchNavigation.Value = activeNavigation == ActivePhaseNavigation.Menu;
		CanGoNextOnConfirm.Value = activeNavigation == ActivePhaseNavigation.Menu;
	}

	private void SwitchNavigation(bool forward = true)
	{
		switch (m_ActivePhaseNavigation.Value)
		{
		case ActivePhaseNavigation.Menu:
			m_ActivePhaseNavigation.Value = ActivePhaseNavigation.Content;
			break;
		case ActivePhaseNavigation.Content:
			m_ActivePhaseNavigation.Value = (forward ? ActivePhaseNavigation.SecondaryInfo : ActivePhaseNavigation.Menu);
			break;
		case ActivePhaseNavigation.SecondaryInfo:
			m_ActivePhaseNavigation.Value = ActivePhaseNavigation.Content;
			break;
		}
	}

	private void Scroll(InputActionEventData data, float y)
	{
		InfoSectionView infoSectionView = ((m_ActivePhaseNavigation.Value == ActivePhaseNavigation.Menu) ? m_InfoView : m_SecondaryInfoView);
		if (!(Mathf.Abs(data.player.GetAxis(3)) < Mathf.Abs(data.player.GetAxis(2))) && (!(y > 0f) || !infoSectionView.ScrollbarOnTop) && (!(y < 0f) || !infoSectionView.ScrollbarOnBottom))
		{
			infoSectionView.Scroll(y);
		}
	}

	private void OnDeclineClick()
	{
		SwitchNavigation(forward: false);
	}

	protected virtual void OnFocusChanged(IConsoleEntity entity)
	{
		TooltipBaseTemplate tooltipBaseTemplate = (entity as IHasTooltipTemplate)?.TooltipTemplate();
		TooltipHelper.HideTooltip();
		m_HasTooltip = tooltipBaseTemplate != null;
		m_CanConfirm.Value = m_HasTooltip && m_ActivePhaseNavigation.Value == ActivePhaseNavigation.Content;
		m_CanShowInfo.Value = m_HasTooltip && m_ActivePhaseNavigation.Value == ActivePhaseNavigation.SecondaryInfo;
		if (m_ActivePhaseNavigation.Value == ActivePhaseNavigation.Content)
		{
			base.ViewModel.SecondaryInfoVM.SetTemplate(tooltipBaseTemplate);
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

	private void ToggleTooltip()
	{
		CharGenConsoleView.ShowTooltip = !CharGenConsoleView.ShowTooltip;
		OnFocusChanged(m_Navigation.DeepestNestedFocus);
	}
}
