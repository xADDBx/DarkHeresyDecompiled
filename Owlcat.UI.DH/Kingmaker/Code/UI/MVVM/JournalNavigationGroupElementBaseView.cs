using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Root;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class JournalNavigationGroupElementBaseView : View<JournalQuestVM>, IQuestEntity, IConsoleNavigationEntity, IConsoleEntity, IUpdateCanCompleteOrderNotificationHandler, ISubscriber
{
	[Space]
	[SerializeField]
	[UsedImplicitly]
	protected OwlcatMultiButton m_MultiButton;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private bool m_SetLabelBold;

	[Header("Completion")]
	[SerializeField]
	[UsedImplicitly]
	private Image m_StatusImage;

	[SerializeField]
	private Sprite m_UpdatedPaperMark;

	[SerializeField]
	private Sprite m_NewQuestPaperMark;

	[SerializeField]
	private Sprite m_NewOrderPaperMark;

	[SerializeField]
	private Sprite m_NewRumourPaperMark;

	[SerializeField]
	private Sprite m_CompletedPaperMark;

	[SerializeField]
	private Sprite m_FailedPaperMark;

	[SerializeField]
	private Sprite m_PostponedPaperMark;

	[SerializeField]
	private Sprite m_AlarmPaperMark;

	[SerializeField]
	private RectTransform m_ReadyToCompleteImage;

	private bool m_SelectNow;

	public bool IsActive => base.ViewModel?.IsActive ?? false;

	public bool IsSelected => base.ViewModel.IsSelected.CurrentValue;

	public Quest Quest => base.ViewModel.Quest;

	protected override void OnBind()
	{
		m_Label.text = base.ViewModel.Title;
		base.ViewModel.IsSelected.Subscribe(OnSelected).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_MultiButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.SelectQuest();
		}).AddTo(this);
		base.ViewModel.IsOrderCompleted.Subscribe(delegate
		{
			SetupStatusMark();
			OnSelected(base.ViewModel.IsSelected.CurrentValue);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.UpdateStatusCommand, delegate
		{
			SetupStatusMark();
		}).AddTo(this);
		SetupStatusMark();
		EventBus.Subscribe(this).AddTo(this);
	}

	private void SetupStatusMark()
	{
		m_StatusImage.sprite = GetPaperStatusSprite();
		UpdateReadyToComplete();
	}

	private Sprite GetPaperStatusSprite()
	{
		if (!base.ViewModel.IsViewed)
		{
			return m_NewQuestPaperMark;
		}
		JournalQuestVM viewModel = base.ViewModel;
		if (viewModel != null)
		{
			if (viewModel.IsNew)
			{
				if (viewModel.IsRumour)
				{
					if (!viewModel.QuestIsViewed)
					{
						return m_NewRumourPaperMark;
					}
				}
				else if (viewModel.IsOrder)
				{
					if (!viewModel.QuestIsViewed)
					{
						return m_NewOrderPaperMark;
					}
				}
				else if (!viewModel.QuestIsViewed)
				{
					return m_NewQuestPaperMark;
				}
				if (viewModel.IsUpdated)
				{
					goto IL_00bc;
				}
				if (viewModel.IsCompleted)
				{
					goto IL_00c5;
				}
				if (viewModel.IsFailed)
				{
					goto IL_00ce;
				}
				if (viewModel.IsPostponed)
				{
					goto IL_00d7;
				}
				if (viewModel.IsViewed)
				{
					return UIConfig.Instance.TransparentImage;
				}
			}
			else
			{
				if (viewModel.IsUpdated)
				{
					goto IL_00bc;
				}
				if (viewModel.IsCompleted)
				{
					goto IL_00c5;
				}
				if (viewModel.IsFailed)
				{
					goto IL_00ce;
				}
				if (viewModel.IsPostponed)
				{
					goto IL_00d7;
				}
			}
		}
		return null;
		IL_00bc:
		return m_UpdatedPaperMark;
		IL_00ce:
		return m_FailedPaperMark;
		IL_00c5:
		return m_CompletedPaperMark;
		IL_00d7:
		return m_PostponedPaperMark;
	}

	private void OnSelected(bool value)
	{
		m_MultiButton.SetActiveLayer(value ? "On" : "Off");
		if (m_SetLabelBold)
		{
			m_Label.fontStyle = (value ? FontStyles.Bold : FontStyles.Normal);
		}
		if (value)
		{
			m_SelectNow = true;
		}
		if (!value && m_SelectNow)
		{
			foreach (QuestObjective objective in Quest.Objectives)
			{
				if (!objective.IsVisible || objective.State == QuestObjectiveState.None || objective.Blueprint.IsAddendum)
				{
					continue;
				}
				objective.NeedToAttention = false;
				IOrderedEnumerable<QuestObjective> orderedEnumerable = (from b in objective?.Blueprint?.Addendums?.Where((BlueprintQuestObjective b) => b != null)
					select objective?.Quest?.TryGetObjective(b) into a
					where a != null
					where a.IsVisible
					orderby a?.Order descending
					select a);
				if (orderedEnumerable == null)
				{
					continue;
				}
				foreach (QuestObjective item in orderedEnumerable)
				{
					item.NeedToAttention = false;
				}
			}
			m_SelectNow = false;
		}
		SetupStatusMark();
	}

	public void SetFocus(bool value)
	{
		m_MultiButton.SetFocus(value);
		m_MultiButton.SetActiveLayer(value ? "Selected" : "Normal");
	}

	public bool IsValid()
	{
		return m_MultiButton.IsValid();
	}

	public void HandleUpdateCanCompleteOrderNotificationInJournal()
	{
		UpdateReadyToComplete();
	}

	private void UpdateReadyToComplete()
	{
		bool active = base.ViewModel.IsNew || base.ViewModel.IsUpdated || base.ViewModel.IsCompleted || base.ViewModel.IsFailed;
		m_ReadyToCompleteImage.gameObject.SetActive(active);
	}
}
