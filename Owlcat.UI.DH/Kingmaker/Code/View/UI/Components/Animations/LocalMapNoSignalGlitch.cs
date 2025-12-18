using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Kingmaker.Code.View.UI.Components.Animations;

public class LocalMapNoSignalGlitch : MonoBehaviour
{
	[SerializeField]
	private Material m_GlitchMaterial;

	[SerializeField]
	private RectTransform m_Marker;

	[Header("AnimationValues")]
	[SerializeField]
	private float m_GlitchStep = 10f;

	[SerializeField]
	private float m_GlitchStrength = 0.2f;

	[SerializeField]
	private float m_LinesFrequency = 1f;

	private static readonly int OffsetScale = Shader.PropertyToID("_OffsetScale");

	private static readonly int OffsetLinesFrequency = Shader.PropertyToID("_OffsetLinesFrequency");

	private float m_NextChangeTime;

	private Coroutine m_GlitchCo;

	private void OnEnable()
	{
		m_GlitchCo = StartCoroutine(GlitchCo());
		m_Marker.DOScale(1.2f, 1f).SetLoops(-1, LoopType.Yoyo).SetUpdate(isIndependentUpdate: true);
	}

	private void OnDisable()
	{
		StopCoroutine(m_GlitchCo);
		DOTween.Kill(m_Marker);
		m_Marker.localScale = Vector3.one;
	}

	private IEnumerator GlitchCo()
	{
		while (true)
		{
			float time = Random.Range(m_GlitchStep, 2f * m_GlitchStep);
			float value = Random.Range(m_LinesFrequency, m_LinesFrequency * 5f);
			float value2 = Random.Range(m_GlitchStrength, m_GlitchStrength * 2f);
			m_GlitchMaterial.SetFloat(OffsetScale, value2);
			m_GlitchMaterial.SetFloat(OffsetLinesFrequency, value);
			yield return new WaitForSecondsRealtime(time);
		}
	}
}
