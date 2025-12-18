using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.HUDNotification.New.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class CaseNotificationView : View<CaseNotificationVM>, INotificationView, IFullScreenUIHandler, ISubscriber
{
	[Header("Views")]
	[SerializeField]
	private NotificationCaseBodyView CaseBodyView;

	[SerializeField]
	private NotificationClueBodyView m_ClueBodyPrefab;

	[Header("Animators")]
	[SerializeField]
	private MoveAnimator m_MoveAnimator;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_CaseName;

	[SerializeField]
	private TMP_Text m_CaseNamePrefix;

	[SerializeField]
	private LayoutElement m_CluesScroll;

	[SerializeField]
	private WidgetList m_CluesList;

	[SerializeField]
	private OwlcatMultiSelectable m_PointerBlocker;

	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	[SerializeField]
	private OwlcatMultiButton m_ToJournalButton;

	[SerializeField]
	private OwlcatMultiSelectable m_ViewSelectable;

	[SerializeField]
	private TMP_Text m_ToJournalButtonText;

	[Header("Values")]
	[SerializeField]
	private int m_MaxCluesCount = 3;

	private IDisposable m_HideDisposable;

	public bool IsEmpty => base.ViewModel == null;

	private void Awake()
	{
		m_MoveAnimator.Initialize();
		m_FadeAnimator.Initialize();
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_MoveAnimator.AppearAnimation();
		m_FadeAnimator.AppearAnimation();
		base.ViewModel.IsNewCase.Subscribe(delegate(bool value)
		{
			CaseBodyView.gameObject.SetActive(value && base.ViewModel.BlueprintCase != null);
		}).AddTo(this);
		CaseBodyView.Bind(base.ViewModel.Case);
		m_CaseNamePrefix.text = UIStrings.Instance.CaseNotificationTexts.CasePrefix.Text;
		m_CaseName.text = ((base.ViewModel.BlueprintCase == null) ? UIStrings.Instance.DetectiveJournal.UnknownCluesHeader.Text : base.ViewModel.BlueprintCase.Name.Text);
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			Hide();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ToJournalButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ToJournal();
		}).AddTo(this);
		m_ToJournalButtonText.text = UIStrings.Instance.CaseNotificationTexts.ToDetectiveJournal.Text;
		SetupHideTimer();
		EventBus.Subscribe(this).AddTo(this);
		m_PointerBlocker.OnPointerEnterAsObservable().Subscribe(delegate
		{
			m_HideDisposable?.Dispose();
		}).AddTo(this);
		m_PointerBlocker.OnPointerExitAsObservable().Subscribe(delegate
		{
			SetupHideTimer();
		}).AddTo(this);
		DrawClues();
		SetScrollHeight();
		string activeLayer = ((!base.ViewModel.IsNewCase.CurrentValue && base.ViewModel.Clues.Count == 1) ? "SingleClue" : "Default");
		m_ViewSelectable.SetActiveLayer(activeLayer);
		GetSound()?.Play();
	}

	private void DrawClues()
	{
		m_CluesList.Clear();
		m_CluesList.DrawEntries(base.ViewModel.Clues, m_ClueBodyPrefab);
		m_CluesScroll.gameObject.SetActive(base.ViewModel.Clues.Count > 0);
	}

	private void SetScrollHeight()
	{
		RectTransform component = m_CluesList.Container.GetComponent<RectTransform>();
		VerticalLayoutGroup component2 = component.GetComponent<VerticalLayoutGroup>();
		LayoutRebuilder.ForceRebuildLayoutImmediate(component);
		int val = (base.ViewModel.IsNewCase.CurrentValue ? m_MaxCluesCount : (m_MaxCluesCount + 1));
		val = Math.Min(val, base.ViewModel.Clues.Count);
		float num = (float)(component2.padding.top + component2.padding.bottom) + component2.spacing * (float)(val - 1);
		if (base.ViewModel.Clues.Count > val)
		{
			num += 20f;
		}
		for (int i = 0; i < val; i++)
		{
			num += component.GetChild(i).GetComponent<RectTransform>().rect.height;
		}
		m_CluesScroll.minHeight = num;
	}

	private void SetupHideTimer()
	{
		m_HideDisposable?.Dispose();
		m_HideDisposable = NotificationUtils.DoActionAfterDelay(NotificationUtils.Time, Hide).AddTo(this);
	}

	public void Hide()
	{
		m_MoveAnimator.DisappearAnimation();
		NotificationUtils.DoActionAfterDelay(0.3f, delegate
		{
			m_FadeAnimator.DisappearAnimation();
		}).AddTo(this);
		NotificationUtils.DoActionAfterDelay(m_MoveAnimator.AnimationTime, delegate
		{
			base.ViewModel?.Close();
		}).AddTo(this);
	}

	private BlueprintUISound.UISound GetSound()
	{
		BlueprintUISound.UISoundNotifications notifications = UISounds.Instance.Sounds.Notifications;
		if (!base.ViewModel.IsNewCase.CurrentValue)
		{
			return notifications.NewDetectiveInformation;
		}
		return notifications.NewDetectiveCase;
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		if (state)
		{
			Hide();
		}
	}
}
