using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UIUtilityShader
{
	public static void FindMinMaxPositions(in List<UIVertex> vertexStream, out Vector3 min, out Vector3 max, out float width, out float height, bool useSorting = false)
	{
		List<UIVertex> list = vertexStream;
		if (list == null || list.Count <= 1)
		{
			min = Vector3.zero;
			max = Vector3.zero;
			width = 0f;
			height = 0f;
			return;
		}
		if (!useSorting)
		{
			min = vertexStream[0].position;
			List<UIVertex> obj = vertexStream;
			max = obj[obj.Count - 1].position;
		}
		else
		{
			min = vertexStream.MinBy((UIVertex vert) => vert.position.x + vert.position.y).position;
			max = vertexStream.MaxBy((UIVertex vert) => vert.position.x + vert.position.y).position;
		}
		width = max.x - min.x;
		height = max.y - min.y;
	}
}
