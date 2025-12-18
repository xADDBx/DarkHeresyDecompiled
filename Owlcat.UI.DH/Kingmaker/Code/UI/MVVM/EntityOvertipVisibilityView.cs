using System;
using System.Collections.Generic;
using DG.Tweening;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class EntityOvertipVisibilityView : View<EntityOvertipVisibilityVM>
{
	[Serializable]
	private struct VisibilityThreshold
	{
		public float VisibilityRate;

		public float Alpha;
	}

	private enum ForcedVisibilityState : byte
	{
		None,
		Show,
		Hide,
		UnreachableTarget
	}

	[Space]
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private VisibilityThreshold[] m_VisibilityThresholds;

	[SerializeField]
	private float m_UnreachableTargetAlpha = 0.3f;

	[SerializeField]
	private int m_UpdateRate = 3;

	private Dictionary<ForcedVisibilityState, Tween> m_CachedVisibilityTweens = new Dictionary<ForcedVisibilityState, Tween>();

	private Tween m_VisibilityTween;

	private Func<Vector3> m_GetViewportPos;

	private ForcedVisibilityState m_CurrentForcedState;

	public void Initialize(Func<Vector3> getViewportPos)
	{
		m_GetViewportPos = getViewportPos;
	}

	protected override void OnBind()
	{
		m_CanvasGroup.alpha = 0f;
		m_CurrentForcedState = ForcedVisibilityState.None;
		Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate).Subscribe(HandleUpdate).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_VisibilityTween?.Pause();
		m_VisibilityTween = null;
	}

	private void HandleUpdate()
	{
		if (Time.frameCount % m_UpdateRate == 0)
		{
			UpdateVisibility();
		}
	}

	private void UpdateVisibility()
	{
		float alpha;
		ForcedVisibilityState forcedVisibilityState = GetForcedVisibilityState(out alpha);
		if (forcedVisibilityState != 0)
		{
			if (forcedVisibilityState != m_CurrentForcedState)
			{
				m_CurrentForcedState = forcedVisibilityState;
				DoVisibility(forcedVisibilityState, alpha);
			}
		}
		else
		{
			SetVisibility();
		}
	}

	private void SetVisibility()
	{
		m_CurrentForcedState = ForcedVisibilityState.None;
		float visibilityRate = base.ViewModel.GetVisibilityRate(m_GetViewportPos());
		int num = RateToThresholdIndex(visibilityRate);
		VisibilityThreshold visibilityThreshold = m_VisibilityThresholds[num];
		if (num < 1)
		{
			m_CanvasGroup.alpha = visibilityThreshold.Alpha;
			return;
		}
		VisibilityThreshold visibilityThreshold2 = m_VisibilityThresholds[num - 1];
		float visibilityRate2 = visibilityThreshold.VisibilityRate;
		float visibilityRate3 = visibilityThreshold2.VisibilityRate;
		float t = (visibilityRate - visibilityRate2) / (visibilityRate3 - visibilityRate2);
		m_CanvasGroup.alpha = Mathf.Lerp(visibilityThreshold.Alpha, visibilityThreshold2.Alpha, t);
		int RateToThresholdIndex(float rate)
		{
			for (int i = 0; i < m_VisibilityThresholds.Length; i++)
			{
				if (rate <= m_VisibilityThresholds[i].VisibilityRate)
				{
					return i;
				}
			}
			return m_VisibilityThresholds.Length - 1;
		}
	}

	private ForcedVisibilityState GetForcedVisibilityState(out float alpha)
	{
		if (base.ViewModel.IsForcedHidden())
		{
			alpha = 0f;
			return ForcedVisibilityState.Hide;
		}
		if (base.ViewModel.IsForcedVisible())
		{
			alpha = 1f;
			return ForcedVisibilityState.Show;
		}
		if (base.ViewModel.IsUnreachableTarget())
		{
			alpha = m_UnreachableTargetAlpha;
			return ForcedVisibilityState.UnreachableTarget;
		}
		alpha = 1f;
		return ForcedVisibilityState.None;
	}

	private void DoVisibility(ForcedVisibilityState visibility, float endAlpha)
	{
		m_VisibilityTween?.Pause();
		(m_VisibilityTween = GetVisibilityTween(visibility, endAlpha)).Restart();
	}

	private Tween GetVisibilityTween(ForcedVisibilityState visibility, float endAlpha)
	{
		if (m_CachedVisibilityTweens.TryGetValue(visibility, out var value))
		{
			return value;
		}
		value = m_CanvasGroup.DOFade(endAlpha, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: false);
		m_CachedVisibilityTweens.Add(visibility, value);
		return value;
	}

	protected override void OnDestroy()
	{
		foreach (KeyValuePair<ForcedVisibilityState, Tween> cachedVisibilityTween in m_CachedVisibilityTweens)
		{
			cachedVisibilityTween.Deconstruct(out var _, out var value);
			value.Kill();
		}
		base.OnDestroy();
	}

	private void OnValidate()
	{
		if (m_VisibilityThresholds != null)
		{
			Array.Sort(m_VisibilityThresholds, (VisibilityThreshold lhs, VisibilityThreshold rhs) => lhs.VisibilityRate.CompareTo(rhs.VisibilityRate));
		}
	}
}
