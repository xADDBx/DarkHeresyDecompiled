using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.Common;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Events;
using Owlcat.UI;
using R3;
using Rewired;
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
	[UsedImplicitly]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private FlexibleLensSelectorView m_SelectorView;

	[SerializeField]
	private ConsoleHint m_PrevHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

	[Header("CanvasSorting")]
	[SerializeField]
	private CanvasSortingComponent m_CanvasSortingComponent;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

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
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		m_NavigationView.Bind(base.ViewModel.Navigation);
		m_SelectorView.Bind(base.ViewModel.Selector);
		base.ViewModel.UpdateView.Subscribe(OnSelectedQuestChange).AddTo(this);
		OnSelectedQuestChange(base.ViewModel.SelectedQuest.CurrentValue);
		CreateInput();
		UpdateNavigation();
		m_NavigationBehaviour.Focus.Subscribe(m_NavigationView.ScrollMenu).AddTo(this);
		m_IsShowCompletedQuests.Value = Game.Instance.Player.UISettings.JournalShowCompletedQuest;
	}

	private void UpdateNavigation()
	{
		m_InputLayer.Unbind();
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.SetEntitiesVertical(m_NavigationView.GetNavigationEntities());
		m_InputLayer.Bind();
		CheckHasFinishedQuests();
		foreach (IConsoleEntity entity in m_NavigationBehaviour.Entities)
		{
			if (entity is JournalNavigationGroupElementConsoleView journalNavigationGroupElementConsoleView && entity.IsValid() && journalNavigationGroupElementConsoleView.IsSelected)
			{
				m_NavigationBehaviour.FocusOnEntityManual(entity);
				m_NavigationView.HandleElementSelected(journalNavigationGroupElementConsoleView);
				break;
			}
		}
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
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "Journal"
		});
		m_NavigationBehaviour.Focus.Subscribe(delegate
		{
			m_IsQuestEntity.Value = m_NavigationBehaviour.CurrentEntity is JournalNavigationGroupElementConsoleView;
			UpdateExpandableElementFlags(m_NavigationBehaviour.CurrentEntity);
		}).AddTo(this);
		m_InputLayer.AddAxis(Scroll, 3, repeat: true).AddTo(this);
		m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			ShowCompletedQuestsChange();
		}, 11, m_IsShowCompletedQuests.And(m_HasFinishedQuests).ToReadOnlyReactiveProperty(initialValue: false)), UIStrings.Instance.QuesJournalTexts.HideCompletedQuests).AddTo(this);
		m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			ShowCompletedQuestsChange();
		}, 11, m_IsShowCompletedQuests.Not().And(m_HasFinishedQuests).ToReadOnlyReactiveProperty(initialValue: false)), UIStrings.Instance.QuesJournalTexts.ShowCompletedQuests).AddTo(this);
		m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			CloseWindow();
		}, 9), UIStrings.Instance.CommonTexts.CloseWindow).AddTo(this);
		m_PrevHint.Bind(m_InputLayer.AddButton(delegate
		{
			m_NavigationView.OnPrevActiveTab();
			m_SelectorView.ChangeTab((int)m_NavigationView.GetActiveTab());
			UpdateNavigation();
		}, 14)).AddTo(this);
		m_NextHint.Bind(m_InputLayer.AddButton(delegate
		{
			m_NavigationView.OnNextActiveTab();
			m_SelectorView.ChangeTab((int)m_NavigationView.GetActiveTab());
			UpdateNavigation();
		}, 15)).AddTo(this);
		m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			Collapse();
		}, 8, m_CanCollapse), UIStrings.Instance.CommonTexts.Collapse).AddTo(this);
		m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			Expand();
		}, 8, m_CanExpand), UIStrings.Instance.CommonTexts.Expand).AddTo(this);
		m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnFocusedChanged(m_NavigationBehaviour.CurrentEntity);
		}, 8, m_IsQuestEntity), UIStrings.Instance.CommonTexts.Select).AddTo(this);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
		m_CanvasSortingComponent.PushView().AddTo(this);
	}

	private void Scroll(InputActionEventData obj, float value)
	{
		if (m_NavigationView.GetActiveTab() == JournalTab.Quests)
		{
			m_QuestView.Scroll(obj, value);
		}
	}

	private void Expand()
	{
		OwlcatMultiButton owlcatMultiButton = m_NavigationBehaviour.Focus.Value as OwlcatMultiButton;
		if (!(owlcatMultiButton == null) && (bool)owlcatMultiButton.gameObject.GetComponent<ExpandableCollapseMultiButtonConsole>())
		{
			owlcatMultiButton.GetComponent<ExpandableCollapseMultiButtonConsole>().Expand();
			UpdateExpandableElementFlags(m_NavigationBehaviour.CurrentEntity);
		}
	}

	private void Collapse()
	{
		OwlcatMultiButton owlcatMultiButton = m_NavigationBehaviour.Focus.Value as OwlcatMultiButton;
		if (!(owlcatMultiButton == null) && (bool)owlcatMultiButton.gameObject.GetComponent<ExpandableCollapseMultiButtonConsole>())
		{
			owlcatMultiButton.GetComponent<ExpandableCollapseMultiButtonConsole>().Collapse();
			UpdateExpandableElementFlags(m_NavigationBehaviour.CurrentEntity);
		}
	}

	private void ShowCompletedQuestsChange()
	{
		bool journalShowCompletedQuest = Game.Instance.Player.UISettings.JournalShowCompletedQuest;
		m_NavigationView.OnShowCompletedToggleChanged(!journalShowCompletedQuest);
		m_IsShowCompletedQuests.Value = !journalShowCompletedQuest;
		UpdateNavigation();
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
