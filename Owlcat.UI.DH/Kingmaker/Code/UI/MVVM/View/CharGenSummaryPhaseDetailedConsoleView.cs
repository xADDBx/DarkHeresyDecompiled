using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenSummaryPhaseDetailedConsoleView : CharGenSummaryPhaseDetailedView
{
	[Header("Common")]
	[SerializeField]
	private CharInfoAbilityScoresBlockBaseView m_AbilityScores;

	[Header("Secondary Info")]
	[SerializeField]
	private GameObject m_SecondaryInfoViewContainer;

	[SerializeField]
	private InfoSectionView m_SecondaryInfoView;

	private readonly ReactiveProperty<ActivePhaseNavigation> m_ActivePhaseNavigation = new ReactiveProperty<ActivePhaseNavigation>(ActivePhaseNavigation.Menu);

	private readonly ReactiveProperty<bool> m_CanConfirm = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanDecline = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanShowInfo = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanSwitchNavigation = new ReactiveProperty<bool>();

	private IConsoleEntity m_ContentEntity;

	private bool m_HasTooltip;

	private bool m_IsOnRightNavigation;

	private GridConsoleNavigationBehaviour m_LeftNavigation;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private GridConsoleNavigationBehaviour m_RightNavigation;

	private TooltipConfig m_TooltipConfig;

	private CharInfoSkillsBlockConsoleView CharInfoSkillsBlockConsoleView => m_SkillsBlockView as CharInfoSkillsBlockConsoleView;

	public override void Initialize()
	{
		base.Initialize();
		m_SecondaryInfoViewContainer.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_TooltipConfig = new TooltipConfig
		{
			TooltipPlace = m_SecondaryInfoView.GetComponent<RectTransform>(),
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			}
		};
		m_SecondaryInfoView.Bind(base.ViewModel.SecondaryInfoVM);
		CanGoNextOnConfirm.Value = true;
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		m_LeftNavigation = new GridConsoleNavigationBehaviour().AddTo(this);
		m_RightNavigation = new GridConsoleNavigationBehaviour().AddTo(this);
		m_RightNavigation.DeepestFocusAsObservable.Subscribe(delegate(IConsoleEntity entity)
		{
			if (m_ActivePhaseNavigation.Value != ActivePhaseNavigation.SecondaryInfo)
			{
				m_IsOnRightNavigation = entity != null;
			}
		}).AddTo(this);
		m_CanDecline.Subscribe(delegate(bool value)
		{
			CanGoBackOnDecline.Value = !value;
		}).AddTo(this);
		m_NavigationBehaviour.DeepestFocusAsObservable.Skip(1).Subscribe(OnFocusChanged).AddTo(this);
	}

	public override void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, ReadOnlyReactiveProperty<bool> isMainCharacter)
	{
		m_ActivePhaseNavigation.Subscribe(UpdateActiveNavigation).AddTo(this);
		inputLayer.AddAxis(Scroll, 3).AddTo(this);
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			OnFunc02Click();
		}, 11, m_CanDecline.Not().And(isMainCharacter).ToReadOnlyReactiveProperty(initialValue: false));
		hintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CharGen.EditName).AddTo(this);
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
			SwitchNavigation();
		}, 10, m_CanConfirm);
		hintsWidget.BindHint(inputBindStruct4, UIStrings.Instance.CommonTexts.Information).AddTo(this);
		inputBindStruct4.AddTo(this);
		InputBindStruct inputBindStruct5 = inputLayer.AddButton(delegate
		{
			ToggleTooltip();
		}, 19, m_CanShowInfo, InputActionEventType.ButtonJustReleased);
		hintsWidget.BindHint(inputBindStruct5, UIStrings.Instance.CommonTexts.Information).AddTo(this);
		inputBindStruct5.AddTo(this);
		navigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationBehaviour);
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
	}

	private void SetMenuNavigation()
	{
		m_NavigationBehaviour.Clear();
	}

	private void SetContentNavigation()
	{
		IConsoleEntity contentEntity = m_ContentEntity;
		m_NavigationBehaviour.Clear();
		m_RightNavigation.Clear();
		m_LeftNavigation.Clear();
		m_RightNavigation.AddColumn<GridConsoleNavigationBehaviour>(m_InfoView.GetNavigationBehaviour());
		m_LeftNavigation.AddEntityVertical(m_AbilityScores.GetNavigation());
		m_LeftNavigation.AddEntityVertical(CharInfoSkillsBlockConsoleView.GetConsoleEntity());
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_LeftNavigation);
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_RightNavigation);
		if (contentEntity != null)
		{
			if (m_IsOnRightNavigation)
			{
				m_RightNavigation.FocusOnEntityManual(contentEntity);
				m_NavigationBehaviour.FocusOnEntityManual(m_RightNavigation);
			}
			else
			{
				m_LeftNavigation.FocusOnEntityManual(contentEntity);
				m_NavigationBehaviour.FocusOnEntityManual(m_LeftNavigation);
			}
		}
		else
		{
			m_NavigationBehaviour.FocusOnEntityManual(m_LeftNavigation);
		}
	}

	private void SetSecondaryInfoNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_SecondaryInfoView.GetNavigationBehaviour());
		m_NavigationBehaviour.FocusOnFirstValidEntity();
		OnFocusChanged(m_NavigationBehaviour.DeepestNestedFocus);
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

	private void OnDeclineClick()
	{
		SwitchNavigation(forward: false);
	}

	private void OnFunc02Click()
	{
		TooltipHelper.HideTooltip();
		base.ViewModel.CharGenNameVM.ShowChangeNameMessageBox();
	}

	private void Scroll(InputActionEventData data, float y)
	{
		InfoSectionView infoSectionView = ((m_ActivePhaseNavigation.Value == ActivePhaseNavigation.Menu) ? m_InfoView : m_SecondaryInfoView);
		if (!(Mathf.Abs(data.player.GetAxis(3)) < Mathf.Abs(data.player.GetAxis(2))) && (!(y > 0f) || !infoSectionView.ScrollbarOnTop) && (!(y < 0f) || !infoSectionView.ScrollbarOnBottom))
		{
			infoSectionView.Scroll(y);
		}
	}

	private void OnFocusChanged(IConsoleEntity entity)
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
				monoBehaviour.ShowConsoleTooltip(tooltipBaseTemplate, m_NavigationBehaviour, m_TooltipConfig);
			}
		}
	}

	private void ToggleTooltip()
	{
		CharGenConsoleView.ShowTooltip = !CharGenConsoleView.ShowTooltip;
		OnFocusChanged(m_NavigationBehaviour.DeepestNestedFocus);
	}

	public override bool PressConfirmOnPhase()
	{
		return true;
	}
}
