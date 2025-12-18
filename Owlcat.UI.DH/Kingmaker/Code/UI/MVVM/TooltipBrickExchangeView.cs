using System;
using System.Collections.Generic;
using Code.Framework.Utility.UnityExtensions;
using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickExchangeView : TooltipBaseBrickView<TooltipBrickExchangeVM>
{
	[Serializable]
	private class TooltipTextSettings
	{
		public float DefaultFontSizeLabel;

		public float DefaultFontSizeValue;

		public float DefaultFontSizeAddValue;

		public float DefaultFontSizeItemType;

		public float DefaultConsoleFontSizeLabel;

		public float DefaultConsoleFontSizeValue;

		public float DefaultConsoleFontSizeAddValue;

		public float DefaultConsoleFontSizeItemType;
	}

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private TextMeshProUGUI m_AddValue;

	[SerializeField]
	private TextMeshProUGUI m_ItemType;

	[SerializeField]
	private Image m_Background;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private float m_DefaultIconSize = 30f;

	[SerializeField]
	private TextMeshProUGUI m_IconText;

	[SerializeField]
	private List<GameObject> m_HasValueGroup = new List<GameObject>();

	[Header("Text Colors")]
	[SerializeField]
	private Color m_NormalColor;

	[SerializeField]
	private Color m_PositiveColor;

	[SerializeField]
	private Color m_NegativeColor;

	[Header("Background Colors")]
	[SerializeField]
	private Color m_NormalBackgroundColor;

	[SerializeField]
	private Color m_PositiveBackgroundColor;

	[SerializeField]
	private Color m_NegativeBackgroundColor;

	[SerializeField]
	private float m_NonNormalWidthExpanse;

	[Header("TextSize")]
	[SerializeField]
	private TooltipTextSettings m_TextSettings;

	private bool m_IsExpansed;

	private Color m_DefaultIconColor;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		m_TextHelper = new AccessibilityTextHelper(m_Label, m_Value, m_AddValue);
		m_DefaultIconColor = m_Icon.color;
		base.OnBind();
		m_Label.text = base.ViewModel.Name;
		m_Value.text = $"x {base.ViewModel.Value}";
		m_ItemType.text = base.ViewModel.ItemType;
		UpdateAddValue(base.ViewModel.AddValue);
		m_AddValue.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)).AddTo(this);
		m_Icon.gameObject.SetActive(base.ViewModel.Icon != null);
		m_Icon.sprite = base.ViewModel.Icon;
		if (base.ViewModel.IconColor.HasValue)
		{
			m_Icon.color = base.ViewModel.IconColor.Value;
		}
		SetIconSize(base.ViewModel.IconSize);
		if ((bool)m_IconText)
		{
			m_IconText.gameObject.SetActive(!string.IsNullOrEmpty(base.ViewModel.IconText));
		}
		if ((bool)m_IconText && !string.IsNullOrEmpty(base.ViewModel.IconText))
		{
			m_IconText.text = base.ViewModel.IconText;
		}
		if (!string.IsNullOrEmpty(base.ViewModel.ValueHint))
		{
			m_Value.SetHint(base.ViewModel.ValueHint).AddTo(this);
		}
		foreach (GameObject item in m_HasValueGroup)
		{
			item.SetActive(base.ViewModel.HasValue);
		}
		ApplyStyle();
		SetTooltip();
		if (base.ViewModel.ReactiveValue != null)
		{
			base.ViewModel.ReactiveValue.Subscribe(delegate(string value)
			{
				m_Value.text = value;
			}).AddTo(this);
		}
		if (base.ViewModel.ReactiveAddValue != null)
		{
			base.ViewModel.ReactiveAddValue.Subscribe(UpdateAddValue).AddTo(this);
		}
		SetTextSize();
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_Icon.color = m_DefaultIconColor;
		SetIconSize(m_DefaultIconSize);
		m_TextHelper.Dispose();
	}

	private void UpdateAddValue(string value)
	{
		m_AddValue.gameObject.SetActive(!value.IsNullOrEmpty());
		m_AddValue.text = value;
	}

	private void ApplyStyle()
	{
		ApplyTextColor();
		ApplyBackgroundColor();
		ApplyFontStyle();
		AdjustBackgroundSize();
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}

	private void ApplyTextColor()
	{
		Color color = base.ViewModel.Type switch
		{
			TooltipBrickIconStatValueType.Normal => m_NormalColor, 
			TooltipBrickIconStatValueType.Positive => m_PositiveColor, 
			TooltipBrickIconStatValueType.Negative => m_NegativeColor, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		TextMeshProUGUI value = m_Value;
		Color color3 = (m_AddValue.color = color);
		value.color = color3;
	}

	private void ApplyBackgroundColor()
	{
		if (!(m_Background == null))
		{
			Color color = base.ViewModel.BackgroundType switch
			{
				TooltipBrickIconStatValueType.Normal => m_NormalBackgroundColor, 
				TooltipBrickIconStatValueType.Positive => m_PositiveBackgroundColor, 
				TooltipBrickIconStatValueType.Negative => m_NegativeBackgroundColor, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			m_Background.color = color;
		}
	}

	private void AdjustBackgroundSize()
	{
		if (!(m_Background == null))
		{
			RectTransform component = m_Background.GetComponent<RectTransform>();
			Vector2 sizeDelta = component.sizeDelta;
			if (base.ViewModel.BackgroundType != TooltipBrickIconStatValueType.Positive && m_IsExpansed)
			{
				sizeDelta.x -= m_NonNormalWidthExpanse;
				m_IsExpansed = false;
			}
			else if (base.ViewModel.BackgroundType == TooltipBrickIconStatValueType.Positive && !m_IsExpansed)
			{
				sizeDelta.x += m_NonNormalWidthExpanse;
				m_IsExpansed = true;
			}
			component.sizeDelta = sizeDelta;
		}
	}

	private void ApplyFontStyle()
	{
		m_Label.fontStyle = ((base.ViewModel.TextStyle == TooltipBrickIconStatValueStyle.Bold) ? FontStyles.Bold : FontStyles.Normal);
	}

	private void SetTextSize()
	{
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		m_Label.fontSize = (isControllerMouse ? m_TextSettings.DefaultFontSizeLabel : m_TextSettings.DefaultConsoleFontSizeLabel) * m_FontMultiplier;
		m_Value.fontSize = (isControllerMouse ? m_TextSettings.DefaultFontSizeValue : m_TextSettings.DefaultConsoleFontSizeValue) * m_FontMultiplier;
		m_AddValue.fontSize = (isControllerMouse ? m_TextSettings.DefaultFontSizeAddValue : m_TextSettings.DefaultConsoleFontSizeAddValue) * m_FontMultiplier;
		m_ItemType.fontSize = (isControllerMouse ? m_TextSettings.DefaultFontSizeItemType : m_TextSettings.DefaultConsoleFontSizeItemType) * m_FontMultiplier;
	}

	private void SetTooltip()
	{
		this.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
	}

	private void SetIconSize(float? width)
	{
		LayoutElement component = m_Icon.GetComponent<LayoutElement>();
		if ((bool)component)
		{
			component.minWidth = width ?? m_DefaultIconSize;
		}
	}
}
