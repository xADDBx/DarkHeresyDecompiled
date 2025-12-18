using System;
using System.Linq;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.HUDNotification.New.Utils;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using ObservableCollections;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class CaseNotificationVM : HUDNotificationNewVM, IDialogStartHandler, ISubscriber, IDialogFinishHandler
{
	public readonly BlueprintCase BlueprintCase;

	public readonly NotificationCaseBodyVM Case;

	public readonly ObservableList<NotificationClueBodyVM> Clues = new ObservableList<NotificationClueBodyVM>();

	private readonly ReactiveProperty<bool> m_IsNewCase = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanOpenJournal = new ReactiveProperty<bool>();

	private readonly Action<CaseNotificationVM> m_OnNotificationClose;

	public ReadOnlyReactiveProperty<bool> IsNewCase => m_IsNewCase;

	public override bool ShouldShow => true;

	public CaseNotificationVM(BlueprintCase blueprintCase, Action<CaseNotificationVM> onNotificationClose)
	{
		BlueprintCase = blueprintCase;
		Case = new NotificationCaseBodyVM(blueprintCase);
		m_OnNotificationClose = onNotificationClose;
		UpdateCanShowButton();
		EventBus.Subscribe(this).AddTo(this);
	}

	public void Close()
	{
		m_OnNotificationClose?.Invoke(this);
	}

	public void ToJournal()
	{
		BlueprintClue clueToFocus = (IsNewCase.CurrentValue ? null : Clues.FirstOrDefault()?.BlueprintClue);
		NotificationUtils.OpenDetectiveJournal(BlueprintCase, clueToFocus);
	}

	private void UpdateCanShowButton()
	{
		bool flag = RootUIContext.Instance.FullScreenUIType == FullScreenUIType.Journal;
		bool flag2 = Game.Instance.CurrentModeType == GameModeType.Cutscene;
		bool flag3 = Game.Instance.CurrentModeType == GameModeType.Dialog;
		m_CanOpenJournal.Value = !flag && !flag2 && !flag3;
	}

	public void MarkAsNew()
	{
		m_IsNewCase.Value = true;
	}

	public void AddClue(BlueprintClue clue)
	{
		NotificationClueBodyVM notificationClueBodyVM = Clues.FirstOrDefault((NotificationClueBodyVM c) => c.BlueprintClue == clue);
		if (notificationClueBodyVM == null)
		{
			notificationClueBodyVM = new NotificationClueBodyVM(clue);
			Clues.Add(notificationClueBodyVM);
		}
		notificationClueBodyVM.MarkAsNew();
	}

	public void AddAddendum(BlueprintClueAddendum addendum)
	{
		NotificationClueBodyVM notificationClueBodyVM = Clues.FirstOrDefault((NotificationClueBodyVM c) => c.BlueprintClue == addendum.ParentClue.Blueprint);
		if (notificationClueBodyVM == null)
		{
			notificationClueBodyVM = new NotificationClueBodyVM(addendum.ParentClue.Blueprint);
			Clues.Add(notificationClueBodyVM);
		}
		notificationClueBodyVM.AddAddendum(addendum);
	}

	public void RemoveAddendum(BlueprintClueAddendum addendum)
	{
		Clues.FirstOrDefault((NotificationClueBodyVM c) => c.BlueprintClue == addendum.ParentClue.Blueprint)?.RemoveAddendum(addendum);
	}

	public void HandleDialogStarted(BlueprintDialog dialog)
	{
		UpdateCanShowButton();
	}

	public void HandleDialogFinished(BlueprintDialog dialog, bool success)
	{
		UpdateCanShowButton();
	}
}
