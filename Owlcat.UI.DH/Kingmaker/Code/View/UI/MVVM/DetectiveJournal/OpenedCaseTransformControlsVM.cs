using DG.Tweening;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class OpenedCaseTransformControlsVM : ViewModel
{
	private readonly ReactiveProperty<float> m_CurrentZoom = new ReactiveProperty<float>(0.9f);

	private Tweener m_ScaleTweener;

	private const float ZoomMin = 0.3f;

	private const float ZoomMax = 1.5f;

	private const int ZoomSteps = 6;

	public ReadOnlyReactiveProperty<float> CurrentZoom => m_CurrentZoom;

	public float CurrentZoom01 => ZoomTo01(m_CurrentZoom.Value);

	public void ChangeZoomBy(float value)
	{
		float value2 = m_CurrentZoom.Value + value;
		value2 = Mathf.Clamp(value2, 0.3f, 1.5f);
		if (!Mathf.Approximately(value2, m_CurrentZoom.Value))
		{
			DOTween.Kill(m_ScaleTweener);
			m_ScaleTweener = DOTween.To(delegate(float x)
			{
				m_CurrentZoom.Value = x;
			}, m_CurrentZoom.Value, value2, 0.2f).SetUpdate(isIndependentUpdate: true).SetEase(Ease.Linear);
		}
	}

	public void ZoomInClick()
	{
		ChangeZoomStep(1);
		UISounds.Instance.Play(UISounds.Instance.Sounds.Common.ZoomInButton, isButton: true);
	}

	public void ZoomOutClick()
	{
		ChangeZoomStep(-1);
		UISounds.Instance.Play(UISounds.Instance.Sounds.Common.ZoomOutButton, isButton: true);
	}

	private void ChangeZoomStep(int step)
	{
		int num = Mathf.RoundToInt(6f * (m_CurrentZoom.Value - 0.3f) / 1.2f + (float)step);
		float num2 = Mathf.Lerp(0.3f, 1.5f, (float)num / 6f);
		ChangeZoomBy(num2 - m_CurrentZoom.Value);
	}

	public void SetValueFromSlider(float value)
	{
		m_CurrentZoom.Value = Mathf.Lerp(0.3f, 1.5f, value);
	}

	public float ZoomTo01(float value)
	{
		return (value - 0.3f) / 1.2f;
	}
}
