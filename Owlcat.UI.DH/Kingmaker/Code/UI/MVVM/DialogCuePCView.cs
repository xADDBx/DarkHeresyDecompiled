using System;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common.DebugInformation;
using Kingmaker.Utility.Attributes;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class DialogCuePCView : View<CueVM>, ISettingsFontSizeUIHandler, ISubscriber, IHasBlueprintInfo
{
	[Header("View links")]
	[SerializeField]
	private TextMeshProUGUI m_FullCueText;

	[Header("Properties")]
	[Tooltip("It may be used in BookEvents, when there are many Cues and part of them must to have another style")]
	[SerializeField]
	private bool m_HasSpecialView;

	[Tooltip("May be used for gamepad view")]
	[SerializeField]
	private bool m_IgnoreFontMultipliter;

	[Header("Font Styles")]
	[SerializeField]
	private FontStyles m_NormalFontStyle;

	[ShowIf("m_HasSpecialView")]
	[SerializeField]
	private FontStyles m_SpecialFontStyle;

	[Header("Font sizes")]
	[SerializeField]
	private float m_DefaultPCFontSize = 22f;

	[ShowIf("m_HasSpecialView")]
	[SerializeField]
	private float m_SpecialPCFontSize = 22f;

	private DialogCueColors m_DialogCueColors;

	private Action m_DestroyAction;

	private TooltipConfig m_TooltipConfig;

	private float m_CurrentFontSize;

	private string m_SpeakerNameFormatedText;

	private string m_SkillcheckFormatedText;

	private string m_SoulmarkFormatedText;

	private string m_CueFormatedText;

	private string m_DevCommentFormatedText;

	private string m_ResultTextFormated;

	public BlueprintScriptableObject Blueprint => base.ViewModel?.Blueprint;

	public void Initialize(Action destroyAction, DialogCueColors dialogCueColors, TooltipConfig tooltipConfig)
	{
		m_DestroyAction = destroyAction;
		m_DialogCueColors = dialogCueColors;
		m_TooltipConfig = tooltipConfig;
		m_TooltipConfig.TooltipPlace = GetComponent<RectTransform>();
	}

	protected override void OnBind()
	{
		EventBus.Subscribe(this).AddTo(this);
		base.gameObject.SetActive(value: true);
		SetupTextFontSize(base.ViewModel.FontSizeMultiplier);
		SetupFontStyle();
		SetupTextContent();
		AddLinksInteraction();
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
		m_DestroyAction?.Invoke();
	}

	private void SetupFontStyle()
	{
		m_FullCueText.fontStyle = (base.ViewModel.IsSpecial ? m_SpecialFontStyle : m_NormalFontStyle);
	}

	private void SetupTextFontSize(float multiplier)
	{
		m_CurrentFontSize = (base.ViewModel.IsSpecial ? m_SpecialPCFontSize : m_DefaultPCFontSize);
		if (!m_IgnoreFontMultipliter)
		{
			m_CurrentFontSize *= multiplier;
		}
		m_FullCueText.fontSize = m_CurrentFontSize;
	}

	private void SetupTextContent()
	{
		Color color = m_DialogCueColors.GetSpeakerColor(base.ViewModel.SpeakerName, base.ViewModel.IsOverrideSpeakerColor, base.ViewModel.SpeakerColor);
		m_SpeakerNameFormatedText = (base.ViewModel.HasSpeaker ? ("<smallcaps><b><color=#" + ColorUtility.ToHtmlStringRGB(color * m_DialogCueColors.NameColorMultiplayer) + ">" + base.ViewModel.SpeakerName + "</color></b></smallcaps>: ") : string.Empty);
		m_SkillcheckFormatedText = (base.ViewModel.HasSkillchecks ? UtilitySkillcheck.SkillCheckText(base.ViewModel.SkillChecks, m_DialogCueColors.SkillCheckColors) : string.Empty);
		m_SoulmarkFormatedText = (base.ViewModel.HasSoulMarkShift ? UIUtilityUnit.SoulMarkShiftsText(base.ViewModel.SoulMarkShifts, m_DialogCueColors.SoulMarkShiftColors) : string.Empty);
		string text = UIUtilityText.StringIDToColor(base.ViewModel.RawText, DialogCueColors.NarratorColorStringID, m_DialogCueColors.Narrator);
		m_CueFormatedText = (base.ViewModel.IsNarratorText ? ("<i><color=#" + m_DialogCueColors.Narrator.HTML() + ">" + text + "</color><i>") : text);
		m_DevCommentFormatedText = (base.ViewModel.IsShowDevComment ? ("\n<color=#" + m_DialogCueColors.DevComment.HTML() + ">[DevComment]:" + base.ViewModel.DevComment + "</color>") : string.Empty);
		m_ResultTextFormated = m_SpeakerNameFormatedText + m_SkillcheckFormatedText + m_SoulmarkFormatedText + m_CueFormatedText + m_DevCommentFormatedText;
		if (base.ViewModel.HasSpeaker)
		{
			m_ResultTextFormated = "<indent=7%><line-indent=-7%>" + m_ResultTextFormated + "</indent>";
		}
		m_FullCueText.text = m_ResultTextFormated;
	}

	private void AddLinksInteraction()
	{
		m_FullCueText.SetLinkTooltip(null, base.ViewModel.SkillChecks, m_TooltipConfig).AddTo(this);
	}

	public void HandleChangeFontSizeSettings(float size)
	{
		SetupTextFontSize(size);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}
}
