using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickFormattedDescriptionView : BrickBaseView<BrickFormattedDescriptionVM>
{
	[SerializeField]
	private TMP_Text m_Text;

	private TMP_StyleSheet m_DefaultStyleSheet;

	private Color? m_DefaultColor;

	protected override void OnBind()
	{
		if ((object)m_DefaultStyleSheet == null)
		{
			m_DefaultStyleSheet = m_Text.styleSheet;
		}
		m_Text.styleSheet = (base.ViewModel.StyleSheet ? base.ViewModel.StyleSheet : m_DefaultStyleSheet);
		Color valueOrDefault = m_DefaultColor.GetValueOrDefault();
		if (!m_DefaultColor.HasValue)
		{
			valueOrDefault = m_Text.color;
			m_DefaultColor = valueOrDefault;
		}
		m_Text.color = base.ViewModel.TextColor ?? m_DefaultColor.Value;
		m_Text.SetText(base.ViewModel.Description);
		m_Text.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true), base.ViewModel.Owner).AddTo(this);
	}
}
