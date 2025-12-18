using DG.Tweening;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipMoraleView : View<UnitMoraleVM>
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
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.UpdateMoraleValue, delegate
		{
			UpdateMorale();
		}).AddTo(this);
		base.ViewModel.UpdateVisibility.Subscribe(UpdateVisibility).AddTo(this);
		m_MoraleView.Bind(base.ViewModel);
	}

	protected override void OnUnbind()
	{
		m_FadeInTween?.Pause();
		m_FadeOutTween?.Pause();
		m_CanvasGroup.blocksRaycasts = false;
		m_CanvasGroup.alpha = 0f;
		m_MoraleView.Unbind();
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
		UIMoralePredictionData currentValue = base.ViewModel.MoraleDeltaValue.CurrentValue;
		SetMoraleDeltaText(currentValue.MinDelta, currentValue.MaxDelta);
	}

	private void SetMoraleDeltaText(int deltaMin, int deltaMax)
	{
		int num = Mathf.Abs(deltaMax);
		if (num == 0)
		{
			m_DeltaValue.SetText(string.Empty);
			m_DeltaValueSign.SetText(string.Empty);
			return;
		}
		int num2 = Mathf.Abs(deltaMin);
		bool num3 = deltaMax > 0;
		string text = (num3 ? "+" : "-");
		string text2 = ((num2 != num) ? $"[{num2}-{num}]" : num.ToString());
		m_DeltaValue.SetText(text2);
		m_DeltaValueSign.SetText(text);
		Color color = (num3 ? m_DeltaPositiveColor : m_DeltaNegativeColor);
		m_DeltaValue.color = color;
		m_DeltaValueSign.color = color;
	}

	protected override void OnDestroy()
	{
		m_FadeInTween?.Pause();
		m_FadeOutTween?.Pause();
		base.OnDestroy();
	}
}
