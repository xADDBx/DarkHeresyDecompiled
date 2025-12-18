using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenShipPhaseDetailedConsoleView : CharGenShipPhaseDetailedView
{
	[SerializeField]
	private GameObject m_SecondaryInfoViewContainer;

	[SerializeField]
	private InfoSectionView m_SecondaryInfoView;

	[SerializeField]
	private CharGenChangeNameMessageBoxConsoleView m_MessageBoxConsoleView;

	private readonly ReactiveProperty<ActivePhaseNavigation> m_ActivePhaseNavigation = new ReactiveProperty<ActivePhaseNavigation>(ActivePhaseNavigation.Menu);

	private readonly ReactiveProperty<bool> m_CanConfirm = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanDecline = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanSwitchNavigation = new ReactiveProperty<bool>();

	private bool m_HasTooltip;

	private GridConsoleNavigationBehaviour m_MenuNavigation;

	private GridConsoleNavigationBehaviour m_Navigation;

	public override void Initialize()
	{
		base.Initialize();
		m_SecondaryInfoViewContainer.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_SecondaryInfoView.Bind(base.ViewModel.SecondaryInfoVM);
		base.ViewModel.MessageBoxVM.Subscribe(m_MessageBoxConsoleView.Bind).AddTo(this);
		m_CanDecline.Subscribe(delegate(bool value)
		{
			CanGoBackOnDecline.Value = !value;
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		TooltipHelper.HideTooltip();
		TooltipHelper.HideInfo();
	}

	public override void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, ReadOnlyReactiveProperty<bool> isMainCharacter)
	{
		m_Navigation = navigationBehaviour;
		m_MenuNavigation = new GridConsoleNavigationBehaviour().AddTo(this);
		m_MenuNavigation.SetEntitiesVertical(m_CharGenShipPhaseSelectorView.GetNavigationEntities());
		m_ActivePhaseNavigation.Subscribe(UpdateActiveNavigation).AddTo(this);
		inputLayer.AddAxis(Scroll, 3).AddTo(this);
		m_Navigation.DeepestFocusAsObservable.Subscribe(OnFocusChanged).AddTo(this);
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			OnConfirmClick();
		}, 8, m_CanConfirm);
		hintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.Select).AddTo(this);
		inputBindStruct.AddTo(this);
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			SwitchNavigation();
		}, 10, m_CanSwitchNavigation);
		hintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.CommonTexts.Information).AddTo(this);
		inputBindStruct2.AddTo(this);
		InputBindStruct inputBindStruct3 = inputLayer.AddButton(delegate
		{
			OnDeclineClick();
		}, 9, m_CanDecline);
		hintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.CommonTexts.Back).AddTo(this);
		inputBindStruct3.AddTo(this);
		InputBindStruct inputBindStruct4 = inputLayer.AddButton(delegate
		{
			OnFunc02Click();
		}, 11, base.ViewModel.IsCompletedAndAvailable);
		hintsWidget.BindHint(inputBindStruct4, UIStrings.Instance.CharGen.EditName).AddTo(this);
		inputBindStruct4.AddTo(this);
		AddInputToPaperHints(ref inputLayer);
	}

	private void AddInputToPaperHints(ref InputLayer inputLayer)
	{
		if (PaperHints != null)
		{
			InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
			{
				base.ViewModel.GoPrevPage();
				RefreshMenuFocus();
			}, 12, base.ViewModel.CurrentPageIsFirst.CombineLatest(m_ActivePhaseNavigation, (bool isFirst, ActivePhaseNavigation navigation) => !isFirst && navigation == ActivePhaseNavigation.Menu).ToReadOnlyReactiveProperty(initialValue: false));
			PaperHints.PageUpHint.Bind(inputBindStruct).AddTo(this);
			inputBindStruct.AddTo(this);
			InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
			{
				base.ViewModel.GoNextPage();
				RefreshMenuFocus();
			}, 13, base.ViewModel.CurrentPageIsLast.CombineLatest(m_ActivePhaseNavigation, (bool isLast, ActivePhaseNavigation navigation) => !isLast && navigation == ActivePhaseNavigation.Menu).ToReadOnlyReactiveProperty(initialValue: false));
			PaperHints.PageDownHint.Bind(inputBindStruct2).AddTo(this);
			inputBindStruct2.AddTo(this);
		}
	}

	private void RefreshMenuFocus()
	{
		m_MenuNavigation.FocusOnEntityManual(m_CharGenShipPhaseSelectorView.GetSelectedEntity());
	}

	private void SetMenuNavigation()
	{
		m_Navigation.Clear();
		m_Navigation.AddColumn<GridConsoleNavigationBehaviour>(m_MenuNavigation);
		m_MenuNavigation.FocusOnEntityManual(m_CharGenShipPhaseSelectorView.GetSelectedEntity());
		m_Navigation.FocusOnEntityManual(m_MenuNavigation);
	}

	private void SetContentNavigation()
	{
		m_Navigation.Clear();
		m_Navigation.AddColumn<GridConsoleNavigationBehaviour>(m_InfoView.GetNavigationBehaviour());
		m_Navigation.FocusOnFirstValidEntity();
	}

	private void UpdateActiveNavigation(ActivePhaseNavigation activeNavigation)
	{
		if (activeNavigation == ActivePhaseNavigation.Menu)
		{
			SetMenuNavigation();
		}
		else
		{
			SetContentNavigation();
		}
		m_CanDecline.Value = activeNavigation == ActivePhaseNavigation.Content;
		m_CanSwitchNavigation.Value = activeNavigation == ActivePhaseNavigation.Menu;
		CanGoNextOnConfirm.Value = activeNavigation == ActivePhaseNavigation.Menu;
	}

	private void SwitchNavigation()
	{
		m_ActivePhaseNavigation.Value = ((m_ActivePhaseNavigation.Value != ActivePhaseNavigation.Content) ? ActivePhaseNavigation.Content : ActivePhaseNavigation.Menu);
	}

	private void Scroll(InputActionEventData data, float y)
	{
		InfoSectionView infoSectionView = ((m_ActivePhaseNavigation.Value == ActivePhaseNavigation.Menu) ? m_InfoView : m_SecondaryInfoView);
		if (!(Mathf.Abs(data.player.GetAxis(3)) < Mathf.Abs(data.player.GetAxis(2))) && (!(y > 0f) || !infoSectionView.ScrollbarOnTop) && (!(y < 0f) || !infoSectionView.ScrollbarOnBottom))
		{
			infoSectionView.Scroll(y);
		}
	}

	private void OnConfirmClick()
	{
		OnFocusChanged(m_Navigation.DeepestNestedFocus);
		if (m_ActivePhaseNavigation.Value == ActivePhaseNavigation.Content && m_HasTooltip)
		{
			TooltipHelper.ShowInfo(base.ViewModel.SecondaryInfoVM.CurrentTooltip);
		}
	}

	private void OnDeclineClick()
	{
		SwitchNavigation();
	}

	protected virtual void OnFocusChanged(IConsoleEntity entity)
	{
		TooltipBaseTemplate tooltipBaseTemplate = (entity as IHasTooltipTemplate)?.TooltipTemplate();
		m_CanConfirm.Value = (m_HasTooltip = tooltipBaseTemplate != null);
		if (tooltipBaseTemplate == null)
		{
			base.ViewModel.SecondaryInfoVM.SetTemplate(null);
		}
		else
		{
			base.ViewModel.SecondaryInfoVM.SetTemplate(tooltipBaseTemplate);
		}
	}

	private void OnFunc02Click()
	{
		base.ViewModel.ShowChangeNameMessageBox();
	}

	public override bool PressConfirmOnPhase()
	{
		return true;
	}
}
