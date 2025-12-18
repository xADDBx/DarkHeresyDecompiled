using System.Collections.Generic;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Enums;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class JournalPCView : JournalBaseView
{
	[SerializeField]
	private JournalNavigationPCView m_NavigationView;

	[SerializeField]
	private JournalQuestPCView m_QuestView;

	[SerializeField]
	private JournalQuestPCView m_DetectiveQuestView;

	public override void Initialize()
	{
		m_NavigationView.Initialize();
		m_QuestView.Initialize();
		m_DetectiveQuestView.Initialize();
		base.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_NavigationView.Bind(base.ViewModel.Navigation);
		base.ViewModel.UpdateView.Subscribe(OnSelectedQuestChange).AddTo(this);
		OnSelectedQuestChange(base.ViewModel.SelectedQuest.CurrentValue);
		UpdateNavigation();
		UIKeybindGeneralSettings uIKeybindGeneralSettings = UISettingsRoot.Instance.UIKeybindGeneralSettings;
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.PrevTab.name, delegate
		{
			m_NavigationView.OnPrevActiveTab();
		}).AddTo(this);
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.NextTab.name, delegate
		{
			m_NavigationView.OnNextActiveTab();
		}).AddTo(this);
	}

	private void UpdateNavigation()
	{
		if (!JournalHelper.HasCurrentQuest)
		{
			JournalHelper.ChangeCurrentQuest(m_NavigationView.GetCurrentQuest(null));
		}
	}

	private void OnSelectedQuestChange(Quest selectedQuest)
	{
		foreach (JournalNavigationGroupVM navigationGroup in base.ViewModel.Navigation.NavigationGroups)
		{
			if (TryBindQuestView(navigationGroup.Quests))
			{
				break;
			}
		}
	}

	private bool TryBindQuestView(IEnumerable<JournalQuestVM> quests)
	{
		foreach (JournalQuestVM quest in quests)
		{
			if (quest.IsSelected.CurrentValue)
			{
				JournalQuestPCView journalQuestPCView = ((quest.Quest.Blueprint.Group == QuestGroupId.DetectiveCases) ? m_DetectiveQuestView : m_QuestView);
				JournalQuestPCView obj = ((quest.Quest.Blueprint.Group == QuestGroupId.DetectiveCases) ? m_QuestView : m_DetectiveQuestView);
				journalQuestPCView.Bind(quest);
				obj.Unbind();
				return true;
			}
		}
		return false;
	}
}
