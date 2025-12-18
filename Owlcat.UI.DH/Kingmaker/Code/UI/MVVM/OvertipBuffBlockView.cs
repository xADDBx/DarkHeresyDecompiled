using DG.Tweening;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipBuffBlockView : View<OvertipBuffBlockVM>
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private BuffsBlockView m_BuffsBlockView;

	private Tweener m_FadeTween;

	public void HideInstant()
	{
		m_FadeTween?.Kill();
		m_CanvasGroup.blocksRaycasts = false;
		m_CanvasGroup.alpha = 0f;
	}

	protected override void OnBind()
	{
		m_BuffsBlockView.Bind(base.ViewModel.UnitBuffBlockVM);
		base.ViewModel.IsVisible.Subscribe(DoVisibility).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_BuffsBlockView.Unbind();
	}

	private void DoVisibility(bool isVisible)
	{
		float endValue = (isVisible ? 1f : 0f);
		m_FadeTween?.Kill();
		m_FadeTween = m_CanvasGroup.DOFade(endValue, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
	}
}
