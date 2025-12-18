using System;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoPartElementView : MonoBehaviour
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private RectTransform m_Container;

	[SerializeField]
	private OwlcatMultiSelectable m_Selectable;

	public void SetIcon(Sprite sprite)
	{
		m_Icon.sprite = sprite;
	}

	public void SetName(string text)
	{
		m_Label.text = text;
	}

	public void SetValue(string text)
	{
		m_Value.text = text;
	}

	public void SetActive(bool active)
	{
		if ((bool)m_Container)
		{
			m_Container.gameObject.SetActive(active);
		}
	}

	public IDisposable SetHint(string hintText)
	{
		return m_Selectable.SetHint(hintText);
	}

	public IDisposable SetTooltip(TooltipBaseTemplate tooltip)
	{
		return m_Selectable.SetTooltip(tooltip);
	}
}
