using System;
using UnityEngine;

namespace Owlcat.UI;

public class ColorPalleteItem : ScriptableObject
{
	public ColorPallete Pallete;

	public Color Color = Color.white;

	public event Action Changed;

	private void OnValidate()
	{
		this.Changed?.Invoke();
	}
}
