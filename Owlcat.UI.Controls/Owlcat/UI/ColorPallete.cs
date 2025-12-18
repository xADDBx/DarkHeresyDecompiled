using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.UI;

[CreateAssetMenu(fileName = "ColorPallete", menuName = "ScriptableObject/Owlcat/UI/ColorPallete")]
public class ColorPallete : ScriptableObject
{
	[SerializeField]
	private List<ColorPalleteItem> m_Colors = new List<ColorPalleteItem>();

	public List<ColorPalleteItem> Colors => m_Colors;
}
