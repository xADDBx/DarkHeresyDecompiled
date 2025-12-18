using System.Collections;
using DG.Tweening;
using Kingmaker.Code.Gameplay.Controllers.DetectiveRadar;
using Kingmaker.Code.View.Bridge.Root;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.SignalDevice;

public class RadarDraftView : View<SignalsDeviceVM>, IInitializable
{
	[Header("Elements")]
	[SerializeField]
	private Image m_RadarImage;

	[SerializeField]
	private Image m_RadarPulse;

	[SerializeField]
	private TMP_Text m_RadarPower;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	[Header("Values")]
	[SerializeField]
	private Gradient m_ColorGradient;

	[SerializeField]
	private Color m_JammerColor;

	[SerializeField]
	private Vector2 m_PulseTimeMinMax = new Vector2(0.25f, 1f);

	[SerializeField]
	private Vector2 m_PulseFadeMinMax = new Vector2(0f, 0.2f);

	[SerializeField]
	private Vector2 m_PulseScaleMinMax = new Vector2(1.1f, 1.4f);

	private Coroutine m_PulseCo;

	private RectTransform m_PulseRectTransform;

	public void Initialize()
	{
		m_PulseRectTransform = m_RadarPulse.GetComponent<RectTransform>();
	}

	protected override void OnBind()
	{
		base.ViewModel.SignalPowerClamped.Subscribe(delegate(float value)
		{
			m_RadarImage.color = ((base.ViewModel.State.CurrentValue == DetectiveRadarState.Jammed) ? m_JammerColor : m_ColorGradient.Evaluate(value));
			m_RadarPower.text = $"{value * 100f: 00.0}";
		}).AddTo(this);
		base.ViewModel.State.Subscribe(delegate(DetectiveRadarState value)
		{
			m_StateSelectable.SetActiveLayer(value.ToString());
			m_RadarImage.color = ((base.ViewModel.State.CurrentValue == DetectiveRadarState.Jammed) ? m_JammerColor : m_ColorGradient.Evaluate(base.ViewModel.SignalPowerClamped.CurrentValue));
		}).AddTo(this);
		m_PulseCo = StartCoroutine(DoPulseCo());
	}

	protected override void OnUnbind()
	{
		if (m_PulseCo != null)
		{
			StopCoroutine(m_PulseCo);
		}
	}

	private IEnumerator DoPulseCo()
	{
		while (true)
		{
			yield return null;
			float currentValue = base.ViewModel.SignalPowerClamped.CurrentValue;
			Color color = m_ColorGradient.Evaluate(currentValue);
			float endValue = Mathf.Lerp(m_PulseScaleMinMax.x, m_PulseScaleMinMax.y, currentValue);
			float endValue2 = Mathf.Lerp(m_PulseFadeMinMax.x, m_PulseFadeMinMax.y, currentValue);
			float num = Mathf.Lerp(m_PulseTimeMinMax.x, m_PulseTimeMinMax.y, currentValue);
			color.a = 0.75f;
			m_RadarPulse.color = color;
			m_PulseRectTransform.localScale = Vector3.one;
			DOTween.Kill(m_PulseRectTransform);
			DOTween.Kill(m_RadarPulse);
			m_PulseRectTransform.DOScale(endValue, num).SetUpdate(isIndependentUpdate: true);
			m_RadarPulse.DOFade(endValue2, num).SetUpdate(isIndependentUpdate: true);
			yield return new WaitForSeconds(num);
		}
	}
}
