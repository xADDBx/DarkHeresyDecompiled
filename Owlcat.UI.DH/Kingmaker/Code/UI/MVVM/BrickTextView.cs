using System.Collections.Generic;
using Code.View.UI.Helpers;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickTextView : BrickBaseView<BrickTextVM>
{
	[SerializeField]
	protected TextMeshProUGUI m_Text;

	[SerializeField]
	private LayoutGroup m_LayoutGroup;

	[SerializeField]
	private Color m_DefaultColor = Color.black;

	[SerializeField]
	private Color m_BrightColor = Color.white;

	private readonly Dictionary<TMP_Text, (float fontSize, float lineSpacing)> m_TextToDefaultFontSize = new Dictionary<TMP_Text, (float, float)>();

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Text).AddTo(this);
		}
		base.OnBind();
		m_Text.text = base.ViewModel.Text;
		if ((bool)m_LayoutGroup)
		{
			ApplyAlignment(base.ViewModel.Alignment);
		}
		ApplyStyle(base.ViewModel.Type);
		m_Text.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true), base.ViewModel.Owner).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}

	private void ApplyStyle(TooltipTextType type)
	{
		ApplyStyleTo(m_Text, type);
	}

	protected void ApplyStyleTo(TextMeshProUGUI text, TooltipTextType type)
	{
		if (!m_TextToDefaultFontSize.TryGetValue(text, out (float, float) value))
		{
			value = (text.fontSize, text.lineSpacing);
			m_TextToDefaultFontSize.Add(text, value);
		}
		text.paragraphSpacing = 0f;
		text.lineSpacing = value.Item2;
		text.fontStyle = FontStyles.Normal;
		text.alignment = TextAlignmentOptions.Left;
		text.fontSize = value.Item1;
		text.color = m_DefaultColor;
		if (type.HasFlag(TooltipTextType.Italic))
		{
			text.fontStyle = FontStyles.Italic;
		}
		if (type.HasFlag(TooltipTextType.Bold))
		{
			text.fontStyle = FontStyles.Bold;
		}
		if (type.HasFlag(TooltipTextType.Centered))
		{
			text.alignment = TextAlignmentOptions.Center;
		}
		if (type.HasFlag(TooltipTextType.BlackColor))
		{
			text.color = m_DefaultColor;
		}
		else if (type.HasFlag(TooltipTextType.BrightColor))
		{
			text.color = m_BrightColor;
		}
	}

	private void ApplyAlignment(TooltipTextAlignment alignment)
	{
		LayoutGroup layoutGroup = m_LayoutGroup;
		layoutGroup.childAlignment = alignment switch
		{
			TooltipTextAlignment.Right => TextAnchor.MiddleRight, 
			TooltipTextAlignment.Midl => TextAnchor.MiddleCenter, 
			TooltipTextAlignment.Left => TextAnchor.MiddleLeft, 
			_ => m_LayoutGroup.childAlignment, 
		};
	}
}
