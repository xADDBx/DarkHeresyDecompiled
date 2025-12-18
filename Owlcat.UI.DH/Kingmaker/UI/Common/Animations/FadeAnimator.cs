using System;
using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.Code.View.Bridge.Root;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Events;

namespace Kingmaker.UI.Common.Animations;

[RequireComponent(typeof(CanvasGroup))]
public class FadeAnimator : MonoBehaviour, IUIAnimator, IInitializable
{
	[SerializeField]
	[UsedImplicitly]
	private Ease m_AppearAnimCurve = Ease.Linear;

	[SerializeField]
	[UsedImplicitly]
	private Ease m_DisappearAnimCurve = Ease.Linear;

	[SerializeField]
	[UsedImplicitly]
	private bool m_GameObjectAlwaysActive;

	[SerializeField]
	private float m_AppearDelay;

	[SerializeField]
	private float m_DisappearDelay;

	private CanvasGroup m_CanvasGroup;

	private bool? m_PermanentBlockRaycast;

	private bool m_IsInitialized;

	private UnityAction m_AppearAction;

	private UnityAction m_DisappearAction;

	private Tweener m_AppearTween;

	private Tweener m_DisappearTween;

	[field: SerializeField]
	[field: UsedImplicitly]
	public float AppearTime { get; private set; } = 0.2f;


	[field: SerializeField]
	[field: UsedImplicitly]
	public float DisappearTime { get; private set; } = 0.2f;


	public CanvasGroup CanvasGroup
	{
		get
		{
			if ((object)m_CanvasGroup == null)
			{
				m_CanvasGroup = this.EnsureComponent<CanvasGroup>();
			}
			return m_CanvasGroup;
		}
	}

	public event Action OnAppearEvent;

	public event Action OnDisappearEvent;

	public void Initialize()
	{
		if (m_GameObjectAlwaysActive)
		{
			base.gameObject.SetActive(value: true);
		}
		if (CanvasGroup != null)
		{
			CanvasGroup.alpha = 0f;
			CanvasGroup.blocksRaycasts = false;
		}
		m_PermanentBlockRaycast = null;
		TryCreateTweens();
		m_IsInitialized = true;
	}

	public void TryCreateTweens()
	{
		if (m_AppearTween == null)
		{
			m_AppearTween = CanvasGroup.Or(null)?.DOFade(1f, AppearTime).SetAutoKill(autoKillOnCompletion: false).SetEase(m_AppearAnimCurve)
				.SetUpdate(isIndependentUpdate: true)
				.SetDelay(m_AppearDelay)
				.OnComplete(delegate
				{
					if (CanvasGroup != null)
					{
						CanvasGroup.blocksRaycasts = !m_PermanentBlockRaycast.HasValue || m_PermanentBlockRaycast.Value;
						CanvasGroup.alpha = 1f;
					}
					m_AppearAction?.Invoke();
				});
			m_AppearTween?.ChangeStartValue(0f);
		}
		if (m_DisappearTween != null)
		{
			return;
		}
		m_DisappearTween = CanvasGroup.Or(null)?.DOFade(0f, DisappearTime).SetAutoKill(autoKillOnCompletion: false).SetEase(m_DisappearAnimCurve)
			.SetUpdate(isIndependentUpdate: true)
			.SetDelay(m_DisappearDelay)
			.OnComplete(delegate
			{
				if (CanvasGroup != null)
				{
					CanvasGroup.alpha = 0f;
				}
				if (m_DisappearAction != null)
				{
					m_DisappearAction();
				}
				else if (!m_GameObjectAlwaysActive)
				{
					base.gameObject.SetActive(value: false);
				}
			});
	}

	public void AppearAnimation([CanBeNull] UnityAction action = null)
	{
		if (!m_IsInitialized)
		{
			Initialize();
		}
		if (m_DisappearTween.IsPlaying())
		{
			m_DisappearTween.Pause();
		}
		if (CanvasGroup != null)
		{
			CanvasGroup.blocksRaycasts = !m_PermanentBlockRaycast.HasValue || m_PermanentBlockRaycast.Value;
		}
		if (!m_GameObjectAlwaysActive)
		{
			if (!base.gameObject.activeInHierarchy && m_CanvasGroup != null)
			{
				m_CanvasGroup.alpha = 0.01f;
			}
			base.gameObject.SetActive(value: true);
		}
		if (!base.gameObject.activeInHierarchy)
		{
			if (m_CanvasGroup != null)
			{
				m_CanvasGroup.alpha = 1f;
			}
			return;
		}
		TryCreateTweens();
		m_AppearAction = action;
		if (CanvasGroup != null)
		{
			CanvasGroup.blocksRaycasts = !m_PermanentBlockRaycast.HasValue || m_PermanentBlockRaycast.Value;
		}
		if (CanvasGroup != null)
		{
			this.OnAppearEvent?.Invoke();
			m_AppearTween.ChangeStartValue(CanvasGroup.alpha);
			m_AppearTween.Play();
		}
	}

	public void DisappearAnimation([CanBeNull] UnityAction action = null)
	{
		if (this == null)
		{
			return;
		}
		if (!m_IsInitialized)
		{
			Initialize();
		}
		if (CanvasGroup != null)
		{
			CanvasGroup.blocksRaycasts = m_PermanentBlockRaycast.HasValue && m_PermanentBlockRaycast.Value;
		}
		if (!base.gameObject.activeInHierarchy)
		{
			if (m_CanvasGroup != null)
			{
				m_CanvasGroup.alpha = 0.001f;
			}
			if (!m_GameObjectAlwaysActive)
			{
				base.gameObject.SetActive(value: false);
			}
			return;
		}
		TryCreateTweens();
		m_DisappearAction = action;
		if (m_AppearTween.IsPlaying())
		{
			m_AppearTween.Pause();
		}
		if (CanvasGroup != null)
		{
			CanvasGroup.blocksRaycasts = m_PermanentBlockRaycast.HasValue && m_PermanentBlockRaycast.Value;
			this.OnDisappearEvent?.Invoke();
			m_DisappearTween.ChangeStartValue(CanvasGroup.alpha);
			m_DisappearTween.Play();
		}
	}

	public void PlayAnimation(bool value, UnityAction action = null)
	{
		if (value)
		{
			AppearAnimation(action);
		}
		else
		{
			DisappearAnimation(action);
		}
	}

	public void PlayOnce()
	{
		AppearAnimation(delegate
		{
			DisappearAnimation();
		});
	}

	public void BlockRaycastPermanent(bool state)
	{
		m_PermanentBlockRaycast = state;
	}

	public void DisappearInstant()
	{
		m_AppearTween?.Pause();
		m_DisappearTween?.Pause();
		CanvasGroup.alpha = 0.001f;
	}

	public void OnDisable()
	{
		bool num = m_AppearTween != null && m_AppearTween.IsPlaying();
		bool flag = m_DisappearTween != null && m_DisappearTween.IsPlaying();
		if (num)
		{
			m_AppearTween.Complete(withCallbacks: true);
		}
		else if (flag)
		{
			m_DisappearTween.Complete(withCallbacks: true);
		}
		m_PermanentBlockRaycast = null;
		m_AppearTween?.Kill();
		m_DisappearTween?.Kill();
		m_AppearTween = null;
		m_DisappearTween = null;
		m_IsInitialized = false;
	}

	public void SetAlwaysActive(bool state)
	{
		m_GameObjectAlwaysActive = state;
	}
}
