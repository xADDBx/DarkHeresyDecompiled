using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Code.UI.MVVM.Common;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Events;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class JournalConsoleView : JournalBaseView
{
	[Header("Console")]
	[SerializeField]
	private JournalNavigationConsoleView m_NavigationView;

	[SerializeField]
	private JournalQuestConsoleView m_QuestView;

	[Header("Hints")]
	[SerializeField]
	private FlexibleLensSelectorView m_SelectorView;

	[SerializeField]
	private HintView m_PrevHint;

	[SerializeField]
	private HintView m_NextHint;

	[Header("CanvasSorting")]
	[SerializeField]
	private CanvasSortingComponent m_CanvasSortingComponent;

	private readonly ReactiveProperty<bool> m_CanExpand = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanCollapse = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsShowCompletedQuests = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsQuestEntity = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsConfirmOrder = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasFinishedQuests = new ReactiveProperty<bool>();

	public override void Initialize()
	{
		m_NavigationView.Initialize();
		m_QuestView.Initialize();
		base.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_NavigationView.Bind(base.ViewModel.Navigation);
		m_SelectorView.Bind(base.ViewModel.Selector);
		base.ViewModel.UpdateView.Subscribe(OnSelectedQuestChange).AddTo(this);
		OnSelectedQuestChange(base.ViewModel.SelectedQuest.CurrentValue);
		CreateInput();
		m_IsShowCompletedQuests.Value = Game.Instance.Player.UISettings.JournalShowCompletedQuest;
	}

	private void CheckHasFinishedQuests()
	{
		if (base.ViewModel.Navigation.ActiveTab.CurrentValue != 0)
		{
			return;
		}
		m_HasFinishedQuests.Value = base.ViewModel.Navigation.NavigationGroups.Any((JournalNavigationGroupVM ng) => ng.Quests.Any((JournalQuestVM q) => !q.IsActive));
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
				m_QuestView.Bind(quest);
				return true;
			}
		}
		return false;
	}

	private void CreateInput()
	{
	}

	private void Scroll(float value)
	{
		if (m_NavigationView.GetActiveTab() == JournalTab.Quests)
		{
			m_QuestView.Scroll(value);
		}
	}

	public void UpdateExpandableElementFlags(IConsoleEntity entity)
	{
		if (entity is JournalNavigationGroupElementConsoleView)
		{
			m_CanExpand.Value = false;
			m_CanCollapse.Value = false;
		}
		OwlcatMultiButton owlcatMultiButton = entity as OwlcatMultiButton;
		if (!(owlcatMultiButton == null) && (bool)owlcatMultiButton.gameObject.GetComponent<ExpandableCollapseMultiButtonConsole>())
		{
			ExpandableCollapseMultiButtonConsole component = owlcatMultiButton.GetComponent<ExpandableCollapseMultiButtonConsole>();
			m_CanExpand.Value = !component.IsOn.CurrentValue;
			m_CanCollapse.Value = component.IsOn.CurrentValue;
		}
	}

	public void OnFocusedChanged(IConsoleEntity entity)
	{
		if (entity is JournalNavigationGroupElementConsoleView element)
		{
			m_NavigationView.HandleElementSelected(element);
		}
	}

	private void CloseWindow()
	{
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleCloseAll();
		});
	}
}
