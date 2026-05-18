using System;
using Kingmaker.Settings;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class DialogAnswerConsoleView : DialogAnswerBaseView
{
	[SerializeField]
	private Image m_ConsoleHint;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 20f;

	private readonly ReactiveProperty<bool> m_Focused = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanShowTooltip = new ReactiveProperty<bool>(value: true);

	public bool HasTooltip => base.ViewModel.AnswerTooltip.CurrentValue != null;

	protected override void OnBind()
	{
		base.OnBind();
		SetTextFontSize(SettingsRoot.Accessiability.FontSizeMultiplier);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_CanShowTooltip.Dispose();
	}

	private void SetTextFontSize(float multiplier)
	{
		m_AnswerText.fontSize = m_DefaultConsoleFontSize * multiplier;
		(from x in m_Focused.Debounce(TimeSpan.FromSeconds(0.20000000298023224))
			where x
			where base.ViewModel != null && base.ViewModel.AnswerTooltip != null
			select x).Subscribe(UpdateHint).AddTo(this);
		m_Focused.Where((bool x) => !x).Subscribe(UpdateHint).AddTo(this);
	}

	private void UpdateHint(bool visible)
	{
		if (visible && m_CanShowTooltip.Value)
		{
			this.ShowTooltip(base.ViewModel.AnswerTooltip.CurrentValue);
		}
		else
		{
			TooltipHelper.HideTooltip();
		}
	}

	public override void UpdateTextSize(float multiplier)
	{
		SetTextFontSize(multiplier);
		base.UpdateTextSize(multiplier);
	}

	public void UpdateCanShowTooltip(bool canShow)
	{
		if (m_CanShowTooltip.Value != canShow)
		{
			m_CanShowTooltip.Value = canShow;
			UpdateHint(m_Focused.Value);
		}
	}
}
