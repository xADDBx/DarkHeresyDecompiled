using System;
using DG.Tweening;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class InitiativeTrackerUnitOrderHorizontalView : CombatUnitOrderView
{
	protected override Tween GetHideAnimationInternal(Action completeAction)
	{
		if (base.ViewModel == null || !base.ViewModel.IsCurrent.CurrentValue)
		{
			return base.CanvasGroup.DOFade(0f, m_AnimationTime).OnComplete(delegate
			{
				completeAction?.Invoke();
			});
		}
		Sequence sequence = DOTween.Sequence();
		Tweener t = base.RectTransform.DOScale(0f, m_AnimationTime);
		sequence.Join(t);
		Vector2 endValue = new Vector2(base.RectTransform.anchoredPosition.x - base.RectTransform.rect.width * 1.2f, base.RectTransform.anchoredPosition.y);
		Tweener t2 = base.RectTransform.DOAnchorPos(endValue, m_AnimationTime);
		sequence.Join(t2);
		Tweener t3 = base.CanvasGroup.DOFade(0f, m_AnimationTime);
		sequence.Append(t3);
		return sequence.OnComplete(delegate
		{
			base.RectTransform.localScale = Vector3.one;
			completeAction?.Invoke();
		});
	}

	protected override Tween GetMoveAnimationInternal(Action completeAction, Vector2 targetPosition)
	{
		Sequence sequence = DOTween.Sequence().Pause().OnComplete(delegate
		{
			completeAction?.Invoke();
		})
			.SetUpdate(isIndependentUpdate: true);
		Tweener t = base.RectTransform.DOAnchorPos(targetPosition, m_AnimationTime).Pause().SetUpdate(isIndependentUpdate: true);
		sequence.Join(t);
		return sequence;
	}

	protected override Tween GetShowAnimationInternal(Action completeAction, Vector2 targetPosition)
	{
		Sequence sequence = DOTween.Sequence().Pause().OnComplete(delegate
		{
			completeAction?.Invoke();
		});
		base.CanvasGroup.alpha = 0.01f;
		base.RectTransform.anchoredPosition = new Vector2(base.RectTransform.anchoredPosition.x, base.RectTransform.anchoredPosition.y + base.RectTransform.rect.height);
		Tweener t = base.RectTransform.DOAnchorPos(targetPosition, m_AnimationTime / 2f).Pause();
		Tweener t2 = base.CanvasGroup.DOFade(1f, m_AnimationTime / 2f).Pause();
		sequence.Join(t2).Join(t);
		return sequence;
	}
}
