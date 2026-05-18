using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class MoraleBalanceView : View<MoraleBalanceVM>
{
	private const string LayerDefault = "Default";

	private const string LayerThreshold = "Threshold";

	private const string LayerPending = "Pending";

	private const string LayerContinued = "Continued";

	[SerializeField]
	private TMP_Text m_MoraleValueLabel;

	[SerializeField]
	private Image[] m_MoraleFillImages;

	[SerializeField]
	private MonoBehaviour m_HintSource;

	[SerializeField]
	private OwlcatMultiSelectable m_MultiSelectable;

	[SerializeField]
	private int m_HighlightThreshold = 95;

	[SerializeField]
	private TMP_StyleSheet m_TooltipStyleSheet;

	[SerializeField]
	private Color m_TooltipTextColor;

	protected override void OnBind()
	{
		base.ViewModel.MoraleBalanceNormalized.Subscribe(SetMoraleBalance).AddTo(this);
		base.ViewModel.MoraleVictoryState.Subscribe(SetVictoryState).AddTo(this);
		m_HintSource.SetTooltip(base.ViewModel.GetTooltip(m_TooltipStyleSheet, m_TooltipTextColor)).AddTo(this);
	}

	private void SetMoraleBalance(float normalizedValue)
	{
		m_MoraleValueLabel.SetText(FloatToPercent(normalizedValue).ToString());
		Image[] moraleFillImages = m_MoraleFillImages;
		for (int i = 0; i < moraleFillImages.Length; i++)
		{
			moraleFillImages[i].fillAmount = normalizedValue;
		}
	}

	private void SetVictoryState(MoraleBalanceState victoryState)
	{
		string activeLayer = victoryState switch
		{
			MoraleBalanceState.Continued => "Continued", 
			MoraleBalanceState.Pending => "Pending", 
			_ => (FloatToPercent(base.ViewModel.MoraleBalanceNormalized.CurrentValue) >= m_HighlightThreshold) ? "Threshold" : "Default", 
		};
		m_MultiSelectable.SetActiveLayer(activeLayer);
	}

	private static int FloatToPercent(float value)
	{
		return Mathf.RoundToInt(100f * value);
	}
}
