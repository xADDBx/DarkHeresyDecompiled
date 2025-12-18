using System;
using UnityEngine;

namespace Owlcat.UI;

[Serializable]
public struct AlphaAnimator
{
	[SerializeField]
	[Min(0f)]
	private float m_Duration;

	[SerializeField]
	private AnimationCurve m_Ease;

	private static readonly Action<CanvasGroup, AlphaAnimator, float> OnUpdateDelegate = OnUpdate;

	public readonly Transition Show(MonoBehaviour host)
	{
		return Tween.Play(host, GetOrCreate<CanvasGroup>(host), this, 0f, 1f, m_Duration, OnUpdateDelegate);
	}

	public readonly Transition Hide(MonoBehaviour host)
	{
		return Tween.Play(host, GetOrCreate<CanvasGroup>(host), this, 1f, 0f, m_Duration, OnUpdateDelegate);
	}

	private static T GetOrCreate<T>(Component component) where T : Component
	{
		if (!component.TryGetComponent<T>(out var component2))
		{
			return component.gameObject.AddComponent<T>();
		}
		return component2;
	}

	private static void OnUpdate(CanvasGroup group, AlphaAnimator settings, float t)
	{
		group.alpha = ((settings.m_Ease.length > 0) ? settings.m_Ease.Evaluate(t) : t);
	}
}
