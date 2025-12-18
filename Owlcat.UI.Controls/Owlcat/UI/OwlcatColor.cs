using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Owlcat.UI;

[ExecuteAlways]
public class OwlcatColor : UIBehaviour
{
	[SerializeField]
	private Graphic m_Graphic;

	[SerializeField]
	[ColorPalleteItemPicker]
	private ColorPalleteItem m_Color;

	protected override void OnEnable()
	{
		if ((bool)m_Color)
		{
			m_Color.Changed += OnColorChanged;
		}
		OnColorChanged();
	}

	protected override void OnDisable()
	{
		if ((bool)m_Color)
		{
			m_Color.Changed -= OnColorChanged;
		}
	}

	private void OnColorChanged()
	{
		if ((bool)m_Graphic && (bool)m_Color)
		{
			m_Graphic.color = m_Color.Color;
		}
	}
}
