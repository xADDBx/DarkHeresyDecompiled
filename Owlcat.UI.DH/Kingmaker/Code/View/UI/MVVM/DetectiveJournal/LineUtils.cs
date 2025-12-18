using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public static class LineUtils
{
	public static Vector3 ToVector(this LineDirection direction)
	{
		return direction.GetDirectionVector();
	}

	public static Vector3 GetDirectionVector(this LineDirection direction)
	{
		return direction switch
		{
			LineDirection.Left => Vector2.left, 
			LineDirection.Right => Vector2.right, 
			LineDirection.Up => Vector2.up, 
			LineDirection.Down => Vector2.down, 
			_ => Vector2.zero, 
		};
	}
}
