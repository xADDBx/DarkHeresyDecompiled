using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public readonly struct WarhammerPathPlayerCell
{
	public readonly Vector3 Position;

	public readonly int DiagonalsCount;

	public readonly float Length;

	public readonly GraphNode Node;

	public readonly GraphNode ParentNode;

	public readonly bool IsCanStand;

	public readonly bool IsOneWayPath;

	public WarhammerPathPlayerCell(Vector3 position, int diagonalsCount, float length, GraphNode node, GraphNode parentNode, bool isCanStand, bool isOneWayPath)
	{
		Position = position;
		DiagonalsCount = diagonalsCount;
		Length = length;
		Node = node;
		ParentNode = parentNode;
		IsCanStand = isCanStand;
		IsOneWayPath = isOneWayPath;
	}

	public override string ToString()
	{
		return $"{Node.AsString()} ({Position}) [Len={Length}, CanStand={IsCanStand}, DiagCount={DiagonalsCount}]";
	}
}
