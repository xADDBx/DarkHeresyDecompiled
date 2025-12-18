using System;
using Pathfinding;

namespace Kingmaker.Pathfinding.LosCaching;

public class QuadtreeLosCache : LosCache
{
	private readonly CustomGridQuadtree quadtree;

	public QuadtreeLosCache(GridGraph graph, IntRect bounds)
		: base(graph, bounds)
	{
		quadtree = new CustomGridQuadtree(graph, bounds);
		if (!IsFlatGraph(graph, bounds))
		{
			quadtree.DivideDown();
			quadtree.ConsolidateUpPairwise(LosCache.CanConsolidateByLos, 32);
		}
	}

	private bool IsFlatGraph(GridGraph graph, IntRect bounds)
	{
		float num = float.MaxValue;
		float num2 = float.MinValue;
		float num3 = 1f;
		GridNodeBase[] nodes = graph.nodes;
		foreach (GridNodeBase gridNodeBase in nodes)
		{
			if (bounds.Contains(gridNodeBase.XCoordinateInGrid, gridNodeBase.ZCoordinateInGrid))
			{
				num = Math.Min(num, gridNodeBase.Vector3Position().y);
				num2 = Math.Max(num2, gridNodeBase.Vector3Position().y);
				if (num2 - num > num3)
				{
					return false;
				}
			}
		}
		return true;
	}

	public override bool CheckLos(GridNodeBase origin, IntRect originSize, GridNodeBase end, IntRect endSize)
	{
		for (int i = originSize.xmin; i <= originSize.xmax; i++)
		{
			for (int j = originSize.ymin; j <= originSize.ymax; j++)
			{
				GridNodeBase node = graph.GetNode(origin.XCoordinateInGrid + i, origin.ZCoordinateInGrid + j);
				for (int k = endSize.xmin; k <= endSize.xmax; k++)
				{
					for (int l = endSize.ymin; l <= endSize.ymax; l++)
					{
						GridNodeBase node2 = graph.GetNode(end.XCoordinateInGrid + k, end.ZCoordinateInGrid + l);
						if (CheckLos(node, node2))
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	public bool CheckLos(GridNodeBase origin, GridNodeBase end)
	{
		return quadtree.Find(origin.XCoordinateInGrid, origin.ZCoordinateInGrid)?.Contains(end.XCoordinateInGrid, end.ZCoordinateInGrid) ?? false;
	}

	public override void DebugDraw()
	{
		DrawQuadtree(quadtree);
	}

	private void DrawQuadtree(CustomGridQuadtree tree)
	{
		if (tree != null)
		{
			if (tree.IsLeaf)
			{
				DrawRect(tree.Rect);
			}
			DrawQuadtree(tree.LeftTopChild);
			DrawQuadtree(tree.RightTopChild);
			DrawQuadtree(tree.LeftBottomChild);
			DrawQuadtree(tree.RightBottomChild);
		}
	}
}
