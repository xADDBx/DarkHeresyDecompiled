using System;
using UnityEngine;

namespace Owlcat.UI;

[Serializable]
public struct SwipeAnimator
{
	[SerializeField]
	[Min(0f)]
	private float m_Duration;

	[SerializeField]
	private AnimationCurve m_Ease;

	[SerializeField]
	[HideInInspector]
	private Vector2 m_Anchor;

	[SerializeField]
	private Vector2 m_Offset;

	private static readonly Action<RectTransform, SwipeAnimator, float> OnUpdateDelegate = OnUpdate;

	public readonly Transition Show(MonoBehaviour host)
	{
		return Tween.Play(host, (RectTransform)host.transform, this, 0f, 1f, m_Duration, OnUpdateDelegate);
	}

	public readonly Transition Hide(MonoBehaviour host)
	{
		return Tween.Play(host, (RectTransform)host.transform, this, 1f, 0f, m_Duration, OnUpdateDelegate);
	}

	private static void OnUpdate(RectTransform rectTransform, SwipeAnimator settings, float t)
	{
		Vector2 a = settings.m_Anchor + settings.m_Offset;
		Vector2 anchor = settings.m_Anchor;
		float t2 = ((settings.m_Ease.length > 0) ? settings.m_Ease.Evaluate(t) : t);
		rectTransform.anchoredPosition = Vector2.Lerp(a, anchor, t2);
	}
}
