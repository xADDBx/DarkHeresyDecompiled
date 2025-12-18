using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class JournalQuestObjectiveBaseView : View<JournalQuestObjectiveVM>
{
	[Header("Texts")]
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_ObjectiveNumber;

	[SerializeField]
	protected TextMeshProUGUI m_Description;

	[SerializeField]
	protected TextMeshProUGUI m_EtudeCounter;

	[SerializeField]
	protected TextMeshProUGUI m_Destination;

	[SerializeField]
	protected TextMeshProUGUI m_PinButtonLabel;

	[Header("Elements")]
	[SerializeField]
	protected OwlcatMultiSelectable m_MarkSelectable;

	[SerializeField]
	protected OwlcatMultiButton m_PinButton;

	[SerializeField]
	private Image m_DestinationGeoMark;

	[SerializeField]
	private GameObject m_IsViewedMark;

	private AccessibilityTextHelper m_AccessibilityTextHelper;

	protected override void OnBind()
	{
		SetupState();
		SetTextFontSize();
		m_PinButton.gameObject.SetActive(base.ViewModel.CanBePinned);
		m_PinButtonLabel.text = UIStrings.Instance.QuestNotificationTexts.PinQuestLabel.Text;
	}

	private void SetTextFontSize()
	{
		if (m_AccessibilityTextHelper == null)
		{
			m_AccessibilityTextHelper = new AccessibilityTextHelper(m_Description, m_EtudeCounter, m_Destination, m_PinButtonLabel).AddTo(this);
		}
		m_AccessibilityTextHelper.UpdateTextSize();
	}

	public virtual void SetupHeader()
	{
		m_Title.text = base.ViewModel.Title;
		SetupBody();
	}

	private void SetupBody()
	{
		bool flag = !string.IsNullOrWhiteSpace(base.ViewModel.Description);
		bool flag2 = !string.IsNullOrWhiteSpace(base.ViewModel.Destination);
		m_Destination.transform.parent.gameObject.SetActive(flag2);
		m_Description.text = (flag ? base.ViewModel.Description : string.Empty);
		bool hasEtudeCounter = base.ViewModel.HasEtudeCounter;
		m_EtudeCounter.gameObject.SetActive(hasEtudeCounter);
		if (hasEtudeCounter)
		{
			m_EtudeCounter.text = $"{base.ViewModel.CurrentEtudeCounter}/{base.ViewModel.MinEtudeCounter} {base.ViewModel.EtudeCounterDescription}";
		}
		m_Destination.text = (flag2 ? base.ViewModel.Destination : string.Empty);
		if (flag2)
		{
			m_Destination.SetTooltip(new TooltipTemplateGlobalMapPosition(), new TooltipConfig(InfoCallPCMethod.None, InfoCallConsoleMethod.None)).AddTo(this);
		}
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.UpdateStatus, delegate
		{
			SetupState();
		}).AddTo(this);
	}

	protected virtual void SetupState()
	{
		m_IsViewedMark.SetActive(!base.ViewModel.IsViewed);
		if (base.ViewModel.IsFailed)
		{
			m_MarkSelectable.SetActiveLayer("Failed");
		}
		else if (base.ViewModel.IsCompleted)
		{
			m_MarkSelectable.SetActiveLayer("Completed");
		}
		else if (!base.ViewModel.IsViewed && !base.ViewModel.IsFailed && !base.ViewModel.IsCompleted && !base.ViewModel.IsPostponed)
		{
			m_MarkSelectable.SetActiveLayer("Attention");
		}
		else if (base.ViewModel.IsPostponed)
		{
			m_MarkSelectable.SetActiveLayer("Postponed");
		}
		else
		{
			m_MarkSelectable.SetActiveLayer("None");
		}
	}

	protected string GetHintText()
	{
		UIQuestNotificationTexts questNotificationTexts = UIStrings.Instance.QuestNotificationTexts;
		JournalQuestObjectiveVM viewModel = base.ViewModel;
		if (viewModel != null)
		{
			if (!viewModel.IsFailed)
			{
				if (!viewModel.IsCompleted)
				{
					if (!viewModel.IsViewed)
					{
						if (!viewModel.IsPostponed)
						{
							return questNotificationTexts.QuestNew;
						}
					}
					else if (!viewModel.IsPostponed)
					{
						return questNotificationTexts.QuestStarted;
					}
					return questNotificationTexts.QuestPostponed;
				}
				return questNotificationTexts.QuestComplite;
			}
			return questNotificationTexts.QuestFailed;
		}
		return string.Empty;
	}
}
