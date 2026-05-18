using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BookEventBaseView : View<BookEventVM>, IEncyclopediaGlossaryModeHandler, ISubscriber
{
	[Header("Window")]
	[SerializeField]
	private FadeAnimator m_WindowAnimator;

	[SerializeField]
	private FadeAnimator m_ContentAnimator;

	[Header("Text page")]
	[SerializeField]
	private GameObject m_FlavorTextHeader;

	[SerializeField]
	private GameObject m_Spacer;

	[SerializeField]
	private GameObject m_CuesPanel;

	[SerializeField]
	protected BookEventCueView m_CueView;

	[SerializeField]
	protected ScrollRectExtended m_CuesScrollRect;

	[SerializeField]
	private GameObject m_AnswersPanel;

	[SerializeField]
	private BookEventAnswerPCView m_AnswerView;

	[SerializeField]
	protected ScrollRectExtended m_AnswersScrollRect;

	[Header("Picture page")]
	[SerializeField]
	private Image m_Picture;

	[SerializeField]
	private FadeAnimator m_PictureAnimator;

	[SerializeField]
	private BookEventCueColors m_Colors;

	private readonly ReactiveProperty<bool> m_IsShowHistory = new ReactiveProperty<bool>();

	private readonly List<CueVM> m_MemorizedCues = new List<CueVM>();

	private bool m_ContentRefreshing;

	private bool m_IsInit;

	private bool m_IsShown;

	private int m_LastHistoryCueIndex;

	private string m_ChoosedAnswer;

	protected readonly List<BookEventCueView> m_MemorizedCuesViews = new List<BookEventCueView>();

	protected readonly List<BookEventCueView> m_CurrentCuesViews = new List<BookEventCueView>();

	protected readonly List<BookEventAnswerPCView> m_AnswersEntities = new List<BookEventAnswerPCView>();

	protected readonly ReactiveProperty<bool> m_VotesIsActive = new ReactiveProperty<bool>();

	protected ReadOnlyReactiveProperty<bool> IsShowHistory => m_IsShowHistory;

	public virtual void Awake()
	{
		m_WindowAnimator.Initialize();
		m_ContentAnimator.Initialize();
		m_PictureAnimator.Initialize();
	}

	protected override void OnBind()
	{
		m_LastHistoryCueIndex = -1;
		Show();
		base.ViewModel.EventPicture.Select((Sprite ev) => ev != null).Subscribe(delegate
		{
			SetupPicture();
		}).AddTo(this);
		base.ViewModel.Cues.ObserveAdd().Subscribe(delegate
		{
			OnContentChanged();
		}).AddTo(this);
		base.ViewModel.Answers.Subscribe(delegate(List<AnswerVM> vms)
		{
			if (vms != null)
			{
				OnContentChanged();
			}
		}).AddTo(this);
		base.ViewModel.IsVisible.Subscribe(delegate(bool value)
		{
			if (m_IsShown != value)
			{
				if (value)
				{
					Show();
				}
				else
				{
					Hide();
				}
			}
		}).AddTo(this);
		base.ViewModel.ChoosedAnswer.Subscribe(delegate(string value)
		{
			m_ChoosedAnswer = value;
			if (!string.IsNullOrEmpty(m_ChoosedAnswer))
			{
				m_Spacer.SetActive(value: true);
				m_FlavorTextHeader.SetActive(value: false);
			}
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	private void Show()
	{
		m_VotesIsActive.Value = false;
		m_IsShown = true;
		m_WindowAnimator.AppearAnimation();
		FullScreenSounds.Instance.Dialogue.BookOpen.Play();
		m_FlavorTextHeader.SetActive(value: true);
		m_Spacer.SetActive(value: false);
	}

	private void Hide()
	{
		m_VotesIsActive.Value = false;
		m_IsShown = false;
		m_WindowAnimator.DisappearAnimation();
		FullScreenSounds.Instance.Dialogue.BookClose.Play();
	}

	private void SetCues()
	{
		m_CuesScrollRect.ScrollToTop();
		m_CurrentCuesViews.Clear();
		foreach (CueVM cue in base.ViewModel.Cues)
		{
			BookEventCueView cueView = WidgetFactory.GetWidget(m_CueView, activate: true, strictMatching: true);
			cueView.Initialize(delegate
			{
				WidgetFactory.DisposeWidget(cueView);
			}, m_Colors);
			cueView.Bind(cue);
			cueView.transform.SetParent(m_CuesPanel.transform, worldPositionStays: false);
			cueView.name = $"Cue {cueView.transform.GetSiblingIndex()}";
			cueView.m_CueGroup.DOFade(1f, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
			if (m_MemorizedCues.Any((CueVM c) => c.RawText == cue.RawText))
			{
				cueView.Shade();
			}
			m_CurrentCuesViews.Add(cueView);
		}
	}

	private void SetAnswers()
	{
		m_VotesIsActive.Value = false;
		if (base.ViewModel.Answers.CurrentValue == null)
		{
			return;
		}
		foreach (BookEventAnswerPCView answersEntity in m_AnswersEntities)
		{
			answersEntity.Unbind();
		}
		m_AnswersEntities.Clear();
		foreach (AnswerVM item in base.ViewModel.Answers.CurrentValue)
		{
			BookEventAnswerPCView widget = WidgetFactory.GetWidget(m_AnswerView, activate: true, strictMatching: true);
			widget.Bind(item);
			widget.transform.SetParent(m_AnswersPanel.transform, worldPositionStays: false);
			widget.name = $"Answer {widget.transform.GetSiblingIndex()}";
			m_AnswersEntities.Add(widget);
		}
		m_AnswersScrollRect.ScrollToTop();
	}

	private void FillHistory()
	{
		if (m_LastHistoryCueIndex != -1)
		{
			BookEventCueView cueView = WidgetFactory.GetWidget(m_CueView, activate: true, strictMatching: true);
			cueView.Initialize(delegate
			{
				WidgetFactory.DisposeWidget(cueView);
			}, m_Colors);
			cueView.name = $"Cue {cueView.transform.GetSiblingIndex()} (Answer)";
			cueView.SetText("\n<b>--- " + m_ChoosedAnswer + "</b>\n ");
			cueView.Highlight();
		}
		for (int i = m_LastHistoryCueIndex + 1; i < base.ViewModel.HistoryCues.Count; i++)
		{
			CueVM cueVM = base.ViewModel.HistoryCues[i];
			BookEventCueView cueView = WidgetFactory.GetWidget(m_CueView, activate: true, strictMatching: true);
			cueView.Initialize(delegate
			{
				WidgetFactory.DisposeWidget(cueView);
			}, m_Colors);
			cueView.Bind(cueVM);
			cueView.name = $"Cue {cueView.transform.GetSiblingIndex()}";
			cueView.m_CueGroup.DOFade(1f, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
			m_MemorizedCues.Add(cueVM);
			m_MemorizedCuesViews.Add(cueView);
			m_LastHistoryCueIndex = i;
		}
	}

	private void OnContentChanged()
	{
		if (!m_ContentRefreshing)
		{
			m_ContentRefreshing = true;
			TooltipHelper.HideTooltip();
			m_ContentAnimator.DisappearAnimation(delegate
			{
				FullScreenSounds.Instance.Dialogue.BookPageTurn.Play();
				m_ContentAnimator.AppearAnimation();
				SetCues();
				SetAnswers();
				FillHistory();
				m_ContentRefreshing = false;
			});
			m_AnswersScrollRect.ScrollToTop();
		}
	}

	private void SetupPicture()
	{
		if (!m_PictureAnimator.gameObject.activeSelf)
		{
			SetPicture();
		}
		else
		{
			m_PictureAnimator.DisappearAnimation(SetPicture);
		}
	}

	private void SetPicture()
	{
		m_Picture.sprite = base.ViewModel.EventPicture.CurrentValue;
		m_PictureAnimator.AppearAnimation();
	}

	protected virtual void CreateInputImpl()
	{
	}

	public void HandleGlossaryMode(bool state)
	{
		if (!state)
		{
			OnCloseGlossaryMode();
		}
	}

	protected virtual void OnCloseGlossaryMode()
	{
	}
}
