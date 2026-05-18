using System;
using Code.View.UI.UIUtils;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Settings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class DialogHistoryEntityView : View<DialogHistoryEntityVM>
{
	[Header("View links")]
	[SerializeField]
	private TextMeshProUGUI m_FullText;

	[SerializeField]
	private float m_DefaultFontSize = 20f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 24f;

	private IDisposable m_LinkTooltip;

	private DialogCueColors m_DialogCueColors;

	private TooltipConfig m_TooltipConfig;

	private string m_SpeakerNameFormatedText;

	private string m_SkillcheckFormatedText;

	private string m_SoulmarkFormatedText;

	private string m_CueFormatedText;

	private string m_ResultTextFormated;

	private static float FontMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public void Initialize(DialogCueColors dialogCueColors, TooltipConfig tooltipConfig)
	{
		m_DialogCueColors = dialogCueColors;
		m_TooltipConfig = tooltipConfig;
		m_TooltipConfig.TooltipPlace = GetComponent<RectTransform>();
	}

	protected override void OnBind()
	{
		SetTextFontSize(FontMultiplier);
		SetupTextContent();
		AddLinksInteraction();
	}

	private void SetupTextContent()
	{
		Color color = m_DialogCueColors.GetSpeakerColor(base.ViewModel.SpeakerName, base.ViewModel.IsOverrideSpeakerColor, base.ViewModel.SpeakerColor);
		m_SpeakerNameFormatedText = (base.ViewModel.HasSpeaker ? ("<smallcaps><b><color=#" + ColorUtility.ToHtmlStringRGB(color * m_DialogCueColors.NameColorMultiplayer) + ">" + base.ViewModel.SpeakerName + "</color></b></smallcaps>: ") : string.Empty);
		m_SkillcheckFormatedText = (base.ViewModel.HasSkillchecks ? UtilitySkillcheck.SkillCheckText(base.ViewModel.SkillChecks, m_DialogCueColors.SkillCheckColors) : string.Empty);
		m_SoulmarkFormatedText = (base.ViewModel.HasSoulMarkShift ? UIUtilityUnit.SoulMarkShiftsText(base.ViewModel.SoulMarkShifts, m_DialogCueColors.SoulMarkShiftColors) : string.Empty);
		string text = UIUtilityText.StringIDToColor(base.ViewModel.Text, DialogCueColors.NarratorColorStringID, m_DialogCueColors.Narrator);
		m_CueFormatedText = (base.ViewModel.IsNarratorText ? ("<i><color=#" + m_DialogCueColors.Narrator.HTML() + ">" + text + "</color><i>") : text);
		m_ResultTextFormated = m_SpeakerNameFormatedText + m_SkillcheckFormatedText + m_SoulmarkFormatedText + m_CueFormatedText;
		if (base.ViewModel.HasSpeaker)
		{
			m_ResultTextFormated = "<indent=7%><line-indent=-7%>" + m_ResultTextFormated + "</indent>";
		}
		m_FullText.text = m_ResultTextFormated;
	}

	private void AddLinksInteraction()
	{
		m_FullText.SetLinkTooltip(null, base.ViewModel.SkillChecks, m_TooltipConfig).AddTo(this);
	}

	protected override void OnUnbind()
	{
	}

	private void SetTextFontSize(float multiplier)
	{
		m_FullText.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * multiplier;
	}

	public void HandleChangeFontSizeSettings(float size)
	{
		SetTextFontSize(size);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}
}
