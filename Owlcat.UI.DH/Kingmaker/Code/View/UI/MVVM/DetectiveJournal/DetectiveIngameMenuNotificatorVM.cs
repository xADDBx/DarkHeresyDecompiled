using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class DetectiveIngameMenuNotificatorVM : ViewModel, ICaseStatusChanged, ISubscriber, IClueStatusChanged, IClueAddendumStatusChanged, IFullScreenUIHandler
{
	private readonly ReactiveProperty<BlueprintCase> m_OperationCase = new ReactiveProperty<BlueprintCase>();

	private readonly ReactiveProperty<DetectiveBadgeType> m_BadgeType = new ReactiveProperty<DetectiveBadgeType>();

	private bool m_ShouldUpdate;

	public ReadOnlyReactiveProperty<BlueprintCase> OperationCase => m_OperationCase;

	public ReadOnlyReactiveProperty<DetectiveBadgeType> BadgeType => m_BadgeType;

	public ReadOnlyReactiveProperty<bool> IsInCombat => GameUIState.Instance.IsInCombat;

	public DetectiveIngameMenuNotificatorVM()
	{
		m_ShouldUpdate = true;
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate), delegate
		{
			if (m_ShouldUpdate)
			{
				UpdateBadge();
				m_ShouldUpdate = false;
			}
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	private void UpdateBadge()
	{
		m_OperationCase.Value = GetFirstNonClosedCaseWithNewClues();
		if (m_OperationCase.Value != null)
		{
			m_BadgeType.Value = DetectiveBadgeType.NewClues;
			return;
		}
		m_OperationCase.Value = GetFirstNonClosedCaseWithNewAddendums();
		if (m_OperationCase.Value != null)
		{
			m_BadgeType.Value = DetectiveBadgeType.NewAddendums;
			return;
		}
		m_OperationCase.Value = GetFirstNonClosedCaseWithNewStudy();
		if (m_OperationCase.Value != null)
		{
			m_BadgeType.Value = DetectiveBadgeType.NewStudies;
			return;
		}
		m_OperationCase.Value = GetFirstNonClosedCaseWithNewConclusion();
		if (m_OperationCase.Value != null)
		{
			m_BadgeType.Value = DetectiveBadgeType.NewConnections;
		}
		else if (UIUtilityDetective.HasUnknownClues())
		{
			m_OperationCase.Value = null;
			m_BadgeType.Value = DetectiveBadgeType.NewClues;
		}
		else
		{
			m_BadgeType.Value = DetectiveBadgeType.None;
		}
	}

	private BlueprintCase GetFirstNonClosedCaseWithNewClues()
	{
		return UIUtilityDetective.Detective.GetCasesWithStatus(CaseStatus.Opened).FirstOrDefault(UIUtilityDetective.HasNewClues);
	}

	public void OnBadgeClick()
	{
		if (m_OperationCase.Value == null)
		{
			if (UIUtilityDetective.HasUnknownClues())
			{
				EventBus.RaiseEvent(delegate(IDetectiveJournalUIHandler h)
				{
					h.HandleUnknownClues();
				});
			}
			else
			{
				EventBus.RaiseEvent(delegate(IDetectiveJournalUIHandler h)
				{
					h.HandleOpenDetectiveJournal(null);
				});
			}
			return;
		}
		BlueprintClue operationCaseItem = null;
		if (UIConfig.Instance.DetectiveConfig.MoveToNewClue && (!UIUtilityDetective.ExaminedDetectiveData.ExaminedCases.IsEntityNew(m_OperationCase.Value) || m_OperationCase.Value.IsUnknown()))
		{
			operationCaseItem = UIUtilityDetective.GetNewClues(m_OperationCase.Value).FirstOrDefault();
		}
		EventBus.RaiseEvent(delegate(IDetectiveJournalUIHandler h)
		{
			h.HandleOpenDetectiveJournal(m_OperationCase.Value, operationCaseItem);
		});
	}

	private BlueprintCase GetFirstNonClosedCaseWithNewAddendums()
	{
		return UIUtilityDetective.Detective.GetCasesWithStatus(CaseStatus.Opened).FirstOrDefault(UIUtilityDetective.HasNewAddendums);
	}

	private BlueprintCase GetFirstNonClosedCaseWithNewStudy()
	{
		return UIUtilityDetective.Detective.GetCasesWithStatus(CaseStatus.Opened).FirstOrDefault(UIUtilityDetective.HasNewStudies);
	}

	private BlueprintCase GetFirstNonClosedCaseWithNewConclusion()
	{
		return UIUtilityDetective.Detective.GetCasesWithStatus(CaseStatus.Opened).FirstOrDefault(UIUtilityDetective.HasNewConclusions);
	}

	public void HandleCaseStatusChanged(BlueprintCase blueprint)
	{
		m_ShouldUpdate = true;
	}

	public void HandleClueStatusChanged(BlueprintClue blueprint)
	{
		m_ShouldUpdate = true;
	}

	public void HandleClueAddendumStatusChanged(BlueprintClueAddendum blueprint)
	{
		m_ShouldUpdate = true;
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		if (!state)
		{
			m_ShouldUpdate = true;
		}
	}
}
