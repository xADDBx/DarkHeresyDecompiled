using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Events;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class QuestNotificatorVM : ViewModel, INewServiceWindowUIHandler, ISubscriber, IQuestObjectiveHandler, IQuestHandler
{
	private readonly ReactiveProperty<bool> m_IsShowUp = new ReactiveProperty<bool>(value: false);

	public static QuestNotificatorVM Instance;

	public readonly ObservableList<QuestNotificationQuestVM> QuestEntities = new ObservableList<QuestNotificationQuestVM>();

	public readonly ObservableList<QuestNotificationEntityVM> ObjectiveEntities = new ObservableList<QuestNotificationEntityVM>();

	private readonly ReactiveCommand<Unit> m_ForceCloseCommand = new ReactiveCommand<Unit>();

	private readonly ReactiveCommand<Unit> m_ClearCommand = new ReactiveCommand<Unit>();

	public ReadOnlyReactiveProperty<bool> IsShowUp => m_IsShowUp;

	public bool ForbiddenQuestNotification
	{
		get
		{
			if (!(Game.Instance.CurrentModeType == GameModeType.Cutscene) && !LoadingProcess.Instance.IsLoadingScreenActive)
			{
				return RootUIContext.Instance.FullScreenUIType == FullScreenUIType.GroupChanger;
			}
			return true;
		}
	}

	public Observable<Unit> ForceCloseCommand => m_ForceCloseCommand;

	public Observable<Unit> ClearCommand => m_ClearCommand;

	public QuestNotificatorVM()
	{
		EventBus.Subscribe(this).AddTo(this);
		Instance = this;
	}

	protected override void OnDispose()
	{
		Instance = null;
		QuestEntities.Clear();
		ObjectiveEntities.Clear();
	}

	public void SetIsShowUp(bool value)
	{
		m_IsShowUp.Value = value;
	}

	public void HandleQuestStarted(Quest quest)
	{
		AddQuestNotification(quest, QuestNotificationState.New);
	}

	public void HandleQuestCompleted(Quest quest)
	{
		AddQuestNotification(quest, QuestNotificationState.Completed);
	}

	public void HandleQuestFailed(Quest quest)
	{
		AddQuestNotification(quest, QuestNotificationState.Failed);
	}

	public void HandleQuestUpdated(Quest quest)
	{
		AddQuestNotification(quest, QuestNotificationState.Updated);
	}

	public void HandleQuestPostponed(Quest quest)
	{
		AddQuestNotification(quest, QuestNotificationState.Postponed);
	}

	private void AddQuestNotification(Quest quest, QuestNotificationState state)
	{
		if (!quest.Blueprint.IsSilentQuestNotification(state))
		{
			QuestEntities.Add(new QuestNotificationQuestVM(quest, state));
		}
	}

	public void HandleQuestShowed(QuestNotificationQuestVM quest)
	{
		quest.Dispose();
		QuestEntities.Remove(quest);
	}

	public void HandleQuestObjectiveStarted(QuestObjective objective)
	{
		AddObjective(objective, QuestNotificationState.New);
	}

	public void HandleQuestObjectiveBecameVisible(QuestObjective objective)
	{
		AddObjective(objective, QuestNotificationState.New);
	}

	public void HandleQuestObjectiveCompleted(QuestObjective objective)
	{
		AddObjective(objective, QuestNotificationState.Completed);
	}

	public void HandleQuestObjectiveFailed(QuestObjective objective)
	{
		AddObjective(objective, QuestNotificationState.Failed);
	}

	private void AddObjective(QuestObjective objective, QuestNotificationState state)
	{
		if (objective.IsVisible && !objective.Blueprint.IsSilentQuestNotification(state))
		{
			QuestNotificationEntityVM questNotificationEntityVM = new QuestNotificationEntityVM(objective, state);
			QuestNotificationEntityVM questNotificationEntityVM2 = ObjectiveEntities.FirstOrDefault((QuestNotificationEntityVM o) => !o.IsAddendum && o.Quest == objective.Quest);
			if (questNotificationEntityVM2 != null && !objective.Blueprint.IsAddendum)
			{
				questNotificationEntityVM2.AddObjective(questNotificationEntityVM);
			}
			else
			{
				ObjectiveEntities.Add(questNotificationEntityVM);
			}
		}
	}

	public void HandleObjectiveShowed(QuestNotificationEntityVM objective)
	{
		objective.Dispose();
		ObjectiveEntities.Remove(objective);
	}

	public void ForceClose()
	{
		QuestEntities.Clear();
		ObjectiveEntities.Clear();
		m_IsShowUp.Value = false;
		m_ForceCloseCommand.Execute();
	}

	public void HandleOpenJournal()
	{
		ForceClose();
	}

	public void HandleCloseAll()
	{
	}

	public void HandleOpenWindowOfType(ServiceWindowsType type)
	{
	}

	public void HandleOpenInventory()
	{
	}

	public void HandleOpenEncyclopedia(INode page = null)
	{
	}

	public void HandleOpenCharacterInfo()
	{
	}

	public void HandleOpenCharacterInfoPage(CharInfoPageType pageType, BaseUnitEntity unitEntity)
	{
	}

	public void HandleOpenLocalMap()
	{
	}

	public void HandleOpenDetectiveJournal()
	{
	}

	public void HandleOpenCargoManagement()
	{
	}

	public void OnAreaBeginUnloading()
	{
		QuestEntities.Clear();
		ObjectiveEntities.Clear();
		m_ClearCommand.Execute();
	}
}
