using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public static class CluesPositionsUtils
{
	private static readonly Vector2 RestrictedSize = new Vector2(325f, 425f);

	public static List<Vector2> GetPlacementPoints(Vector2 container, Vector2Int gridSize, bool useShift = true)
	{
		float x = container.x;
		float y = container.y;
		List<Vector2> list = new List<Vector2>();
		Vector2 vector = new Vector2(x / (float)(gridSize.x + 1), y / (float)(gridSize.y + 1));
		for (int i = 0; i < gridSize.x; i++)
		{
			int y2 = gridSize.y;
			for (int j = 0; j < y2; j++)
			{
				Vector2 item = new Vector2((0f - x) * 0.5f + (float)(i + 1) * vector.x, (0f - y) * 0.5f + (float)(j + 1) * vector.y);
				item.y += ((useShift && i % 2 == 0) ? 0f : (vector.y * 0.5f));
				if (!(Math.Abs(item.x) < RestrictedSize.x) || !(Math.Abs(item.y) < RestrictedSize.y))
				{
					list.Add(item);
				}
			}
		}
		return list;
	}
}
