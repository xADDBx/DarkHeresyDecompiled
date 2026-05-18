using System;
using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Controllers.DetectiveRadar;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.SignalDevice;

public class SignalsDeviceView : View<SignalsDeviceVM>
{
	[Serializable]
	private class RadarSettings
	{
		[field: SerializeField]
		public float Scale { get; private set; }

		[field: SerializeField]
		public float Speed { get; private set; }

		[field: SerializeField]
		public float VoiceScale { get; private set; }

		public RadarSettings(float scale, float speed, float voiceScale)
		{
			Scale = scale;
			Speed = speed;
			VoiceScale = voiceScale;
		}
	}

	[Header("Views")]
	[SerializeField]
	private List<SignalLinesView> m_LinesViews = new List<SignalLinesView>();

	[Header("Elements")]
	[SerializeField]
	private CanvasGroup m_Canvas;

	[SerializeField]
	private RectTransform m_ScaleTransform;

	[SerializeField]
	private Material m_SignalMaterial;

	[SerializeField]
	private TMP_Text m_RadarPower;

	[SerializeField]
	private TMP_Text m_SignalSourceLabel;

	[SerializeField]
	private TMP_Text m_OutOfRangeLabel;

	[SerializeField]
	private TMP_Text m_JammedLabel;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	[SerializeField]
	private MoveAnimator m_ActionBarAnimator;

	[SerializeField]
	private MoveAnimator m_PanelAnimator;

	[SerializeField]
	private Image m_LampSignal;

	[SerializeField]
	private CanvasGroup m_LampSignalGroup;

	[SerializeField]
	private Image m_OutOfRangeIcon;

	[SerializeField]
	private GameObject m_OutOfRange;

	[SerializeField]
	private Image m_JammedIcon;

	[Header("Values")]
	[SerializeField]
	private RadarSettings m_MinValueSettings;

	[SerializeField]
	private RadarSettings m_MaxValueSettings;

	[SerializeField]
	private float m_HideDelayTime = 2f;

	private static readonly int SignalColorID = Shader.PropertyToID("_Color");

	private static readonly int VoiceScaleID = Shader.PropertyToID("_Voice_Scale");

	private static readonly int TimeStepSpeedID = Shader.PropertyToID("_Time_Step_Speed");

	private IDisposable m_HideDisposable;

	private IDisposable m_SignalPulseSubscription;

	private Tween m_JammedStateTween;

	private SignalUISettings Settings => DetectiveClueSignalRoot.Instance.UISettings;

	private void Awake()
	{
		m_ActionBarAnimator.Initialize();
		m_PanelAnimator.Initialize();
	}

	protected override void OnBind()
	{
		m_OutOfRangeLabel.text = UIStrings.Instance.HUDTexts.OutOfRangeLabel.Text;
		m_LinesViews.ForEach(delegate(SignalLinesView v)
		{
			v.Bind(base.ViewModel.SignalPowerClamped);
		});
		base.ViewModel.SignalSourceName.Subscribe(delegate(string value)
		{
			m_SignalSourceLabel.text = (string.IsNullOrEmpty(value) ? ((string)UIStrings.Instance.HUDTexts.SignalLabel) : value);
		}).AddTo(this);
		base.ViewModel.SignalPowerClamped.Subscribe(UpdateSignalPower).AddTo(this);
		base.ViewModel.State.Subscribe(UpdateDetectiveRadarState).AddTo(this);
		m_JammedLabel.text = UIStrings.Instance.HUDTexts.JammedLabel.Text;
		m_JammedLabel.SetHint(UIStrings.Instance.HUDTexts.JammedSignalHint.Text).AddTo(this);
		base.ViewModel.ActionBarIsActive.Subscribe(SetActionBarVisibility).AddTo(this);
		base.ViewModel.ForceHideDevice.Subscribe(delegate(bool value)
		{
			if (value)
			{
				HideDevice();
			}
			else
			{
				UpdateDetectiveRadarState(base.ViewModel.State.CurrentValue);
			}
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_HideDisposable?.Dispose();
		DOTween.Kill(m_JammedIcon);
		DOTween.Kill(m_OutOfRangeIcon);
		m_SignalPulseSubscription?.Dispose();
		m_SignalPulseSubscription = null;
		m_JammedStateTween?.Kill();
		m_JammedStateTween = null;
	}

	private void ShowDevice()
	{
		m_HideDisposable?.Dispose();
		m_PanelAnimator.AppearAnimation();
		ServiceWindowsSounds.Instance.DetectiveJournal.SignalDeviceShow.Play();
		DOTween.Kill(m_Canvas);
		m_Canvas.DOFade(1f, 0.1f).SetUpdate(isIndependentUpdate: true);
		m_SignalPulseSubscription?.Dispose();
		m_SignalPulseSubscription = ObservableSubscribeExtensions.Subscribe(SignalFxContext.Instance.DoPulse, delegate
		{
			DoPulse();
		});
	}

	private void HideDevice()
	{
		m_PanelAnimator.DisappearAnimation(delegate
		{
			DOTween.Kill(m_Canvas);
			m_Canvas.alpha = 0f;
			m_SignalPulseSubscription?.Dispose();
			m_SignalPulseSubscription = null;
			m_JammedStateTween?.Pause();
		});
		ServiceWindowsSounds.Instance.DetectiveJournal.SignalDeviceHide.Play();
	}

	private void UpdateDetectiveRadarState(DetectiveRadarState state)
	{
		if (base.ViewModel.ForceHideDevice.CurrentValue)
		{
			return;
		}
		m_StateSelectable.SetActiveLayer(state.ToString());
		m_OutOfRange.gameObject.SetActive(state == DetectiveRadarState.NotActivated);
		switch (state)
		{
		case DetectiveRadarState.NotActivated:
			DOTween.Kill(m_OutOfRangeIcon);
			m_OutOfRangeIcon.transform.DOScale(Vector3.one * 1.2f, m_HideDelayTime * 0.2f).SetLoops(8, LoopType.Yoyo).SetUpdate(isIndependentUpdate: true);
			m_HideDisposable?.Dispose();
			m_HideDisposable = ObservableSubscribeExtensions.Subscribe(Observable.Timer(TimeSpan.FromSeconds(m_HideDelayTime)), delegate
			{
				HideDevice();
			}).AddTo(this);
			m_JammedStateTween?.Pause();
			break;
		case DetectiveRadarState.Activated:
			ShowDevice();
			m_JammedStateTween?.Pause();
			break;
		case DetectiveRadarState.Jammed:
			if (m_JammedStateTween == null)
			{
				m_JammedStateTween = m_JammedIcon.transform.DOScale(Vector3.one * 0.8f, 0.5f).SetUpdate(isIndependentUpdate: true).SetLoops(-1, LoopType.Yoyo);
			}
			m_JammedStateTween.Play();
			ShowDevice();
			break;
		default:
			throw new ArgumentOutOfRangeException("state", state, null);
		}
	}

	private void UpdateSignalPower(float power)
	{
		if (base.ViewModel.State.CurrentValue != DetectiveRadarState.Jammed)
		{
			Color value = Settings.ColorGradient.Evaluate(power);
			m_SignalMaterial.SetColor(SignalColorID, value);
			RadarSettings settingsByPercent = GetSettingsByPercent(power);
			SetMaterialValues(settingsByPercent);
			m_RadarPower.text = (int)(power * 100f) + "%";
		}
	}

	private void DoPulse()
	{
		if (SignalDeviceUtils.CanPulse())
		{
			DOTween.Kill(m_LampSignalGroup);
			m_LampSignalGroup.alpha = 0f;
			Color color = Settings.ColorGradient.Evaluate(base.ViewModel.SignalPowerClamped.CurrentValue);
			color.a = Mathf.Lerp(0.1f, 0.4f, base.ViewModel.SignalPowerClamped.CurrentValue);
			m_LampSignal.color = color;
			float duration = Settings.GetWavesTime(1f - base.ViewModel.SignalPowerClamped.CurrentValue) * 0.2f;
			AkUnitySoundEngine.SetRTPCValue("SignalsDevice", base.ViewModel.SignalPowerClamped.CurrentValue);
			ServiceWindowsSounds.Instance.DetectiveJournal.SignalDevice.Play();
			m_LampSignalGroup.DOFade(1f, duration).SetUpdate(isIndependentUpdate: true).SetLoops(2, LoopType.Yoyo)
				.OnComplete(delegate
				{
					m_LampSignalGroup.alpha = 0f;
				});
		}
	}

	private void SetActionBarVisibility(bool isActionBarActive)
	{
		if (isActionBarActive)
		{
			m_ActionBarAnimator.AppearAnimation();
		}
		else
		{
			m_ActionBarAnimator.DisappearAnimation();
		}
	}

	private void SetMaterialValues(RadarSettings settings)
	{
		SetMaterialValues(settings.Scale, settings.Speed, settings.VoiceScale);
	}

	private void SetMaterialValues(float scale, float speed, float voiceScale)
	{
		Vector3 one = Vector3.one;
		one.y = scale;
		m_ScaleTransform.localScale = one;
		m_SignalMaterial.SetFloat(TimeStepSpeedID, speed);
		m_SignalMaterial.SetFloat(VoiceScaleID, voiceScale);
	}

	private RadarSettings GetSettingsByPercent(float percent)
	{
		float scale = Mathf.Lerp(m_MinValueSettings.Scale, m_MaxValueSettings.Scale, percent);
		float speed = Mathf.Lerp(m_MinValueSettings.Speed, m_MaxValueSettings.Speed, percent);
		float voiceScale = Mathf.Lerp(m_MinValueSettings.VoiceScale, m_MaxValueSettings.VoiceScale, percent);
		return new RadarSettings(scale, speed, voiceScale);
	}
}
