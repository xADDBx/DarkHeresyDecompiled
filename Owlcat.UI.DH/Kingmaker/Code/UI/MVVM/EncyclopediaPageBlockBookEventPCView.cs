using System.Collections.Generic;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaPageBlockBookEventPCView : EncyclopediaPageBlockPCView<EncyclopediaPageBlockBookEventVM>
{
	[Header("Cue Group")]
	[SerializeField]
	private GameObject m_CueGroup;

	[SerializeField]
	private TextMeshProUGUI m_CueText;

	[Header("Answer Group")]
	[SerializeField]
	private GameObject m_AnswerGroup;

	[SerializeField]
	private TextMeshProUGUI m_AnswerText;

	[SerializeField]
	private float m_DefaultFontSize = 22f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 22f;

	protected override void OnBind()
	{
		base.OnBind();
		m_CueGroup.SetActive(base.ViewModel.IsCue);
		m_AnswerGroup.SetActive(base.ViewModel.IsAnswer);
		(base.ViewModel.IsAnswer ? m_AnswerText : m_CueText).text = base.ViewModel.Text;
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		m_CueText.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.LeftMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: true)).AddTo(this);
		m_AnswerText.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.LeftMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: true)).AddTo(this);
		m_CueText.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * base.ViewModel.FontMultiplier;
		m_AnswerText.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * base.ViewModel.FontMultiplier;
	}

	public override List<TextMeshProUGUI> GetLinksTexts()
	{
		return new List<TextMeshProUGUI> { m_CueText, m_AnswerText };
	}
}
