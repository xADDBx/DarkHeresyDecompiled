using DG.Tweening;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public abstract class BaseCareerPathSelectionTabPCView<TViewModel> : BaseCareerPathSelectionTabCommonView<TViewModel>, ICareerPathSelectionTabPCView, ICareerPathSelectionTabView where TViewModel : ViewModel
{
	[Header("Title")]
	[SerializeField]
	private CanvasGroup m_TitleHighlightGroup;

	[SerializeField]
	private CanvasGroup m_CompleteHighlightGroup;

	[SerializeField]
	protected OwlcatMultiButton m_HighlightButton;

	[SerializeField]
	[HideIf("m_ButtonsSetFromParent")]
	private CareerButtonsBlock m_ButtonsBlock;

	protected readonly ReactiveProperty<string> HintText = new ReactiveProperty<string>();

	private readonly ReactiveProperty<bool> m_NextButtonInteractable = new ReactiveProperty<bool>(value: true);

	private readonly ReactiveProperty<bool> m_BackButtonInteractable = new ReactiveProperty<bool>(value: true);

	public bool CanCommit { get; protected set; }

	public void SetButtonsBlock(CareerButtonsBlock buttonsBlock)
	{
		m_ButtonsBlock = buttonsBlock;
	}

	protected override void OnBind()
	{
		base.OnBind();
		if (!(m_ButtonsBlock == null))
		{
			ObservableSubscribeExtensions.Subscribe(m_ButtonsBlock.NextButton.OnLeftClickAsObservable(), delegate
			{
				HandleClickNext();
			}).AddTo(this);
			m_ButtonsBlock.NextButton.SetHint(HintText).AddTo(this);
			ObservableSubscribeExtensions.Subscribe(m_ButtonsBlock.FinishButton.OnLeftClickAsObservable(), delegate
			{
				HandleClickFinish();
			}).AddTo(this);
			m_NextButtonInteractable.Subscribe(m_ButtonsBlock.NextButton.SetInteractable).AddTo(this);
			m_BackButtonInteractable.Subscribe(m_ButtonsBlock.BackButton.SetInteractable).AddTo(this);
			ObservableSubscribeExtensions.Subscribe(m_ButtonsBlock.BackButton.OnLeftClickAsObservable(), delegate
			{
				HandleClickBack();
			}).AddTo(this);
			NextButtonLabel.Subscribe(delegate(string value)
			{
				m_ButtonsBlock.NextButtonLabel.text = value;
			}).AddTo(this);
			BackButtonLabel.Subscribe(delegate(string value)
			{
				m_ButtonsBlock.BackButtonLabel.text = value;
			}).AddTo(this);
			FinishButtonLabel.Subscribe(delegate(string value)
			{
				m_ButtonsBlock.FinishButtonLabel.text = value;
			}).AddTo(this);
			ObservableSubscribeExtensions.Subscribe(m_HighlightButton.Or(null)?.OnLeftClickAsObservable(), delegate
			{
				OnHighlightButtonClick();
			});
		}
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_HighlightButton.Or(null)?.gameObject.SetActive(value: false);
	}

	protected void SetNextButtonInteractable(bool value)
	{
		m_NextButtonInteractable.Value = value;
	}

	protected void SetBackButtonInteractable(bool value)
	{
		m_BackButtonInteractable.Value = value;
	}

	protected void SetButtonVisibility(bool value)
	{
		m_ButtonsBlock.Or(null)?.gameObject.SetActive(value);
	}

	protected void SetFinishInteractable(bool value)
	{
		if ((bool)m_ButtonsBlock)
		{
			m_ButtonsBlock.FinishButton.Interactable = value;
		}
	}

	protected void SetButtonSound(ButtonSoundsEnum soundType)
	{
		if ((bool)m_ButtonsBlock)
		{
			UISounds.Instance.SetClickSound(m_ButtonsBlock.NextButton, soundType);
		}
	}

	private void OnHighlightButtonClick()
	{
		if ((bool)m_TitleHighlightGroup)
		{
			m_TitleHighlightGroup.DOKill();
			m_CompleteHighlightGroup.DOKill();
			CanvasGroup highlightGroup = (CanCommit ? m_CompleteHighlightGroup : m_TitleHighlightGroup);
			highlightGroup.DOFade(1f, 0.1f).SetLoops(4, LoopType.Yoyo).SetUpdate(isIndependentUpdate: true)
				.OnComplete(delegate
				{
					highlightGroup.alpha = 0f;
				});
		}
	}
}
