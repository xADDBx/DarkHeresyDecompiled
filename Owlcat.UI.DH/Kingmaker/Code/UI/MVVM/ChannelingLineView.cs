using System.Collections.Generic;
using DG.Tweening;
using Owlcat.UI;
using R3;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ChannelingLineView : View<ChannelingLineVM>
{
	private const string HdrColorScaleName = "_HdrColorScale";

	private static readonly int HdrColorScaleId = Shader.PropertyToID("_HdrColorScale");

	[SerializeField]
	private int m_Density = 3;

	[SerializeField]
	public int m_MinPoint = 10;

	[SerializeField]
	[Range(0f, 2f)]
	private float m_FadeTime = 0.2f;

	[SerializeField]
	private LineRenderer m_LineRenderer;

	[SerializeField]
	private float m_ArcHight = 0.5f;

	[Header("Gradients")]
	[Tooltip("Gradient applied while the channeling line is in its normal/active state.")]
	[SerializeField]
	private Gradient m_DefaultGradient;

	[Tooltip("Gradient applied while obstructed (LOS blocked / range). E.g. yellow.")]
	[SerializeField]
	private Gradient m_ObstructedGradient;

	[Header("HDR Color Scale values")]
	[Tooltip("_HdrColorScale used in the normal/active state.")]
	[SerializeField]
	[Min(0f)]
	private float m_DefaultHdrScale = 1f;

	[Tooltip("_HdrColorScale used while obstructed (the settle value after the blink).")]
	[SerializeField]
	[Min(0f)]
	private float m_ObstructedHdrScale = 0.5f;

	[Tooltip("Peak _HdrColorScale at the top of the obstructed-state-change blink.")]
	[SerializeField]
	[Min(0f)]
	private float m_ObstructedBlinkHdrScale = 5f;

	[Tooltip("Peak _HdrColorScale at the top of the death blink, before fading to zero.")]
	[SerializeField]
	[Min(0f)]
	private float m_DeathBlinkHdrScale = 5f;

	[Header("Obstructed transition")]
	[Tooltip("Time to ramp HDR up to the obstructed-blink peak when state flips.")]
	[SerializeField]
	[Range(0f, 1f)]
	private float m_ObstructedBlinkRiseDuration = 0.08f;

	[SerializeField]
	private Ease m_ObstructedBlinkRiseEase = Ease.OutQuad;

	[Tooltip("Time to settle HDR from the blink peak down to the target (default or obstructed) value.")]
	[SerializeField]
	[Range(0f, 1f)]
	private float m_ObstructedTransitionDuration = 0.2f;

	[Header("Death (buff ended — completed / interrupted)")]
	[Tooltip("Time to ramp HDR from current value up to the death-blink peak.")]
	[SerializeField]
	[Range(0f, 1f)]
	private float m_DeathBlinkRiseDuration = 0.08f;

	[SerializeField]
	private Ease m_DeathBlinkRiseEase = Ease.OutQuad;

	[Tooltip("Time to fade HDR from the death-blink peak down to zero (line vanishes).")]
	[SerializeField]
	[Range(0f, 2f)]
	private float m_DeathFadeDuration = 0.4f;

	[SerializeField]
	private Ease m_DeathFadeEase = Ease.InQuad;

	[Header("Mechanics Feature Icons")]
	[Tooltip("Icon shown at the arc apex when the caster has SteadyConcentration. Embed as a child of this prefab; leave null to disable. RGB is driven from the line's default/obstructed gradient (sampled at IconGradientSampleT) unless overridden below; alpha mirrors the line's fade.")]
	[SerializeField]
	private GameObject m_SteadyConcentrationIcon;

	[Tooltip("If enabled, the icon's RGB is taken from the override colors below (one for default state, one for obstructed) instead of being sampled from the line's gradient. Alpha is still driven by the line's fade lifecycle.")]
	[SerializeField]
	private bool m_OverrideSteadyConcentrationIconColor;

	[Tooltip("Solid RGB color for the icon in the default (non-obstructed) state when override is enabled. Alpha is ignored — icon transparency is driven by the line's fade tweens.")]
	[SerializeField]
	[ColorUsage(false)]
	private Color m_SteadyConcentrationIconColorDefaultOverride = Color.white;

	[Tooltip("Solid RGB color for the icon in the obstructed state when override is enabled. Alpha is ignored — icon transparency is driven by the line's fade tweens.")]
	[SerializeField]
	[ColorUsage(false)]
	private Color m_SteadyConcentrationIconColorObstructedOverride = Color.yellow;

	[Tooltip("Where along the active line gradient (0..1) to sample the icon's color. 0.5 gives the color at the arc apex, which matches the icon's spatial position. Ignored when override is enabled.")]
	[SerializeField]
	[Range(0f, 1f)]
	private float m_IconGradientSampleT = 0.5f;

	private bool m_DeathStarted;

	private Sequence m_DeathSequence;

	private Tween m_StateTween;

	private bool m_FirstObstructionApplied;

	private Material m_ActiveIconMaterial;

	private Tween m_IconAlphaTween;

	protected override void OnBind()
	{
		m_DeathStarted = false;
		m_DeathSequence?.Kill();
		m_DeathSequence = null;
		m_StateTween?.Kill();
		m_StateTween = null;
		m_IconAlphaTween?.Kill();
		m_IconAlphaTween = null;
		m_FirstObstructionApplied = false;
		ApplyActiveLook();
		UpdateIconForState(base.ViewModel.IsObstructed.CurrentValue);
		base.ViewModel.StartPos.CombineLatest(base.ViewModel.EndPos, (Vector3 start, Vector3 end) => new { start, end }).Subscribe(value =>
		{
			OnPositionsChanged(value.start, value.end);
		}).AddTo(this);
		base.ViewModel.IsVisible.Subscribe(m_LineRenderer.gameObject.SetActive).AddTo(this);
		base.ViewModel.IsObstructed.Subscribe(OnObstructedChanged).AddTo(this);
		base.ViewModel.IsDying.Where((bool d) => d).Take(1).Subscribe(delegate
		{
			PlayDeath();
		})
			.AddTo(this);
		FadeInOut(0f, visible: true);
		StartIconFadeIn();
	}

	protected override void OnUnbind()
	{
		if (m_DeathStarted)
		{
			m_DeathSequence?.Kill();
			m_DeathSequence = null;
			m_StateTween?.Kill();
			m_StateTween = null;
			m_IconAlphaTween?.Kill();
			m_IconAlphaTween = null;
		}
		else
		{
			FadeInOut(null, visible: false).OnComplete(delegate
			{
				WidgetFactory.DisposeWidget(this);
			});
			StartIconFadeOut();
		}
	}

	private void OnPositionsChanged(Vector3 start, Vector3 end)
	{
		if (!m_DeathStarted)
		{
			SetArcLine(start, end);
			PositionMidpointIcon(start, end);
		}
	}

	private void OnObstructedChanged(bool obstructed)
	{
		if (!m_DeathStarted)
		{
			UpdateIconForState(obstructed);
			m_StateTween?.Kill();
			Gradient gradient = ((obstructed && m_ObstructedGradient != null) ? m_ObstructedGradient : m_DefaultGradient);
			if (gradient != null)
			{
				m_LineRenderer.colorGradient = gradient;
			}
			float num = (obstructed ? m_ObstructedHdrScale : m_DefaultHdrScale);
			if (!m_FirstObstructionApplied || base.ViewModel.IsHologramLine)
			{
				m_FirstObstructionApplied = true;
				m_LineRenderer.material.SetFloat(HdrColorScaleId, num);
				return;
			}
			Sequence sequence = DOTween.Sequence();
			sequence.Append(m_LineRenderer.material.DOFloat(m_ObstructedBlinkHdrScale, "_HdrColorScale", m_ObstructedBlinkRiseDuration).SetEase(m_ObstructedBlinkRiseEase));
			sequence.Append(m_LineRenderer.material.DOFloat(num, "_HdrColorScale", m_ObstructedTransitionDuration));
			m_StateTween = sequence;
		}
	}

	private void PlayDeath()
	{
		m_DeathStarted = true;
		m_DeathSequence?.Kill();
		m_StateTween?.Kill();
		m_LineRenderer.material.DOKill();
		Sequence sequence = DOTween.Sequence();
		sequence.Append(m_LineRenderer.material.DOFloat(m_DeathBlinkHdrScale, "_HdrColorScale", m_DeathBlinkRiseDuration).SetEase(m_DeathBlinkRiseEase));
		sequence.Append(m_LineRenderer.material.DOFloat(0f, "_HdrColorScale", m_DeathFadeDuration).SetEase(m_DeathFadeEase));
		sequence.OnComplete(delegate
		{
			m_DeathSequence = null;
			base.ViewModel?.NotifyDeathAnimationComplete();
		});
		m_DeathSequence = sequence;
		StartIconDeathFade();
	}

	private void ApplyActiveLook()
	{
		if (!(m_LineRenderer == null))
		{
			m_LineRenderer.material.SetFloat(HdrColorScaleId, m_DefaultHdrScale);
			if (m_DefaultGradient != null)
			{
				m_LineRenderer.colorGradient = m_DefaultGradient;
			}
		}
	}

	private Tweener FadeInOut(float? startAlpha, bool visible)
	{
		Color color = m_LineRenderer.material.color;
		color.a = startAlpha ?? color.a;
		m_LineRenderer.material.color = color;
		return m_LineRenderer.material.DOFade(visible ? 1f : 0f, m_FadeTime);
	}

	private void SetArcLine(Vector3 startPos, Vector3 endPos)
	{
		startPos += base.ViewModel.StartObjectOffset;
		endPos += base.ViewModel.EndObjectOffset;
		m_LineRenderer.sharedMaterial.SetFloat(ShaderProps._TimeEditor, Time.unscaledTime - Time.time);
		Vector3 vector = (startPos + endPos) / 2f;
		Vector3 vector2 = new Vector3(0f, m_ArcHight, 0f);
		Vector3 control = vector + vector2;
		float num = Vector3.Distance(startPos, endPos);
		int num2 = Mathf.Max((int)Mathf.Floor((float)m_Density * num), m_MinPoint);
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i <= num2; i++)
		{
			float t = (float)i / (float)num2;
			Vector3 item = CalculateQuadraticBezierPoint(t, startPos, control, endPos);
			list.Add(item);
		}
		m_LineRenderer.positionCount = list.Count;
		m_LineRenderer.SetPositions(list.ToArray());
	}

	private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 start, Vector3 control, Vector3 end)
	{
		float num = 1f - t;
		float num2 = t * t;
		return num * num * start + 2f * num * t * control + num2 * end;
	}

	private void UpdateIconForState(bool obstructed)
	{
		if (m_SteadyConcentrationIcon == null)
		{
			m_ActiveIconMaterial = null;
			return;
		}
		if (base.ViewModel == null || !base.ViewModel.HasSteadyConcentration)
		{
			m_SteadyConcentrationIcon.SetActive(value: false);
			m_ActiveIconMaterial = null;
			return;
		}
		m_SteadyConcentrationIcon.SetActive(value: true);
		if (m_ActiveIconMaterial == null)
		{
			m_ActiveIconMaterial = GetIconMaterial(m_SteadyConcentrationIcon);
		}
		ApplyIconColor(obstructed);
	}

	private void ApplyIconColor(bool obstructed)
	{
		if (m_ActiveIconMaterial == null)
		{
			return;
		}
		Color color;
		if (m_OverrideSteadyConcentrationIconColor)
		{
			color = (obstructed ? m_SteadyConcentrationIconColorObstructedOverride : m_SteadyConcentrationIconColorDefaultOverride);
		}
		else
		{
			Gradient gradient = ((obstructed && m_ObstructedGradient != null) ? m_ObstructedGradient : m_DefaultGradient);
			if (gradient == null)
			{
				return;
			}
			color = gradient.Evaluate(m_IconGradientSampleT);
		}
		color.a = m_ActiveIconMaterial.color.a;
		m_ActiveIconMaterial.color = color;
	}

	private static Material GetIconMaterial(GameObject icon)
	{
		Renderer componentInChildren = icon.GetComponentInChildren<Renderer>(includeInactive: true);
		if (!(componentInChildren != null))
		{
			return null;
		}
		return componentInChildren.material;
	}

	private void PositionMidpointIcon(Vector3 start, Vector3 end)
	{
		if (!(m_SteadyConcentrationIcon == null) && m_SteadyConcentrationIcon.activeSelf)
		{
			Vector3 vector = start + base.ViewModel.StartObjectOffset;
			Vector3 vector2 = end + base.ViewModel.EndObjectOffset;
			Vector3 position = (vector + vector2) * 0.5f + new Vector3(0f, m_ArcHight * 0.5f, 0f);
			m_SteadyConcentrationIcon.transform.position = position;
		}
	}

	private void StartIconFadeIn()
	{
		if (!(m_ActiveIconMaterial == null))
		{
			m_IconAlphaTween?.Kill();
			Color color = m_ActiveIconMaterial.color;
			color.a = 0f;
			m_ActiveIconMaterial.color = color;
			m_IconAlphaTween = m_ActiveIconMaterial.DOFade(1f, m_FadeTime);
		}
	}

	private void StartIconFadeOut()
	{
		if (!(m_ActiveIconMaterial == null))
		{
			m_IconAlphaTween?.Kill();
			m_IconAlphaTween = m_ActiveIconMaterial.DOFade(0f, m_FadeTime);
		}
	}

	private void StartIconDeathFade()
	{
		if (!(m_ActiveIconMaterial == null))
		{
			m_IconAlphaTween?.Kill();
			Sequence sequence = DOTween.Sequence();
			sequence.AppendInterval(m_DeathBlinkRiseDuration);
			sequence.Append(m_ActiveIconMaterial.DOFade(0f, m_DeathFadeDuration).SetEase(m_DeathFadeEase));
			m_IconAlphaTween = sequence;
		}
	}
}
