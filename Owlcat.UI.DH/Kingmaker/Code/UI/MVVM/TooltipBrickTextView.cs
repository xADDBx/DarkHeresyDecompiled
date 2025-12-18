using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickTextView : TooltipBaseBrickView<TooltipBrickTextVM>
{
	[SerializeField]
	protected TextMeshProUGUI m_Text;

	[SerializeField]
	private GameObject m_RightDecoration;

	[SerializeField]
	private GameObject m_LeftDecoration;

	[SerializeField]
	private GameObject m_LineAndBackground;

	[SerializeField]
	private LayoutGroup m_LayoutGroup;

	[SerializeField]
	private Color m_DefaultColor = Color.black;

	[SerializeField]
	private Color m_BrightColor = Color.white;

	[SerializeField]
	private float m_DefaultFontSize = 22f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 22f;

	protected override void OnBind()
	{
		base.OnBind();
		m_Text.text = base.ViewModel.Text;
		if ((bool)m_RightDecoration && (bool)m_LeftDecoration)
		{
			m_RightDecoration.SetActive(base.ViewModel.IsHeader);
			m_LeftDecoration.SetActive(base.ViewModel.IsHeader);
		}
		if ((bool)m_LayoutGroup)
		{
			ApplyAlignment(base.ViewModel.Alignment);
		}
		if ((bool)m_LineAndBackground)
		{
			m_LineAndBackground.SetActive(base.ViewModel.IsOverline);
		}
		ApplyStyle(base.ViewModel.Type);
		if (base.ViewModel.NeedChangeSize)
		{
			ChangeTextSize();
		}
		m_Text.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)).AddTo(this);
	}

	private void ApplyStyle(TooltipTextType type)
	{
		ApplyStyleTo(m_Text, type);
	}

	protected void ApplyStyleTo(TextMeshProUGUI text, TooltipTextType type)
	{
		text.paragraphSpacing = 0f;
		text.fontStyle = FontStyles.Normal;
		text.alignment = TextAlignmentOptions.Left;
		text.color = m_DefaultColor;
		if (type.HasFlag(TooltipTextType.Paragraph))
		{
			text.paragraphSpacing = 60f;
		}
		if (type.HasFlag(TooltipTextType.LevelUpLineSpacing))
		{
			text.lineSpacing = 22.5f;
		}
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
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		if (type.HasFlag(TooltipTextType.GlossarySize))
		{
			text.fontSize = 20f * m_FontMultiplier;
		}
		else if (!type.HasFlag(TooltipTextType.GlossarySize))
		{
			text.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * m_FontMultiplier;
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
		switch (alignment)
		{
		case TooltipTextAlignment.Right:
			m_LayoutGroup.childAlignment = TextAnchor.MiddleRight;
			break;
		case TooltipTextAlignment.Midl:
			m_LayoutGroup.childAlignment = TextAnchor.MiddleCenter;
			break;
		case TooltipTextAlignment.Left:
			m_LayoutGroup.childAlignment = TextAnchor.MiddleLeft;
			break;
		}
	}

	protected virtual void ChangeTextSize()
	{
		m_Text.enableAutoSizing = false;
		m_Text.fontSize = (float)base.ViewModel.TextSize * m_FontMultiplier;
	}
}
