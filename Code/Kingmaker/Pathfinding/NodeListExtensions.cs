using System;
using Pathfinding;

namespace Kingmaker.Pathfinding;

public static class NodeListExtensions
{
	public static bool NonSingle(this ref NodeList nodeList)
	{
		bool flag = false;
		foreach (GridNodeBase node in nodeList)
		{
			_ = node;
			if (flag)
			{
				return true;
			}
			flag = true;
		}
		return false;
	}

	public static GridNodeBase First(this ref NodeList nodeList)
	{
		using (NodeList.Enumerator enumerator = nodeList.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		throw new InvalidOperationException("Node list is empty");
	}
}
