using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Components;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.UI.MVVM.DetectiveJournal;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.Sound;
using ObservableCollections;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.View.UI.MVVM.Dialog;

public class DetectiveEpilogBaseView : View<DetectiveEpilogVM>
{
	[Header("Detective")]
	[SerializeField]
	private CaseCardBaseView m_CaseCard;

	[SerializeField]
	private DetectivePaperReportView m_PaperReport;

	[SerializeField]
	private EpilogTrueAnswerView m_TrueAnswerEntityView;

	[SerializeField]
	private OwlcatMultiButton m_ShowTrueAnswerButton;

	[SerializeField]
	private TMP_Text m_ShowTrueAnswerButtonLabel;

	[SerializeField]
	private TMP_Text m_ReportWithNumber;

	[SerializeField]
	private TextStyle m_TextStyle;

	[Header("Window")]
	[SerializeField]
	private FadeAnimator m_WindowAnimator;

	[SerializeField]
	private FadeAnimator m_ContentAnimator;

	[Header("Text page")]
	[SerializeField]
	private GameObject m_CuesPanel;

	[SerializeField]
	protected BookEventCueView m_CueView;

	[SerializeField]
	protected ScrollRectExtended m_CuesScrollRect;

	[SerializeField]
	protected OwlcatMultiButton m_AnswerButton;

	[SerializeField]
	private TMP_Text m_AnswerButtonLabel;

	[SerializeField]
	private TMP_Text m_BottomDecorText;

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

	private IDisposable m_ChooseAnswerDisposable;

	protected ReadOnlyReactiveProperty<bool> IsShowHistory => m_IsShowHistory;

	public virtual void Awake()
	{
		m_WindowAnimator.Initialize();
		m_ContentAnimator.Initialize();
		m_PictureAnimator.Initialize();
	}

	protected override void OnBind()
	{
		base.ViewModel.DetectiveCasePage.Subscribe(delegate(DetectiveCasePage value)
		{
			if (value == null)
			{
				return;
			}
			using (GameLogContext.Scope)
			{
				GameLogContext.TextStyle = m_TextStyle;
				GameLogContext.Case = base.ViewModel.DetectiveCasePage.CurrentValue?.BlueprintCase.MaybeBlueprint;
				m_ReportWithNumber.text = UIStrings.Instance.DetectiveDecor.ReportNumberLabel.Text;
				GameLogContext.TextStyle = UIConfig.Instance.DefaultTextStyle;
			}
		}).AddTo(this);
		base.ViewModel.TrueAnswerVM.Subscribe(delegate(EpilogTrueAnswerVM value)
		{
			bool flag = value != null;
			m_ShowTrueAnswerButtonLabel.text = (flag ? UIStrings.Instance.CommonTexts.Back : UIStrings.Instance.BookEvent.ToArchives);
			m_ShowTrueAnswerButton.SetActiveLayer(flag ? 1 : 0);
		}).AddTo(this);
		m_ShowTrueAnswerButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.ToggleTrueAnswer).AddTo(this);
		base.ViewModel.IsLastAnswer.Subscribe(delegate(bool value)
		{
			m_ShowTrueAnswerButton.Interactable = value;
		}).AddTo(this);
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
		}).AddTo(this);
		base.ViewModel.DetectiveCasePage.Subscribe(delegate(DetectiveCasePage value)
		{
			if (value?.BlueprintCase != null)
			{
				m_CaseCard.Bind(new CaseCardVM(value.BlueprintCase));
			}
		}).AddTo(this);
		m_CaseCard.transform.SetAsLastSibling();
		base.ViewModel.ReportVM.Subscribe(m_PaperReport.Bind).AddTo(this);
		base.ViewModel.TrueAnswerVM.Subscribe(m_TrueAnswerEntityView.Bind).AddTo(this);
		m_BottomDecorText.text = UIStrings.Instance.DetectiveDecor.ReportDecorSignature.Text;
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	private void Show()
	{
		m_IsShown = true;
		m_WindowAnimator.AppearAnimation();
		FullScreenSounds.Instance.Dialogue.BookOpen.Play();
	}

	private void Hide()
	{
		m_IsShown = false;
		m_WindowAnimator.DisappearAnimation();
		FullScreenSounds.Instance.Dialogue.BookClose.Play();
	}

	private void SetCues()
	{
		m_CuesScrollRect.ScrollToTop();
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
		}
	}

	private void SetAnswers()
	{
		if (base.ViewModel.Answers.CurrentValue == null)
		{
			return;
		}
		string text = (base.ViewModel.IsLastAnswer.CurrentValue ? UIStrings.Instance.CommonTexts.CloseWindow.Text : UIStrings.Instance.Tutorial.Next.Text);
		m_AnswerButtonLabel.text = text;
		m_ChooseAnswerDisposable?.Dispose();
		m_ChooseAnswerDisposable = ObservableSubscribeExtensions.Subscribe(m_AnswerButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Answers.CurrentValue.FirstOrDefault((AnswerVM a) => a.CanSelect)?.OnChooseAnswer();
		}).AddTo(this);
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
		m_Picture.transform.SetAsLastSibling();
		m_PictureAnimator.AppearAnimation();
	}
}
