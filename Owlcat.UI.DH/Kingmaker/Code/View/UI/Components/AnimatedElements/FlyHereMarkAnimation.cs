using DG.Tweening;
using UnityEngine;

namespace Kingmaker.Code.View.UI.Components.AnimatedElements;

public class FlyHereMarkAnimation : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_Arrow;

	[Space]
	[SerializeField]
	private float m_Distance = 20f;

	[SerializeField]
	private float m_Duration = 0.6f;

	[SerializeField]
	private float m_Delay = 0.3f;

	[SerializeField]
	private Ease m_Ease = Ease.InOutSine;

	private Vector2 m_InitialPosition;

	private Sequence m_Sequence;

	private void Awake()
	{
		m_InitialPosition = m_Arrow.anchoredPosition;
	}

	private void OnEnable()
	{
		m_Arrow.anchoredPosition = m_InitialPosition;
		m_Sequence = DOTween.Sequence().SetUpdate(isIndependentUpdate: true);
		m_Sequence.Append(m_Arrow.DOAnchorPosY(m_InitialPosition.y + m_Distance, m_Duration).SetEase(m_Ease));
		m_Sequence.AppendInterval(m_Delay);
		m_Sequence.SetLoops(-1, LoopType.Restart);
	}

	private void OnDisable()
	{
		m_Sequence?.Kill();
		m_Sequence = null;
		m_Arrow.anchoredPosition = m_InitialPosition;
	}
}
