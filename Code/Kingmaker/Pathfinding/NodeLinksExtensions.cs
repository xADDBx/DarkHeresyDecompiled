using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public static class NodeLinksExtensions
{
	public static bool AreConnected(GraphNode from, GraphNode to, out IWarhammerNodeLink currentLink)
	{
		currentLink = null;
		if (!(from is GridNodeBase gridNodeBase) || !(to is GridNodeBase))
		{
			return false;
		}
		if (gridNodeBase.connections == null)
		{
			return false;
		}
		for (int i = 0; i < gridNodeBase.connections.Length; i++)
		{
			LinkNode linkNode = gridNodeBase.connections[i].node as LinkNode;
			if (to == linkNode.linkConcrete.startNodes[0] || to == linkNode.linkConcrete.endNodes[0])
			{
				currentLink = linkNode.linkSource.component as IWarhammerNodeLink;
				return true;
			}
		}
		return false;
	}

	public static bool AreConnected(Vector3 from, Vector2 to, out GraphNode fromNode, out GraphNode toNode, out WarhammerNodeLink currentLink)
	{
		fromNode = null;
		toNode = null;
		currentLink = null;
		GridNodeBase gridNodeBase = (GridNodeBase)ObstacleAnalyzer.GetNearestNode(from).node;
		if (gridNodeBase.connections == null)
		{
			return false;
		}
		for (int i = 0; i < gridNodeBase.connections.Length; i++)
		{
			Connection connection = gridNodeBase.connections[i];
			LinkNode linkNode = connection.node as LinkNode;
			GraphNode graphNode = ((linkNode.linkConcrete.startNodes[0] == gridNodeBase) ? linkNode.linkConcrete.endNodes[0] : linkNode.linkConcrete.startNodes[0]);
			if ((double)(to - graphNode.Vector3Position().To2D()).sqrMagnitude < 1E-06 || (double)(to - linkNode.Vector3Position().To2D()).sqrMagnitude < 1E-06)
			{
				currentLink = NodeLink2.GetNodeLink(connection.node) as WarhammerNodeLink;
				toNode = graphNode;
				fromNode = gridNodeBase;
				return true;
			}
		}
		return false;
	}

	public static bool IsNodeLinked(this GraphNode node)
	{
		if (node is GridNodeBase { connections: var connections })
		{
			if (connections != null)
			{
				return connections.Length > 0;
			}
			return false;
		}
		return false;
	}
}
