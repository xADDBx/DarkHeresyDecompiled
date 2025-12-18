using System;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

[Serializable]
public class LineDirectionData
{
	public RectTransform Dot;

	public LineDirection Direction;

	public float Length;

	public LineDirectionData()
	{
	}

	public LineDirectionData(RectTransform dot, LineDirection direction, float length)
	{
		Dot = dot;
		Direction = direction;
		Length = length;
	}

	public LineDirectionData(LineDirectionData other)
	{
		Dot = other.Dot;
		Direction = other.Direction;
		Length = other.Length;
	}
}
