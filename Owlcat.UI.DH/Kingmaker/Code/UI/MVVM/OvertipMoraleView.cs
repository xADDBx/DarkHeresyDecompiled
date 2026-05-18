using DG.Tweening;
using Kingmaker.Predictions;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipMoraleView : View<OvertipMoraleVM>
{
	[SerializeField]
	private UnitMoraleView m_MoraleView;

	[SerializeField]
	private TextMeshProUGUI m_DeltaValue;

	[SerializeField]
	private TMP_Text m_DeltaValueSign;

	[Space]
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private Color m_DeltaPositiveColor;

	[SerializeField]
	private Color m_DeltaNegativeColor;

	private Tweener m_FadeInTween;

	private Tweener m_FadeOutTween;

	private bool m_IsVisible;

	public void HideInstant()
	{
		m_FadeInTween?.Pause();
		m_FadeOutTween?.Pause();
		m_CanvasGroup.blocksRaycasts = false;
		m_CanvasGroup.alpha = 0f;
		m_IsVisible = false;
	}

	protected override void OnBind()
	{
		UpdateMorale();
		UpdateVisibility();
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.MoraleVM.UpdateMoraleValue, delegate
		{
			UpdateMorale();
		}).AddTo(this);
		base.ViewModel.MoraleVM.UpdateVisibility.Subscribe(UpdateVisibility).AddTo(this);
		m_MoraleView.Bind(base.ViewModel.MoraleVM);
	}

	protected override void OnUnbind()
	{
		m_FadeInTween?.Pause();
		m_FadeOutTween?.Pause();
		m_CanvasGroup.blocksRaycasts = false;
		m_CanvasGroup.alpha = 0f;
		m_MoraleView.Unbind();
	}

	protected override void OnDestroy()
	{
		m_FadeInTween?.Pause();
		m_FadeOutTween?.Pause();
		base.OnDestroy();
	}

	private void UpdateVisibility()
	{
		bool flag = base.ViewModel.IsVisible();
		if (flag != m_IsVisible)
		{
			m_IsVisible = flag;
			DoVisibility(flag);
		}
	}

	private void DoVisibility(bool isVisible)
	{
		m_FadeInTween?.Pause();
		m_FadeOutTween?.Pause();
		(isVisible ? (m_FadeInTween ?? (m_FadeInTween = m_CanvasGroup.DOFade(1f, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: false))) : (m_FadeOutTween ?? (m_FadeOutTween = m_CanvasGroup.DOFade(0f, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: false)))).Restart();
		m_CanvasGroup.blocksRaycasts = isVisible;
	}

	private void UpdateMorale()
	{
		UIMoralePredictionData currentValue = base.ViewModel.MoraleVM.MoraleDeltaValue.CurrentValue;
		SetMoraleDeltaText(currentValue.MinDelta, currentValue.MaxDelta);
	}

	private void SetMoraleDeltaText(int deltaMin, int deltaMax)
	{
		if (deltaMax == 0 && deltaMin == 0)
		{
			m_DeltaValue.SetText(string.Empty);
			m_DeltaValueSign.SetText(string.Empty);
		}
		else if (deltaMin < 0 && deltaMax > 0)
		{
			SetMoraleIncreased(deltaMax, deltaMax);
			PFLog.UI.Error($"Morale prediction returned inconclusive result: min={deltaMin}, max={deltaMax}");
		}
		else if (deltaMax > 0)
		{
			SetMoraleIncreased(deltaMin, deltaMax);
		}
		else
		{
			SetMoraleDecreased(deltaMin, deltaMax);
		}
	}

	private void SetMoraleIncreased(int deltaMin, int deltaMax)
	{
		m_DeltaValueSign.SetText("+");
		m_DeltaValue.color = m_DeltaPositiveColor;
		m_DeltaValueSign.color = m_DeltaPositiveColor;
		string text = ((deltaMin == deltaMax) ? deltaMax.ToString() : $"[{deltaMin}-{deltaMax}]");
		m_DeltaValue.SetText(text);
	}

	private void SetMoraleDecreased(int deltaMin, int deltaMax)
	{
		m_DeltaValueSign.SetText("-");
		m_DeltaValue.color = m_DeltaNegativeColor;
		m_DeltaValueSign.color = m_DeltaNegativeColor;
		string text = ((deltaMin == deltaMax) ? deltaMax.ToString() : $"[{Mathf.Abs(deltaMax)}-{Mathf.Abs(deltaMin)}]");
		m_DeltaValue.SetText(text);
	}
}
