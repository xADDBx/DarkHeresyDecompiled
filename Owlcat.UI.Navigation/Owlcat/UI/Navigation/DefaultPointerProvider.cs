using UnityEngine;

namespace Owlcat.UI.Navigation;

public class DefaultPointerProvider : IPointerProvider
{
	public bool Enabled
	{
		get
		{
			return Cursor.visible;
		}
		set
		{
			Cursor.visible = value;
		}
	}

	public Vector2 Position
	{
		get
		{
			return Input.mousePosition;
		}
		set
		{
		}
	}
}
