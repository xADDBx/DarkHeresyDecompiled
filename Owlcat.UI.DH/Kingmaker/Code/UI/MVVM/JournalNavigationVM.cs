using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Enums;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class JournalNavigationVM : ViewModel
{
	private readonly ReactiveProperty<JournalTab> m_ActiveTab = new ReactiveProperty<JournalTab>(JournalTab.Quests);

	public readonly List<JournalNavigationGroupVM> NavigationGroups;

	public readonly Action<Quest> SelectQuest;

	public ReadOnlyReactiveProperty<JournalTab> ActiveTab => m_ActiveTab;

	public JournalNavigationVM(IEnumerable<Quest> quests, ReactiveProperty<Quest> selectedQuest, Action<Quest> selectQuest)
	{
		SelectQuest = selectQuest;
		Dictionary<QuestGroupId, List<Quest>> dictionary = new Dictionary<QuestGroupId, List<Quest>>();
		foreach (Quest quest in quests)
		{
			if (!dictionary.TryGetValue(quest.Blueprint.Group, out var value))
			{
				value = new List<Quest>();
				dictionary.Add(quest.Blueprint.Group, value);
			}
			value.Add(quest);
		}
		NavigationGroups = new List<JournalNavigationGroupVM>();
		List<QuestGroup> list = ConfigRoot.Instance.Quests?.Groups?.OrderBy((QuestGroup g) => g.Order).ToList();
		if (list.Empty())
		{
			Debug.LogError("QuestRoot is not configured or broken!");
			list = (from g in quests.Select((Quest q) => q.Blueprint.Group).ToList().Distinct()
				select new QuestGroup
				{
					Id = g,
					Name = null,
					Order = 0
				}).ToList();
		}
		foreach (QuestGroup group in list)
		{
			if (dictionary.ContainsKey(group.Id))
			{
				KeyValuePair<QuestGroupId, List<Quest>> keyValuePair = dictionary.First((KeyValuePair<QuestGroupId, List<Quest>> d) => d.Key == group.Id);
				NavigationGroups.Add(new JournalNavigationGroupVM(keyValuePair.Key, keyValuePair.Value, selectedQuest, selectQuest).AddTo(this));
			}
		}
	}

	public void SetActiveTab(JournalTab activeTab)
	{
		if (ActiveTab.CurrentValue != activeTab)
		{
			m_ActiveTab.Value = activeTab;
		}
	}

	public void OnNextActiveTab()
	{
		float num = MathF.Min((float)(ActiveTab.CurrentValue + 1), Enum.GetValues(typeof(JournalTab)).Length);
		SetActiveTab((JournalTab)num);
	}

	public void OnPrevActiveTab()
	{
		float num = MathF.Max((float)(ActiveTab.CurrentValue - 1), 0f);
		SetActiveTab((JournalTab)num);
	}
}
