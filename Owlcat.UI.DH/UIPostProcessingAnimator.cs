using System;
using System.Collections.Generic;
using Code.UI.Common.Animations;
using DG.Tweening;
using Kingmaker;
using Kingmaker.UI;
using Owlcat.Runtime.Visual.Overrides;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class UIPostProcessingAnimator : MonoBehaviour
{
	[Serializable]
	private struct UIPostEffectStateEntry
	{
		public UIPostEffectState State;

		public PostFXStateData Data;
	}

	[SerializeField]
	private ScaleAnimator m_ScaleAnimator;

	[SerializeField]
	private List<UIPostEffectStateEntry> m_StateSettings;

	private readonly Dictionary<UIPostEffectState, PostFXStateData> m_States = new Dictionary<UIPostEffectState, PostFXStateData>();

	private ChromaticAberration m_Chromatic;

	private LensDistortion m_Distortion;

	private BloomEnhanced m_Bloom;

	private FilmGrain m_FilmGrain;

	private Tweener m_ChromaticTweener;

	private Tweener m_DistortionTweener;

	private Tweener m_DistortionXTweener;

	private Tweener m_DistortionYTweener;

	private Tweener m_BloomTweener;

	private Tweener m_FilmGrainTweener;

	private bool m_IsInit;

	private Volume m_Volume;

	private static UIPostProcessingAnimator m_Instance;

	public static UIPostProcessingAnimator Instance => m_Instance;

	public void Bind()
	{
		if (m_IsInit)
		{
			return;
		}
		if (m_Instance != null && m_Instance != this)
		{
			PFLog.UI.Error("[UIPostProcessingAnimator] Instance already exists.");
			return;
		}
		m_Instance = this;
		m_Volume = UICamera.AdditionalUICamera?.GetComponentInChildren<Volume>();
		if (m_Volume == null)
		{
			return;
		}
		if (!m_Volume.profile.TryGet<ChromaticAberration>(out m_Chromatic) || !m_Volume.profile.TryGet<LensDistortion>(out m_Distortion) || !m_Volume.profile.TryGet<BloomEnhanced>(out m_Bloom) || !m_Volume.profile.TryGet<FilmGrain>(out m_FilmGrain))
		{
			Debug.LogWarning("Missing post-processing components.");
			return;
		}
		foreach (UIPostEffectStateEntry stateSetting in m_StateSettings)
		{
			m_States[stateSetting.State] = stateSetting.Data;
		}
		CreateTweeners();
		SetValuesImmediately(UIPostEffectState.Off);
		m_IsInit = true;
	}

	private void SetValuesImmediately(UIPostEffectState state)
	{
		if (!m_States.TryGetValue(state, out var value))
		{
			Debug.LogWarning($"UIPostEffectState '{state}' not configured.");
			return;
		}
		m_Chromatic.intensity.value = value.ChromaticIntensity.Value;
		m_Distortion.intensity.value = value.LensDistortionIntensity.Value;
		m_Distortion.xMultiplier.value = value.LensDistortionXMultiplier.Value;
		m_Distortion.yMultiplier.value = value.LensDistortionYMultiplier.Value;
		m_Bloom.intensity.value = value.BloomIntensity.Value;
		m_FilmGrain.intensity.value = value.FilmGrainIntensity.Value;
		m_ScaleAnimator.SetValuesImmediately(new Vector3(value.ScaleX.Value, value.ScaleY.Value, value.ScaleZ.Value));
	}

	private void CreateTweeners()
	{
		m_ChromaticTweener = DOTween.To(() => m_Chromatic.intensity.value, delegate(float x)
		{
			m_Chromatic.intensity.value = x;
		}, 0f, 0f).SetAutoKill(autoKillOnCompletion: false).Pause()
			.SetUpdate(isIndependentUpdate: true);
		m_DistortionTweener = DOTween.To(() => m_Distortion.intensity.value, delegate(float x)
		{
			m_Distortion.intensity.value = x;
		}, 0f, 0f).SetAutoKill(autoKillOnCompletion: false).Pause()
			.SetUpdate(isIndependentUpdate: true);
		m_DistortionXTweener = DOTween.To(() => m_Distortion.xMultiplier.value, delegate(float x)
		{
			m_Distortion.xMultiplier.value = x;
		}, 0f, 0f).SetAutoKill(autoKillOnCompletion: false).Pause()
			.SetUpdate(isIndependentUpdate: true);
		m_DistortionYTweener = DOTween.To(() => m_Distortion.yMultiplier.value, delegate(float x)
		{
			m_Distortion.yMultiplier.value = x;
		}, 0f, 0f).SetAutoKill(autoKillOnCompletion: false).Pause()
			.SetUpdate(isIndependentUpdate: true);
		m_BloomTweener = DOTween.To(() => m_Bloom.intensity.value, delegate(float x)
		{
			m_Bloom.intensity.value = x;
		}, 0f, 0f).SetAutoKill(autoKillOnCompletion: false).Pause()
			.SetUpdate(isIndependentUpdate: true);
		m_FilmGrainTweener = DOTween.To(() => m_FilmGrain.intensity.value, delegate(float x)
		{
			m_FilmGrain.intensity.value = x;
		}, 0f, 0f).SetAutoKill(autoKillOnCompletion: false).Pause()
			.SetUpdate(isIndependentUpdate: true);
	}

	public void PlayState(UIPostEffectState state, UnityAction onComplete = null)
	{
		Bind();
		if (!m_States.TryGetValue(state, out var value))
		{
			Debug.LogWarning($"UIPostEffectState '{state}' not configured.");
			return;
		}
		UpdateTweener(m_ChromaticTweener, m_Chromatic.intensity.value, value.ChromaticIntensity.Value, value.ChromaticIntensity.Time, value.ChromaticIntensity.Curve);
		UpdateTweener(m_DistortionTweener, m_Distortion.intensity.value, value.LensDistortionIntensity.Value, value.LensDistortionIntensity.Time, value.LensDistortionIntensity.Curve);
		UpdateTweener(m_DistortionXTweener, m_Distortion.xMultiplier.value, value.LensDistortionXMultiplier.Value, value.LensDistortionXMultiplier.Time, value.LensDistortionXMultiplier.Curve);
		UpdateTweener(m_DistortionYTweener, m_Distortion.yMultiplier.value, value.LensDistortionYMultiplier.Value, value.LensDistortionYMultiplier.Time, value.LensDistortionYMultiplier.Curve);
		UpdateTweener(m_BloomTweener, m_Bloom.intensity.value, value.BloomIntensity.Value, value.BloomIntensity.Time, value.BloomIntensity.Curve);
		UpdateTweener(m_FilmGrainTweener, m_FilmGrain.intensity.value, value.FilmGrainIntensity.Value, value.FilmGrainIntensity.Time, value.FilmGrainIntensity.Curve);
		m_ScaleAnimator?.DoCustomAnimation(new Vector3(value.ScaleX.Value, value.ScaleY.Value, value.ScaleZ.Value), value.ScaleX.Time, value.ScaleX.Curve, value.Loop);
	}

	private void UpdateTweener(Tweener tweener, float from, float to, float duration, AnimationCurve curve, UnityAction onComplete = null)
	{
		tweener.ChangeStartValue(from, duration).ChangeEndValue(to, snapStartValue: true).SetEase(curve)
			.OnComplete(delegate
			{
				onComplete?.Invoke();
			})
			.Restart();
	}

	public void Dispose()
	{
		m_ChromaticTweener?.Kill();
		m_DistortionTweener?.Kill();
		m_DistortionXTweener?.Kill();
		m_DistortionYTweener?.Kill();
		m_BloomTweener = null;
		m_FilmGrainTweener = null;
		m_ChromaticTweener = null;
		m_DistortionTweener = null;
		m_DistortionXTweener = null;
		m_DistortionYTweener = null;
		m_BloomTweener = null;
		m_FilmGrainTweener = null;
		m_IsInit = false;
		m_Instance = null;
	}
}
