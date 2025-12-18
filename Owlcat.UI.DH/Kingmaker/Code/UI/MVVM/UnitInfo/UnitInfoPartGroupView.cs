using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoPartGroupView : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_Header;

	[SerializeField]
	private RectTransform m_Container;

	[SerializeField]
	private Image m_Separator;

	private Color m_SeparatorInitialColor;

	public RectTransform Container => m_Container;

	private void Awake()
	{
		if (!(m_Separator == null))
		{
			m_SeparatorInitialColor = m_Separator.color;
		}
	}

	public void SetHeader(string header)
	{
		m_Header.text = header;
	}

	public void SetSeparator(bool hasSeparator)
	{
		if (!(m_Separator == null))
		{
			m_Separator.color = (hasSeparator ? m_SeparatorInitialColor : Color.clear);
		}
	}

	public void SetActive(bool active)
	{
		base.gameObject.SetActive(active);
	}
}
