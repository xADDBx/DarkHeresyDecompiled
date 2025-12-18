using Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.AR;

public class GridHeightTextureGeneratorUtil
{
	public static Texture2D Generate(GridGraph graph, float impassableTerrainHeight)
	{
		if (graph == null)
		{
			return null;
		}
		Texture2D texture2D = new Texture2D(graph.width, graph.depth, TextureFormat.RFloat, mipChain: false, linear: true);
		texture2D.filterMode = FilterMode.Point;
		Color color = new Color(impassableTerrainHeight, impassableTerrainHeight, impassableTerrainHeight, impassableTerrainHeight);
		GridNodeBase[] nodes = graph.nodes;
		if (nodes == null || nodes.Length == 0)
		{
			return null;
		}
		foreach (GridNodeBase gridNodeBase in nodes)
		{
			if (gridNodeBase.Walkable)
			{
				Vector3 vector = (Vector3)gridNodeBase.position;
				texture2D.SetPixel(color: new Color(vector.y, vector.y, vector.y, vector.y), x: gridNodeBase.XCoordinateInGrid, y: gridNodeBase.ZCoordinateInGrid);
			}
			else
			{
				texture2D.SetPixel(gridNodeBase.XCoordinateInGrid, gridNodeBase.ZCoordinateInGrid, color);
			}
		}
		texture2D.Apply();
		return texture2D;
	}
}
