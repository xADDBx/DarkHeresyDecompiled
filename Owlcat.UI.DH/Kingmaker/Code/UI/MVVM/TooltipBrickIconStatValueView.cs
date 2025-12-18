using Code.Framework.Utility.UnityExtensions;
using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickIconStatValueView : TooltipBaseBrickView<TooltipBrickIconStatValueVM>
{
	[Header("Elements")]
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private TextMeshProUGUI m_AddValue;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_IconText;

	[SerializeField]
	private Image m_Background;

	[Header("Selectables")]
	[SerializeField]
	private OwlcatMultiSelectable m_HasValueSelectable;

	[SerializeField]
	private OwlcatMultiSelectable m_TextStateSelectable;

	[SerializeField]
	private OwlcatMultiSelectable m_BgrStateSelectable;

	[Header("Values")]
	[SerializeField]
	private float m_DefaultIconSize = 30f;

	[SerializeField]
	private float m_NonNormalWidthExpanse;

	private bool m_IsExpansed;

	private Color m_DefaultIconColor;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		m_TextHelper = new AccessibilityTextHelper(m_Label, m_Value, m_AddValue).AddTo(this);
		m_DefaultIconColor = m_Icon.color;
		base.OnBind();
		m_Label.text = base.ViewModel.Name;
		m_Value.text = base.ViewModel.Value;
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
		m_HasValueSelectable.SetActiveLayer(base.ViewModel.HasValue ? "HasValues" : "NoValues");
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
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_Icon.color = m_DefaultIconColor;
		SetIconSize(m_DefaultIconSize);
		m_TextHelper.Dispose();
		m_TextHelper = null;
	}

	private void UpdateAddValue(string value)
	{
		m_AddValue.gameObject.SetActive(!value.IsNullOrEmpty());
		m_AddValue.text = value;
	}

	private void ApplyStyle()
	{
		m_TextStateSelectable.SetActiveLayer(base.ViewModel.Type.ToString());
		m_BgrStateSelectable.SetActiveLayer(base.ViewModel.BackgroundType.ToString());
		if ((bool)m_Background)
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
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
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
