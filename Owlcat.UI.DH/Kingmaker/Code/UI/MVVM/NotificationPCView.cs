using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NotificationPCView<T> : View<T> where T : NotificationVM
{
	[SerializeField]
	private RectTransform m_Container;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private Vector2 m_StartPos;

	[SerializeField]
	private Vector2 m_MidPos;

	[SerializeField]
	private Vector2 m_EndPos;

	[SerializeField]
	private float m_AppearTime;

	[SerializeField]
	private float m_HoldTime;

	[SerializeField]
	private float m_DisappearTime;

	private readonly List<Tweener> m_StartedTweeners = new List<Tweener>();

	public void Initialize()
	{
		ResetAnimation();
	}

	protected override void OnBind()
	{
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.ShowNotificationCommand, delegate
		{
			ShowNotification();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		StopAllCoroutines();
		m_StartedTweeners.ForEach(delegate(Tweener t)
		{
			t.Kill();
		});
		m_StartedTweeners.Clear();
	}

	private void ShowNotification()
	{
		StopAllCoroutines();
		StartCoroutine(PlayAnimationCoroutine());
	}

	private IEnumerator PlayAnimationCoroutine()
	{
		ResetAnimation();
		yield return null;
		PlayAppearAnimation();
		yield return new WaitForSeconds(m_AppearTime + m_HoldTime);
		PlayDisappearAnimation();
	}

	private void PlayAppearAnimation()
	{
		m_StartedTweeners.Add(m_Container.DOAnchorPos(m_MidPos, m_AppearTime).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true));
		m_StartedTweeners.Add(m_CanvasGroup.DOFade(1f, m_AppearTime).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true));
	}

	private void PlayDisappearAnimation()
	{
		m_StartedTweeners.Add(m_Container.DOAnchorPos(m_EndPos, m_DisappearTime).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true));
		m_StartedTweeners.Add(m_CanvasGroup.DOFade(0f, m_DisappearTime).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true));
	}

	private void ResetAnimation()
	{
		m_Container.anchoredPosition = m_StartPos;
		m_CanvasGroup.alpha = 0f;
	}
}
