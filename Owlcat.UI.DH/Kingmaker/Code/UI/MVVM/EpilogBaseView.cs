using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Sound;
using ObservableCollections;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM;

public class EpilogBaseView : View<EpilogVM>, IInitializable
{
	[Header("Window")]
	[SerializeField]
	private FadeAnimator m_WindowAnimator;

	[Header("Picture page")]
	[SerializeField]
	private CharBPortraitChanger m_Portrait;

	[Header("Cue")]
	[SerializeField]
	private EpilogCueBaseView m_Cue;

	[SerializeField]
	protected ScrollRectExtended m_CueScrollRect;

	[Header("Title")]
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[Header("Background")]
	[SerializeField]
	private Image m_BackgroundImage;

	[SerializeField]
	private VideoPlayerHelper m_BackgroundVideo;

	[SerializeField]
	private FadeAnimator m_BackgroundAnimator;

	private bool m_ContentRefreshing;

	private bool m_IsInit;

	public virtual void Initialize()
	{
		if (!m_IsInit)
		{
			base.gameObject.SetActive(value: false);
			m_WindowAnimator.Initialize();
			m_BackgroundAnimator.Initialize();
			m_BackgroundVideo.Initialize();
			m_Cue.Initialize(ConfigRoot.Instance.UIConfig.m_DefaultDialogCueColors);
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		Show();
		base.ViewModel.Portrait.Subscribe(delegate(Sprite p)
		{
			m_Portrait.SetNewPortrait(p);
		}).AddTo(this);
		base.ViewModel.Title.Subscribe(delegate
		{
			OnTitleChanged();
		}).AddTo(this);
		base.ViewModel.BackgroundClip.Subscribe(delegate
		{
			OnBackgroundChanged();
		}).AddTo(this);
		base.ViewModel.BackgroundSprite.Subscribe(delegate
		{
			OnBackgroundChanged();
		}).AddTo(this);
		base.ViewModel.Cues.ObserveAdd().Subscribe(delegate
		{
			OnCuesChanged();
		}).AddTo(this);
		base.ViewModel.Answers.Subscribe(delegate
		{
			OnAnswersChanged();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	protected virtual void OnAnswersChanged()
	{
	}

	private void OnTitleChanged()
	{
		SetTitle();
	}

	private void OnBackgroundChanged()
	{
		m_BackgroundAnimator.DisappearAnimation(delegate
		{
			SetBackground();
			m_BackgroundAnimator.AppearAnimation();
		});
	}

	private void OnCuesChanged()
	{
		CueVM cueVM = base.ViewModel.Cues.LastOrDefault();
		m_Cue.gameObject.SetActive(cueVM != null);
		if (cueVM != null)
		{
			m_Cue.Bind(cueVM);
		}
		m_CueScrollRect.ScrollToTop();
	}

	private void SetTitle()
	{
		m_Title.text = base.ViewModel.Title.CurrentValue;
	}

	private void SetBackground()
	{
		VideoClip currentValue = base.ViewModel.BackgroundClip.CurrentValue;
		m_BackgroundVideo.gameObject.SetActive(currentValue != null);
		if (m_BackgroundVideo.VideoClip != currentValue)
		{
			m_BackgroundVideo.Stop();
			m_BackgroundVideo.SetClip(currentValue, SoundStateType.Video, prepareVideo: false, base.ViewModel.SoundStart.CurrentValue, base.ViewModel.SoundStop.CurrentValue);
		}
		Sprite currentValue2 = base.ViewModel.BackgroundSprite.CurrentValue;
		m_BackgroundImage.gameObject.SetActive(currentValue2 != null);
		m_BackgroundImage.sprite = currentValue2;
		if ((bool)currentValue2 && m_BackgroundImage.TryGetComponent<AspectRatioFitter>(out var component))
		{
			component.aspectRatio = currentValue2.rect.width / currentValue2.rect.height;
		}
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
		m_WindowAnimator.AppearAnimation();
		UISounds.Instance.Sounds.Dialogue.BookOpen.Play();
	}

	private void Hide()
	{
		m_WindowAnimator.DisappearAnimation(delegate
		{
			base.gameObject.SetActive(value: false);
		});
		UISounds.Instance.Sounds.Dialogue.BookClose.Play();
	}

	protected virtual void Confirm()
	{
		base.ViewModel.Answers.CurrentValue.FirstOrDefault()?.OnChooseAnswer();
	}
}
