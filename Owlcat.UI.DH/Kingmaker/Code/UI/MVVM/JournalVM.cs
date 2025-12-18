using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Code.UI.MVVM.Common;
using Kingmaker.Designers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class JournalVM : ViewModel, ISetCurrentQuestHandler, ISubscriber
{
	public readonly JournalNavigationVM Navigation;

	public readonly LensSelectorVM Selector;

	private readonly ReactiveProperty<Quest> m_SelectedQuest = new ReactiveProperty<Quest>();

	private readonly ReactiveCommand<Quest> m_UpdateView = new ReactiveCommand<Quest>();

	public ReadOnlyReactiveProperty<Quest> SelectedQuest => m_SelectedQuest;

	public Observable<Quest> UpdateView => m_UpdateView;

	public JournalVM()
	{
		Navigation = new JournalNavigationVM(GameHelper.Quests.GetList(), m_SelectedQuest, SelectQuest).AddTo(this);
		Selector = new LensSelectorVM().AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		m_SelectedQuest.Value = JournalHelper.CurrentQuest ?? SelectedQuest.CurrentValue;
		m_UpdateView.Execute(SelectedQuest.CurrentValue);
	}

	private void SelectQuest(Quest quest)
	{
		if (quest != null)
		{
			m_SelectedQuest.Value = quest;
			m_UpdateView.Execute(SelectedQuest.CurrentValue);
			JournalHelper.ChangeCurrentQuest(quest);
		}
	}

	void ISetCurrentQuestHandler.HandleSetCurrentQuest(Quest quest)
	{
		m_SelectedQuest.Value = quest;
		m_UpdateView.Execute(SelectedQuest.CurrentValue);
	}
}
