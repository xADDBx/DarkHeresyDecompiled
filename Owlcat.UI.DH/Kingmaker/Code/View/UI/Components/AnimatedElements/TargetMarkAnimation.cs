using DG.Tweening;
using UnityEngine;

namespace Kingmaker.Code.View.UI.Components.AnimatedElements;

public class TargetMarkAnimation : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_Ring;

	[SerializeField]
	private RectTransform m_Corners;

	[Space]
	[SerializeField]
	private float m_Duration = 0.5f;

	[SerializeField]
	private float m_Delay = 0.3f;

	[SerializeField]
	private float m_CornerScale = 1.3f;

	[SerializeField]
	private float m_RotationAngle = 180f;

	[SerializeField]
	private Ease m_Ease = Ease.InOutSine;

	private Sequence m_Sequence;

	private void OnEnable()
	{
		m_Ring.localRotation = Quaternion.identity;
		m_Corners.localScale = Vector3.one;
		m_Sequence = DOTween.Sequence().SetUpdate(isIndependentUpdate: true);
		m_Sequence.Append(m_Ring.DOLocalRotate(new Vector3(0f, 0f, m_RotationAngle), m_Duration, RotateMode.FastBeyond360).SetEase(m_Ease));
		m_Sequence.Join(m_Corners.DOScale(m_CornerScale, m_Duration).SetEase(m_Ease));
		m_Sequence.AppendInterval(m_Delay);
		m_Sequence.Append(m_Ring.DOLocalRotate(Vector3.zero, m_Duration, RotateMode.FastBeyond360).SetEase(m_Ease));
		m_Sequence.Join(m_Corners.DOScale(1f, m_Duration).SetEase(m_Ease));
		m_Sequence.AppendInterval(m_Delay);
		m_Sequence.SetLoops(-1);
	}

	private void OnDisable()
	{
		m_Sequence?.Kill();
		m_Sequence = null;
	}
}
