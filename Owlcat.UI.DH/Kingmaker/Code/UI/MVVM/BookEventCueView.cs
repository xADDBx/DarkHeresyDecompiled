using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Settings;
using Kingmaker.UI.Common.DebugInformation;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BookEventCueView : View<CueVM>, IHasBlueprintInfo
{
	[Serializable]
	public class BookEventCueStyle
	{
		[SerializeField]
		private FontStyles m_FontStyle;

		[SerializeField]
		private Color m_FontColor = Color.black;

		[SerializeField]
		private float m_FontSize = 18f;

		[SerializeField]
		private float m_CharacterSpacing;

		[SerializeField]
		private float m_LineSpacing;

		[SerializeField]
		private RectOffset m_Margins;

		public void ApplyStyleTo(TMP_Text text, float fontSizeMultiplier)
		{
			text.fontStyle = m_FontStyle;
			text.color = m_FontColor;
			text.fontSize = m_FontSize * fontSizeMultiplier;
			text.characterSpacing = m_CharacterSpacing;
			text.lineSpacing = m_LineSpacing;
			text.margin = new Vector4(m_Margins.left, m_Margins.right, m_Margins.top, m_Margins.bottom);
		}
	}

	[SerializeField]
	internal CanvasGroup m_CueGroup;

	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private BookEventCueStyle m_NormalText;

	[SerializeField]
	private BookEventCueStyle m_HighlightText;

	[SerializeField]
	private BookEventCueStyle m_ShadeText;

	[SerializeField]
	private BookEventCueStyle m_SpecialText;

	[Header("First letter")]
	[SerializeField]
	private TMP_FontAsset m_FirstLetterFont;

	[SerializeField]
	private Material m_FirstLetterFontMaterial;

	[SerializeField]
	private Color m_FirstLetterColor = Color.black;

	[SerializeField]
	private int m_FirstLetterSize = 170;

	[SerializeField]
	private int m_FirstLetterVOffset;

	private BookEventCueColors m_Colors;

	private Action m_DestroyAction;

	public TextMeshProUGUI Text => m_Text;

	public List<SkillCheckResult> SkillChecks => base.ViewModel?.SkillChecks;

	public BlueprintScriptableObject Blueprint => base.ViewModel?.Blueprint;

	public void Initialize(Action destroyAction, BookEventCueColors colors)
	{
		m_DestroyAction = destroyAction;
		m_Colors = colors;
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_CueGroup.alpha = 0f;
		string mechanicText = base.ViewModel.GetMechanicText(m_Colors.SkillCheckColors, m_Colors.SoulMarkShiftColors, "\n\n");
		string text = UIUtilityText.StringIDToColor(base.ViewModel.RawText, DialogCueColors.NarratorColorStringID, m_Colors.Narrator);
		if (!base.ViewModel.IsSpecial)
		{
			text = UIUtilityText.GetBookFormat(text, m_FirstLetterFont, m_FirstLetterColor, m_FirstLetterSize, m_FirstLetterVOffset, m_FirstLetterFontMaterial);
		}
		SetText(mechanicText + " " + text);
		m_Text.SetLinkTooltip(null, base.ViewModel.SkillChecks, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetText(string text)
	{
		m_Text.text = text;
		CueVM viewModel = base.ViewModel;
		((viewModel != null && viewModel.IsSpecial) ? m_SpecialText : m_NormalText).ApplyStyleTo(m_Text, SettingsRoot.Accessiability.FontSizeMultiplier);
	}

	public void Highlight()
	{
		m_HighlightText.ApplyStyleTo(m_Text, SettingsRoot.Accessiability.FontSizeMultiplier);
	}

	public void Shade()
	{
		m_ShadeText.ApplyStyleTo(m_Text, SettingsRoot.Accessiability.FontSizeMultiplier);
	}
}
