using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoPartCompareElementView : MonoBehaviour
{
	[SerializeField]
	private UnitInfoPartElementView m_ElementView;

	[SerializeField]
	private Image m_CompareIcon;

	[SerializeField]
	private Color m_IncreasedColor;

	[SerializeField]
	private Color m_DecreasedColor;

	[SerializeField]
	private Color m_DefaultColor = Color.white;

	public UnitInfoPartElementView ElementView => m_ElementView;

	public void SetValueChangeMarker(bool valueChanged, bool valueIncreased)
	{
		Color color = ((!valueChanged) ? m_DefaultColor : (valueIncreased ? m_IncreasedColor : m_DecreasedColor));
		m_CompareIcon.color = color;
	}

	public void SetIcon(Sprite sprite)
	{
		m_ElementView.SetIcon(sprite);
	}

	public void SetName(string text)
	{
		m_ElementView.SetName(text);
	}

	public void SetValue(string text)
	{
		m_ElementView.SetValue(text);
	}
}
