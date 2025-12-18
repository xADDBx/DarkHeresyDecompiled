using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoPartHeader : MonoBehaviour
{
	[Serializable]
	public struct Colors
	{
		public Color Background;

		public Color Brackets;

		public Color Title;

		public Color Value;
	}

	[SerializeField]
	private TMP_Text m_TitleLabel;

	[SerializeField]
	private TMP_Text m_ValueLabel;

	[SerializeField]
	private Image m_BackgroundImage;

	[SerializeField]
	private Image m_BracketsImage;

	[SerializeField]
	private RectTransform m_BackgroundRectTransform;

	[SerializeField]
	private float m_BackgrounRectPadding;

	[SerializeField]
	private float m_BackgrounRectMinWidth;

	public void SetTitle(string text)
	{
		if (!(this == null))
		{
			m_TitleLabel.SetText(text);
		}
	}

	public void SetValue(string text)
	{
		if (!(this == null))
		{
			m_ValueLabel.SetText(text);
			float size = Mathf.Max(m_BackgrounRectMinWidth, m_ValueLabel.preferredWidth + m_BackgrounRectPadding);
			m_BackgroundRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
		}
	}

	public void SetColors(Colors colors)
	{
		m_TitleLabel.color = colors.Title;
		m_ValueLabel.color = colors.Value;
		m_BackgroundImage.color = colors.Background;
		m_BracketsImage.color = colors.Brackets;
	}

	public void SetActive(bool isActive)
	{
		base.gameObject.SetActive(isActive);
	}
}
