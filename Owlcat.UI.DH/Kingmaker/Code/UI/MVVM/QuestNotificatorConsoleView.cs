using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class QuestNotificatorConsoleView : QuestNotificatorBaseView
{
	[SerializeField]
	private HintView m_CloseHint;

	[SerializeField]
	private HintView m_JournalHint;

	private CompositeDisposable m_Disposable;

	private readonly ReactiveProperty<bool> m_IsJournalHintActive = new ReactiveProperty<bool>();

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_Disposable?.Dispose();
		m_Disposable = null;
	}

	protected override void ShowNextNotification()
	{
		base.ShowNextNotification();
		if (m_Disposable == null)
		{
			m_Disposable = new CompositeDisposable();
		}
		else
		{
			m_Disposable.Clear();
		}
	}

	protected override void CheckJournalButtons()
	{
		base.CheckJournalButtons();
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			m_IsJournalHintActive.Value = CheckActiveToJournalButtons();
		}).AddTo(this);
	}

	protected override void Hide()
	{
		base.Hide();
		m_Disposable?.Clear();
	}
}
