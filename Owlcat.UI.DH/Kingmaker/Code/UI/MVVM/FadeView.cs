using DG.Tweening;
using JetBrains.Annotations;
using Owlcat.UI;
using R3;
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

	[CanBeNull]
	private Tween m_Tween;

	protected override void OnBind()
	{
		base.ViewModel.LoadingScreen.Subscribe(ShowLoadingScreen).AddTo(this);
		base.ViewModel.CutsceneOverlay.Subscribe(DoCutScene).AddTo(this);
	}

	public void DoCutScene(bool state)
	{
		m_Vignette.gameObject.SetActive(state);
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
}
