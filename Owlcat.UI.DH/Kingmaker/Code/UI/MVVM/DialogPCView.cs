using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

[ViewFactoryPolicy(ViewFactoryPolicyFlag.DontPool, null)]
public class DialogPCView : View<DialogVM>
{
	[Header("Answer Block")]
	[SerializeField]
	private GameObject m_AnswererPortraitPlace;

	[SerializeField]
	private TextMeshProUGUI m_AnswererName;

	[SerializeField]
	private Image m_AnswererPortrait;

	[SerializeField]
	private OwlcatMultiSelectable m_AnswererPortraitSelectable;

	[Header("Speaker Block")]
	[SerializeField]
	private GameObject m_SpeakerPortraitPlace;

	[SerializeField]
	private TextMeshProUGUI m_SpeakerName;

	[SerializeField]
	private Image m_SpeakerPortrait;

	[SerializeField]
	private FadeAnimator m_SpeakerFadeAnimator;

	[SerializeField]
	private MoveAnimator m_SpeakerMoveAnimator;

	[SerializeField]
	private OwlcatMultiSelectable m_SpeakerPortraitSelectable;

	[Header("Scroll")]
	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

	[SerializeField]
	private CanvasGroup m_ScrollRectContentCanvasGroup;

	[SerializeField]
	private OwlcatMultiButton m_scrollToBottomButton;

	private Sequence m_ScrollAnimationSequence;

	[Header("Cue Block")]
	[SerializeField]
	private DialogCuePCView m_CueView;

	[Header("Cue Notify Answer Block")]
	[SerializeField]
	private RectTransform CueNotifyAnswerPlace;

	[Header("Answer Block")]
	[SerializeField]
	private WidgetList m_AnswersWidgetList;

	[SerializeField]
	private DialogAnswerPCView m_AnswerView;

	[SerializeField]
	private DialogSystemAnswerPCView m_SystemAnswerPCView;

	[SerializeField]
	private CanvasGroup m_AnswersCanvasGroup;

	[Header("History Block")]
	[SerializeField]
	private TMP_Text m_ProtocolLabel;

	[SerializeField]
	private DialogHistoryEntityView m_HistoryEntityView;

	[SerializeField]
	private RectTransform m_HistoryContainer;

	[SerializeField]
	private CanvasGroup m_HistoryCanvasGroup;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_HistoryHiddenAlpha = 0.5f;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_HistoryShowedAlpha = 1f;

	[Header("Notification Block")]
	[SerializeField]
	private DialogNotificationsView m_DialogNotifications;

	[Header("Cue Notify Answer Block")]
	[SerializeField]
	private RectTransform m_CueNotifyAnswerPlace;

	[Header("Common Block")]
	[SerializeField]
	private List<FadeAnimator> m_FadeAnimators;

	[SerializeField]
	private List<MoveAnimator> m_MoveAnimators;

	[SerializeField]
	private FadeAnimator m_DownPortraitAnimator;

	[Header("Colors")]
	[SerializeField]
	private DialogCueColors m_DialogCueColors;

	[Header("Tooltip")]
	[SerializeField]
	private TooltipConfig m_TooltipPlace;

	private readonly List<DialogHistoryEntityView> m_HistoryViewEntities = new List<DialogHistoryEntityView>();

	private bool m_VisibleState;

	private bool m_IsAnimating;

	private IDisposable m_SetVisibleTask;

	private IDisposable m_PortraitsAnimationTask;

	private const int PaddingForAnimations = 40;

	[Header("Big Portrait")]
	[SerializeField]
	private DialogPortraitView m_AnswererBigPortraitView;

	[SerializeField]
	private DialogPortraitView m_SpeakerBigPortraitView;

	public void Awake()
	{
		m_SystemAnswerPCView.Initialize();
		m_CueView.Initialize(null, m_DialogCueColors, m_TooltipPlace);
		m_FadeAnimators.ForEach(delegate(FadeAnimator a)
		{
			a.Initialize();
		});
		m_MoveAnimators.ForEach(delegate(MoveAnimator a)
		{
			a.Initialize();
		});
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.ViewModel.AnswerName.Subscribe(delegate(string value)
		{
			m_AnswererName.gameObject.SetActive(value != null);
			if (value != null)
			{
				m_AnswererName.text = value;
			}
		}).AddTo(this);
		base.ViewModel.AnswerPortrait.Subscribe(delegate(Sprite value)
		{
			m_AnswererPortraitPlace.gameObject.SetActive(value != null);
			if (value != null)
			{
				m_AnswererPortrait.sprite = value;
			}
		}).AddTo(this);
		base.ViewModel.SpeakerName.Subscribe(delegate(string value)
		{
			m_SpeakerName.gameObject.SetActive(value != null);
			if (value != null)
			{
				m_SpeakerName.text = value;
			}
		}).AddTo(this);
		base.ViewModel.SpeakerPortrait.Subscribe(delegate(Sprite value)
		{
			m_SpeakerPortraitPlace.gameObject.SetActive(value != null);
			if (value != null)
			{
				m_SpeakerPortrait.sprite = value;
			}
		}).AddTo(this);
		m_AnswererPortraitSelectable.OnHoverAsObservable().Subscribe(base.ViewModel.ShowHideBigScreenshotAnswerer).AddTo(this);
		m_SpeakerPortraitSelectable.OnHoverAsObservable().Subscribe(base.ViewModel.ShowHideBigScreenshotSpeaker).AddTo(this);
		base.ViewModel.SpeakerFullPortraitVM.Subscribe(m_SpeakerBigPortraitView.Bind).AddTo(this);
		base.ViewModel.AnswererFullPortraitVM.Subscribe(m_AnswererBigPortraitView.Bind).AddTo(this);
		m_DialogNotifications.Bind(base.ViewModel.DialogNotifications);
		base.ViewModel.Cue.Subscribe(m_CueView.Bind).AddTo(this);
		m_ProtocolLabel.Or(null)?.SetText(UIStrings.Instance.Dialog.ProtocolLabel.Text);
		foreach (DialogHistoryEntityVM item in base.ViewModel.History)
		{
			AddHistoryView(item, withTweenToItem: false);
		}
		base.ViewModel.History.ObserveAdd().Subscribe(delegate(CollectionAddEvent<DialogHistoryEntityVM> historyEntity)
		{
			AddHistoryView(historyEntity.Value);
		}).AddTo(this);
		base.ViewModel.Answers.Subscribe(delegate
		{
			DrawAnswers();
		}).AddTo(this);
		base.ViewModel.SystemAnswer.Subscribe(m_SystemAnswerPCView.Bind).AddTo(this);
		m_ScrollRect.verticalScrollbar.OnValueChangedAsObservable().Subscribe(delegate
		{
			m_HistoryCanvasGroup.alpha = (m_ScrollRect.BottomEdgeNeeded ? m_HistoryShowedAlpha : m_HistoryHiddenAlpha);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_scrollToBottomButton.OnLeftClickAsObservable(), delegate
		{
			PlayScrollDownAnimation();
		}).AddTo(this);
		base.ViewModel.IsVisible.Subscribe(SetVisible).AddTo(this);
		base.ViewModel.OnCueUpdate.Subscribe(StartUpdateCoroutine).AddTo(this);
		base.ViewModel.EmptySpeaker.Subscribe(UpdateSpeakerPortrait).AddTo(this);
		base.ViewModel.OnDetachView.Subscribe(Unbind).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnUnbind()
	{
		SetVisible(state: false);
		OwlcatR3UnitExtensions.Subscribe(Observable.Timer(Mathf.Max(m_FadeAnimators.Select((FadeAnimator f) => f.DisappearTime).ToArray()).Seconds()), delegate
		{
			m_HistoryViewEntities.ForEach(WidgetFactory.DisposeWidget);
			m_HistoryViewEntities.Clear();
		}).AddTo(this);
		m_PortraitsAnimationTask?.Dispose();
		m_ScrollAnimationSequence?.Kill();
		m_ScrollAnimationSequence = null;
	}

	private void AddHistoryView(DialogHistoryEntityVM vm, bool withTweenToItem = true)
	{
		DialogHistoryEntityView widget = WidgetFactory.GetWidget(m_HistoryEntityView, activate: true, strictMatching: true);
		widget.Initialize(m_DialogCueColors, m_TooltipPlace);
		widget.transform.SetParent(m_HistoryContainer, worldPositionStays: false);
		widget.Bind(vm);
		m_HistoryViewEntities.Add(widget);
		if (withTweenToItem)
		{
			TweenCueHistoryPosition(widget);
		}
	}

	private void DrawAnswers()
	{
		if (base.ViewModel.Answers.CurrentValue == null)
		{
			m_AnswersWidgetList.Clear();
		}
		else
		{
			m_AnswersWidgetList.DrawEntries(base.ViewModel.Answers.CurrentValue, m_AnswerView).AddTo(this);
		}
	}

	public void StartUpdateCoroutine()
	{
	}

	private void UpdateSpeakerPortrait(bool isEmpty)
	{
		m_SpeakerFadeAnimator.PlayAnimation(!isEmpty);
		m_SpeakerMoveAnimator.PlayAnimation(!isEmpty);
	}

	private void TweenCueHistoryPosition(DialogHistoryEntityView item)
	{
		CanvasGroup itemCanvasGroup;
		LayoutElement itemLayoutElement;
		if (!m_IsAnimating)
		{
			m_IsAnimating = true;
			m_AnswersCanvasGroup.alpha = 0f;
			itemCanvasGroup = EnsureComponent<CanvasGroup>(item.gameObject);
			itemCanvasGroup.alpha = 0f;
			itemLayoutElement = EnsureComponent<LayoutElement>(item.gameObject);
			Move();
		}
		void Move()
		{
			float preferredHeight = LayoutUtility.GetPreferredHeight(m_CueView.transform as RectTransform);
			itemLayoutElement.preferredHeight = preferredHeight;
			itemLayoutElement.preferredHeight += 40f;
			VerticalLayoutGroup historyVerticalGroup = EnsureComponent<VerticalLayoutGroup>(m_HistoryContainer.gameObject);
			int bottom = historyVerticalGroup.padding.bottom;
			float num = preferredHeight + (float)historyVerticalGroup.padding.top + historyVerticalGroup.spacing;
			float num2 = ((float)bottom + num) * -1f;
			historyVerticalGroup.padding.bottom = (int)num2;
			VerticalLayoutGroup cueNotifyAnswerVerticalGroup = EnsureComponent<VerticalLayoutGroup>(m_CueNotifyAnswerPlace.gameObject);
			int top = cueNotifyAnswerVerticalGroup.padding.top;
			cueNotifyAnswerVerticalGroup.padding.top += (int)num + 40;
			m_ScrollAnimationSequence?.Kill();
			m_ScrollAnimationSequence = DOTween.Sequence().SetTarget(item).SetUpdate(isIndependentUpdate: true);
			m_ScrollAnimationSequence.Append(DOTween.To(() => historyVerticalGroup.padding.bottom, delegate(int value)
			{
				historyVerticalGroup.padding.bottom = value;
			}, bottom, 0.1f));
			m_ScrollAnimationSequence.Join(DOTween.To(() => cueNotifyAnswerVerticalGroup.padding.top, delegate(int value)
			{
				cueNotifyAnswerVerticalGroup.padding.top = value;
			}, top, 0.1f));
			m_ScrollAnimationSequence.Join(itemCanvasGroup.DOFade(1f, 0.1f));
			m_ScrollAnimationSequence.Append(DOTween.To(() => itemLayoutElement.preferredHeight, delegate(float value)
			{
				itemLayoutElement.preferredHeight = value;
			}, preferredHeight, 0.15f));
			m_ScrollAnimationSequence.OnComplete(OnHistoryAnimationComplete);
		}
	}

	private T EnsureComponent<T>(GameObject obj) where T : Component
	{
		T component = obj.GetComponent<T>();
		if (!(component != null))
		{
			return obj.AddComponent<T>();
		}
		return component;
	}

	private void SetVisible(bool state)
	{
		if (m_VisibleState == state)
		{
			return;
		}
		m_SetVisibleTask?.Dispose();
		m_VisibleState = state;
		if (m_VisibleState)
		{
			m_IsAnimating = false;
			base.gameObject.SetActive(value: true);
			UpdateSpeakerPortrait(base.ViewModel.EmptySpeaker.CurrentValue);
		}
		else
		{
			m_SpeakerFadeAnimator.DisappearAnimation();
			m_SpeakerMoveAnimator.DisappearAnimation();
			m_DownPortraitAnimator?.DisappearAnimation();
			TooltipHelper.HideTooltip();
		}
		float value = Mathf.Max(m_MoveAnimators.Select((MoveAnimator f) => f.AnimationTime).ToArray());
		m_PortraitsAnimationTask = OwlcatR3UnitExtensions.Subscribe(Observable.Timer(value.Seconds()), delegate
		{
			if (base.ViewModel != null)
			{
				m_DownPortraitAnimator?.PlayAnimation(m_VisibleState);
			}
		}).AddTo(this);
		m_FadeAnimators.ForEach(delegate(FadeAnimator a)
		{
			a.PlayAnimation(m_VisibleState);
		});
		m_MoveAnimators.ForEach(delegate(MoveAnimator a)
		{
			a.PlayAnimation(m_VisibleState);
		});
		(state ? FullScreenSounds.Instance.Dialogue.Open : FullScreenSounds.Instance.Dialogue.Close).Play();
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			SetVisible(state: false);
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			SetVisible(state: true);
		}
	}

	private void OnHistoryAnimationComplete()
	{
		float num = m_ScrollRect.content.rect.height - m_ScrollRect.viewport.rect.height;
		float num2 = CueNotifyAnswerPlace.anchoredPosition.y + CueNotifyAnswerPlace.rect.height * 0.5f;
		float value = ((num <= 0f) ? 0f : (1f + num2 / num));
		value = Mathf.Clamp(value, 0f, 1f);
		m_ScrollAnimationSequence?.Kill();
		m_ScrollAnimationSequence = DOTween.Sequence().SetUpdate(isIndependentUpdate: true);
		m_ScrollAnimationSequence.Join(m_AnswersCanvasGroup.DOFade(1f, 0.1f));
		PlayScrollAnimation(value);
		m_ScrollAnimationSequence.OnComplete(delegate
		{
			m_IsAnimating = false;
		});
	}

	private void PlayScrollDownAnimation()
	{
		m_ScrollAnimationSequence?.Kill();
		m_ScrollAnimationSequence = DOTween.Sequence().SetUpdate(isIndependentUpdate: true);
		PlayScrollAnimation(0f);
	}

	private void PlayScrollAnimation(float verticalPosition)
	{
		m_ScrollAnimationSequence.Append(DOTween.To(() => m_ScrollRect.verticalNormalizedPosition, delegate(float value)
		{
			m_ScrollRect.verticalNormalizedPosition = value;
		}, verticalPosition, 0.15f));
	}
}
