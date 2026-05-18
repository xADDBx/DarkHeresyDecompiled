using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Root;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class JournalNavigationGroupVM : ViewModel
{
	private readonly ReactiveProperty<bool> m_IsSelected = new ReactiveProperty<bool>();

	public readonly QuestGroup QuestGroup;

	public readonly string Title;

	public readonly List<JournalQuestVM> Quests;

	public ReadOnlyReactiveProperty<bool> IsSelected => m_IsSelected;

	public bool HasActiveQuests => Quests.Any((JournalQuestVM q) => q.IsActive || !q.IsViewed);

	public bool IsCollapse
	{
		get
		{
			return QuestGroup.IsCollapse;
		}
		set
		{
			QuestGroup.IsCollapse = value;
		}
	}

	public JournalNavigationGroupVM(QuestGroupId groupId, IEnumerable<Quest> quests, ReactiveProperty<Quest> selectedQuest, Action<Quest> selectQuest)
	{
		QuestGroup = ConfigRoot.Instance.Quests.GetGroup(groupId);
		LocalizedString name = QuestGroup.Name;
		Title = ((name != null) ? ((string)name) : string.Empty);
		selectedQuest.Subscribe(OnSelectedQuestChange).AddTo(this);
		Quests = new List<JournalQuestVM>();
		foreach (Quest quest in quests)
		{
			Quests.Add(new JournalQuestVM(quest, selectedQuest, selectQuest).AddTo(this));
		}
	}

	private void OnSelectedQuestChange(Quest quest)
	{
		bool value = Quests.Any((JournalQuestVM questVM) => questVM.Quest == quest);
		m_IsSelected.Value = value;
	}
}
