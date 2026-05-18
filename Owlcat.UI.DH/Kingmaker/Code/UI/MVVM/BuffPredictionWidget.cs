using System;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BuffPredictionWidget : MonoBehaviour
{
	[Serializable]
	private struct ValueColor
	{
		public Color Color;

		public int RangeFrom;
	}

	[SerializeField]
	private Image m_Image;

	[SerializeField]
	private TMP_Text m_Value;

	[SerializeField]
	private Graphic[] m_GraphicsToColor;

	[SerializeField]
	private ValueColor[] m_ValueColors;

	[SerializeField]
	private Color m_DefaultColor;

	public void SetActive(bool isActive)
	{
		base.gameObject.SetActive(isActive);
	}

	public void SetIcon(Sprite icon)
	{
		m_Image.sprite = icon;
	}

	public void SetValue(int value)
	{
		Color color = GetColor(value);
		Graphic[] graphicsToColor = m_GraphicsToColor;
		for (int i = 0; i < graphicsToColor.Length; i++)
		{
			graphicsToColor[i].color = color;
		}
		m_Value.text = value.ToString(CultureInfo.InvariantCulture);
	}

	private Color GetColor(int value)
	{
		ValueColor[] valueColors = m_ValueColors;
		for (int i = 0; i < valueColors.Length; i++)
		{
			ValueColor valueColor = valueColors[i];
			if (value >= valueColor.RangeFrom)
			{
				return valueColor.Color;
			}
		}
		return m_DefaultColor;
	}

	private void OnValidate()
	{
		m_ValueColors = m_ValueColors.OrderByDescending((ValueColor v) => v.RangeFrom).ToArray();
	}
}
