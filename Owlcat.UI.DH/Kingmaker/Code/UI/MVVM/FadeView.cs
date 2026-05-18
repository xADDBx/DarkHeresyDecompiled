using System;
using System.Diagnostics;
using DG.Tweening;
using JetBrains.Annotations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class FadeView : View<FadeVM>
{
	[SerializeField]
	[UsedImplicitly]
	private Image m_FadeImage;

	[SerializeField]
	[UsedImplicitly]
	[Range(0f, 1f)]
	public float FadeTimer = 0.3f;

	[SerializeField]
	[UsedImplicitly]
	private CanvasGroup m_Vignette;

	[SerializeField]
	[UsedImplicitly]
	[Range(0f, 1f)]
	private float m_CutSceneTimer = 0.3f;

	[Header("Timer Block")]
	[SerializeField]
	[UsedImplicitly]
	private int m_minRndTimerSeconds;

	[SerializeField]
	[UsedImplicitly]
	private int m_maxRndTimerSeconds;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_Timer;

	[CanBeNull]
	private Tween m_Tween;

	private bool m_IsCutSceneShow;

	private TimeSpan m_StartTime;

	private readonly Stopwatch m_Stopwatch = new Stopwatch();

	protected override void OnBind()
	{
		base.ViewModel.LoadingScreen.Subscribe(ShowLoadingScreen).AddTo(this);
		base.ViewModel.CutsceneOverlay.Subscribe(DoCutScene).AddTo(this);
	}

	public void DoCutScene(bool state)
	{
		m_Vignette.gameObject.SetActive(state);
		m_IsCutSceneShow = state;
		if (m_IsCutSceneShow)
		{
			m_Stopwatch.Reset();
			m_Stopwatch.Start();
			m_StartTime = TimeSpan.FromSeconds(UnityEngine.Random.Range(m_minRndTimerSeconds, m_maxRndTimerSeconds));
		}
		else
		{
			m_Stopwatch.Stop();
		}
	}

	public void ShowLoadingScreen(FadeVM.Params fadeParams)
	{
		CancelTween();
		Ease ease = fadeParams.FadeParams?.Ease ?? Ease.Linear;
		float duration = fadeParams.FadeParams?.Duration ?? FadeTimer;
		if (fadeParams.Fade)
		{
			base.ViewModel.SetStateShowAnimation();
			m_Tween = m_FadeImage.DOFade(1f, duration).SetEase(ease).OnComplete(delegate
			{
				base.ViewModel.SetStateShown();
			})
				.SetUpdate(isIndependentUpdate: true);
		}
		else
		{
			base.ViewModel.SetStateHideAnimation();
			m_Tween = m_FadeImage.DOFade(0f, duration).SetEase(ease).OnComplete(delegate
			{
				base.ViewModel.SetStateHidden();
			})
				.SetUpdate(isIndependentUpdate: true);
		}
	}

	private void CancelTween()
	{
		m_Tween?.Kill();
		m_Tween = null;
	}

	private void Update()
	{
		if (m_IsCutSceneShow)
		{
			TimeSpan timeSpan = m_StartTime.Add(TimeSpan.FromMilliseconds(m_Stopwatch.ElapsedMilliseconds));
			m_Timer.text = $"{timeSpan.Days:D2} :: {timeSpan.Hours:D2} :: {timeSpan.Minutes:D2} :: {timeSpan.Seconds:D2}";
		}
	}
}
