using System.Collections.Generic;
using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateMoralePressure : TooltipBaseTemplate
{
	private readonly ReadOnlyReactiveProperty<float> m_MoraleBalanceNormalized;

	private readonly ReadOnlyReactiveProperty<MoraleBalanceState> m_MoraleVictoryState;

	private readonly TMP_StyleSheet m_StyleSheet;

	private readonly Color? m_TextColor;

	private StringBuilder m_StringBuilder;

	public TooltipTemplateMoralePressure(ReadOnlyReactiveProperty<float> moraleBalanceNormalized, ReadOnlyReactiveProperty<MoraleBalanceState> moraleVictoryState, TMP_StyleSheet styleSheet = null, Color? textColor = null)
	{
		m_MoraleBalanceNormalized = moraleBalanceNormalized;
		m_MoraleVictoryState = moraleVictoryState;
		m_StyleSheet = styleSheet;
		m_TextColor = textColor;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new BrickTitleVM(UIStrings.Instance.MoralePressureTooltip.Title);
		yield return new BrickSpaceVM(10f);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new BrickFormattedDescriptionVM(GetText(), null).SetStyleSheet(m_StyleSheet).SetTextColor(m_TextColor);
	}

	private string GetText()
	{
		if (m_StringBuilder == null)
		{
			m_StringBuilder = new StringBuilder();
		}
		m_StringBuilder.Clear();
		UIMoralePressureTooltip moralePressureTooltip = UIStrings.Instance.MoralePressureTooltip;
		m_StringBuilder.Append(moralePressureTooltip.GeneralDescription);
		m_StringBuilder.Append("\n\n");
		int num = Mathf.RoundToInt(100f * m_MoraleBalanceNormalized.CurrentValue);
		string obj = ((m_MoraleVictoryState.CurrentValue == MoraleBalanceState.None) ? string.Empty : ((string)moralePressureTooltip.ThresholdReached));
		GameLogContext.CurrentValue = num;
		GameLogContext.Text = obj;
		m_StringBuilder.Append(moralePressureTooltip.CurrentMoralePressure);
		m_StringBuilder.Append("\n\n");
		LocalizedString localizedString = m_MoraleVictoryState.CurrentValue switch
		{
			MoraleBalanceState.Continued => moralePressureTooltip.SuppressedMoraleDescription, 
			MoraleBalanceState.Pending => moralePressureTooltip.HighPressureDescription, 
			_ => (num != 100) ? moralePressureTooltip.LowPressureDescription : moralePressureTooltip.HighPressureDescription, 
		};
		m_StringBuilder.Append(localizedString);
		return m_StringBuilder.ToString();
	}
}
