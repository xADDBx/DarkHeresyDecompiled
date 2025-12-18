using System;
using DG.Tweening;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class MoraleBalanceView : View<MoraleBalanceVM>
{
	[SerializeField]
	private TMP_Text m_MoraleValueLabel;

	[SerializeField]
	private Image[] m_MoraleFillImages;

	[SerializeField]
	private MonoBehaviour m_HintSource;

	[Space]
	[SerializeField]
	private TMP_Text[] m_LabelsToColor;

	[SerializeField]
	private Image[] m_ImagesToColor;

	[SerializeField]
	private Color m_ImageStartColor = Color.white;

	[SerializeField]
	private Color m_ImageEndColor = Color.white;

	[SerializeField]
	private Color m_TextStartColor = Color.white;

	[SerializeField]
	private Color m_TextEndColor = Color.white;

	[SerializeField]
	private Color m_ToolipHighlightColor = Color.white;

	[SerializeField]
	private int m_TooltipHighlightSize = 110;

	[Space]
	[SerializeField]
	private Image m_MoraleFillHighlight;

	[SerializeField]
	private int m_HighlightValueThreshold = 99;

	[SerializeField]
	private float m_HighightMinAlpha = 0.25f;

	[SerializeField]
	private float m_HighightMaxAlpha = 1f;

	[SerializeField]
	private float m_HighightTweenDuration = 0.25f;

	[SerializeField]
	private AnimationCurve m_HighlightCurve;

	private Tween m_HighlightTween;

	private bool m_IsHighlightedState;

	private IDisposable m_Tooltip;

	protected override void OnBind()
	{
		base.ViewModel.MoraleBalanceNormalized.Subscribe(SetMoraleBalance).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_HighlightTween?.Kill();
		m_HighlightTween = null;
		m_IsHighlightedState = false;
		m_Tooltip?.Dispose();
		m_Tooltip = null;
	}

	private void SetMoraleBalance(float normalizedValue)
	{
		int num = Mathf.RoundToInt(100f * normalizedValue);
		m_MoraleValueLabel.SetText(num.ToString());
		Image[] moraleFillImages = m_MoraleFillImages;
		for (int i = 0; i < moraleFillImages.Length; i++)
		{
			moraleFillImages[i].fillAmount = normalizedValue;
		}
		SetColor(normalizedValue);
		bool flag = num >= m_HighlightValueThreshold;
		if (flag != m_IsHighlightedState || m_Tooltip == null)
		{
			m_IsHighlightedState = flag;
			ShowHighlight(flag);
			SetTooltip(flag);
		}
	}

	private void SetTooltip(bool isHighlightedState)
	{
		m_Tooltip?.Dispose();
		TooltipBaseTemplate template = (isHighlightedState ? base.ViewModel.GetVictoryHintTooltipTemplate(m_ToolipHighlightColor, m_TooltipHighlightSize) : base.ViewModel.GetDefaultHintTooltipTemplate());
		m_Tooltip = m_HintSource.SetTooltip(template).AddTo(this);
	}

	private void SetColor(float normalizedValue)
	{
		Color color = Color.Lerp(m_ImageStartColor, m_ImageEndColor, normalizedValue);
		Image[] imagesToColor = m_ImagesToColor;
		for (int i = 0; i < imagesToColor.Length; i++)
		{
			imagesToColor[i].color = color;
		}
		Color color2 = Color.Lerp(m_TextStartColor, m_TextEndColor, normalizedValue);
		TMP_Text[] labelsToColor = m_LabelsToColor;
		for (int i = 0; i < labelsToColor.Length; i++)
		{
			labelsToColor[i].color = color2;
		}
	}

	private void ShowHighlight(bool show)
	{
		if (show)
		{
			Color color = m_MoraleFillHighlight.color;
			color.a = m_HighightMinAlpha;
			m_MoraleFillHighlight.color = color;
			m_MoraleFillHighlight.gameObject.SetActive(value: true);
			if (m_HighlightTween == null)
			{
				m_HighlightTween = GetHighlightTween();
			}
		}
		else
		{
			m_HighlightTween?.Pause();
			m_MoraleFillHighlight.gameObject.SetActive(value: false);
		}
	}

	private Tween GetHighlightTween()
	{
		return m_MoraleFillHighlight.DOFade(m_HighightMaxAlpha, m_HighightTweenDuration).SetEase(m_HighlightCurve).SetLoops(-1)
			.SetAutoKill(autoKillOnCompletion: false);
	}

	[ContextMenu("Show Highlight")]
	private void EditorRestartTween()
	{
		m_HighlightTween?.Kill();
		Color color = m_MoraleFillHighlight.color;
		color.a = m_HighightMinAlpha;
		m_MoraleFillHighlight.color = color;
		m_HighlightTween = GetHighlightTween();
	}
}
