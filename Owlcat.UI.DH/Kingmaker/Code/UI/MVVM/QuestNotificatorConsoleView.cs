using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class QuestNotificatorConsoleView : QuestNotificatorBaseView
{
	[SerializeField]
	private ConsoleHint m_CloseHint;

	[SerializeField]
	private ConsoleHint m_JournalHint;

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
		InputLayer baseLayer = GamePad.Instance.BaseLayer;
		m_Disposable.Add(m_CloseHint.BindCustomAction(9, baseLayer, base.ViewModel.IsShowUp.And(m_IsJournalHintActive).ToReadOnlyReactiveProperty(initialValue: false)));
		m_Disposable.Add(m_JournalHint.BindCustomAction(17, baseLayer, base.ViewModel.IsShowUp.And(m_IsJournalHintActive).ToReadOnlyReactiveProperty(initialValue: false)));
		m_CloseHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow);
		m_JournalHint.SetLabel(UIStrings.Instance.QuestNotificationTexts.ToJournal);
		baseLayer.Bind();
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
