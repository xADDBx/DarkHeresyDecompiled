using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ObsoleteCharacterInfoPCView : View<ObsoleteCharacterInfoVM>
{
	[SerializeField]
	protected CharInfoPagesMenuPCView m_CharInfoPagesMenu;

	[SerializeField]
	protected CharInfoNameAndPortraitBaseView m_NameAndPortraitView;

	[SerializeField]
	private CharInfoLevelClassScoresPCView m_LevelClassScoresView;

	[SerializeField]
	protected CharInfoSkillsAndWeaponsBaseView m_SkillsAndWeaponsView;

	[SerializeField]
	private CharInfoAlignmentWheelPCView m_AlignmentWheelView;

	[SerializeField]
	private CharInfoFeaturesBaseView FeaturesView;

	[SerializeField]
	private CharInfoChoicesMadeView m_ChoicesMadeView;

	[SerializeField]
	private CharInfoNameAndPortraitBaseView NameFullPortraitBaseView;

	[SerializeField]
	private CharInfoFactionsReputationPCView m_FactionsReputationPCView;

	[SerializeField]
	private CharInfoStoriesView m_BiographyStoriesView;

	[SerializeField]
	protected UnitProgressionCommonView m_ProgressionView;

	[SerializeField]
	private CharInfoSummaryBaseView m_SummaryView;

	protected readonly Dictionary<CharInfoComponentType, ICharInfoComponentView> ComponentViews = new Dictionary<CharInfoComponentType, ICharInfoComponentView>();

	public virtual void Initialize()
	{
		m_NameAndPortraitView.Initialize();
		ComponentViews[CharInfoComponentType.NameAndPortrait] = m_NameAndPortraitView;
		m_LevelClassScoresView.Initialize();
		ComponentViews[CharInfoComponentType.LevelClassScores] = m_LevelClassScoresView;
		m_SkillsAndWeaponsView.Initialize();
		ComponentViews[CharInfoComponentType.SkillsAndWeapons] = m_SkillsAndWeaponsView;
		m_AlignmentWheelView.Initialize();
		ComponentViews[CharInfoComponentType.AlignmentWheel] = m_AlignmentWheelView;
		FeaturesView.Initialize();
		ComponentViews[CharInfoComponentType.Abilities] = FeaturesView;
		m_ChoicesMadeView.Initialize();
		ComponentViews[CharInfoComponentType.AlignmentHistory] = m_ChoicesMadeView;
		NameFullPortraitBaseView.Initialize();
		ComponentViews[CharInfoComponentType.NameFullPortrait] = NameFullPortraitBaseView;
		m_FactionsReputationPCView.Initialize();
		ComponentViews[CharInfoComponentType.FactionsReputation] = m_FactionsReputationPCView;
		m_BiographyStoriesView.Initialize();
		ComponentViews[CharInfoComponentType.BiographyStories] = m_BiographyStoriesView;
		m_ProgressionView.Initialize(OnProgressionWindowStateChange);
		ComponentViews[CharInfoComponentType.Progression] = m_ProgressionView;
		m_SummaryView.Initialize();
		ComponentViews[CharInfoComponentType.Summary] = m_SummaryView;
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		ShowWindow();
		m_CharInfoPagesMenu.Bind(base.ViewModel.PagesSelectionGroupRadioVM);
		foreach (KeyValuePair<CharInfoComponentType, ICharInfoComponentView> componentView in ComponentViews)
		{
			base.ViewModel.ComponentVMs[componentView.Key].Subscribe(componentView.Value.BindSection).AddTo(this);
		}
	}

	protected override void OnUnbind()
	{
		HideWindow();
	}

	protected virtual void OnProgressionWindowStateChange(UnitProgressionWindowState state)
	{
	}

	private void ShowWindow()
	{
		base.gameObject.SetActive(value: true);
		OnShow();
		UISounds.Instance.Sounds.Character.CharacterInfoShow.Play();
	}

	private void OnShow()
	{
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.CharacterScreen);
		});
		EventBus.RaiseEvent(delegate(IFullScreenUIHandlerWorkaround h)
		{
			h.HandleFullScreenUiChangedWorkaround(state: true, FullScreenUIType.CharacterScreen);
		});
		Game.Instance.RequestPauseUi(isPaused: true);
	}

	private void HideWindow()
	{
		OnHide();
		ContextMenuHelper.HideContextMenu();
		base.gameObject.SetActive(value: false);
		UISounds.Instance.Sounds.Character.CharacterInfoHide.Play();
	}

	private void OnHide()
	{
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.CharacterScreen);
		});
		EventBus.RaiseEvent(delegate(IFullScreenUIHandlerWorkaround h)
		{
			h.HandleFullScreenUiChangedWorkaround(state: false, FullScreenUIType.CharacterScreen);
		});
		Game.Instance.RequestPauseUi(isPaused: false);
	}
}
