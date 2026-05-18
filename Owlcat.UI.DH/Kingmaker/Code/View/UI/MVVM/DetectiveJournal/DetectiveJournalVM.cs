using System;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.MVVM.DetectiveJournal.MainPage;
using Kingmaker.Code.View.UI.MVVM.ServiceWindows;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Events;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Sound;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class DetectiveJournalVM : ViewModel, IServiceWindow
{
	public readonly ReactiveProperty<bool> SwitchedFromServiceWindow = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<DetectiveMainPageVM> m_MainPageVM = new ReactiveProperty<DetectiveMainPageVM>();

	private readonly ReactiveProperty<DetectiveOpenedCaseVM> m_OpenedCaseVM = new ReactiveProperty<DetectiveOpenedCaseVM>();

	public ReadOnlyReactiveProperty<DetectiveMainPageVM> MainPageVM => m_MainPageVM;

	public ReadOnlyReactiveProperty<DetectiveOpenedCaseVM> OpenedCaseVM => m_OpenedCaseVM;

	public DetectiveJournalVM(BlueprintCase caseToOpen = null)
	{
		SoundState.Instance.OpenedCaseWasShowBefore = false;
		m_MainPageVM.Value = new DetectiveMainPageVM(delegate(BlueprintCase @case)
		{
			OpenCase(@case);
		}).AddTo(this);
		if (caseToOpen != null)
		{
			OpenCase(caseToOpen);
		}
		SoundState.Instance.OnDetectiveJournalChange(MusicStateHandler.DetectiveBoardMusicState.Default);
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		SoundState.Instance.OnMusicStateChange(MusicStateHandler.MusicState.Setting);
		SoundState.Instance.OnDetectiveJournalChange(MusicStateHandler.DetectiveBoardMusicState.Default);
	}

	public void SetOpenedCase(BlueprintCase caseToOpen, BlueprintClue focusClue, bool canCloseCase = true)
	{
		OpenCase(caseToOpen, canCloseCase);
		if (!canCloseCase || focusClue == null)
		{
			return;
		}
		if (focusClue.ParentCase.Blueprint.IsUnknown() && m_OpenedCaseVM.Value == null)
		{
			m_OpenedCaseVM.Value = new DetectiveOpenedCaseVM(null, CloseOpenedCase).AddTo(this);
		}
		ObservableSubscribeExtensions.Subscribe(Observable.Timer(0.75f.Seconds(), UnityTimeProvider.UpdateIgnoreTimeScale), delegate
		{
			EventBus.RaiseEvent(delegate(IMoveToCaseItemHandler h)
			{
				h.HandleMoveToCaseItem(focusClue);
			});
		}).AddTo(this);
	}

	private void OpenCase(BlueprintCase caseToOpen, bool canCloseCase = true)
	{
		Action closeAction = (canCloseCase ? new Action(CloseOpenedCase) : null);
		m_OpenedCaseVM.Value = new DetectiveOpenedCaseVM(caseToOpen, closeAction).AddTo(this);
		m_MainPageVM.Value?.Dispose();
		m_MainPageVM.Value = null;
	}

	private void CloseOpenedCase()
	{
		m_MainPageVM.Value = new DetectiveMainPageVM(delegate(BlueprintCase @case)
		{
			OpenCase(@case);
		}).AddTo(this);
		m_OpenedCaseVM.Value?.Dispose();
		m_OpenedCaseVM.Value = null;
	}

	public void HandleOnSwitchedFromWindow()
	{
		SwitchedFromServiceWindow.Value = true;
	}
}
