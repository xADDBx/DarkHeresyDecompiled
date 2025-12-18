using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class ReplaceNodeLinkPositionsModifier : PathModifier
{
	public bool TurnBased;

	public override int Order => 20;

	public override void Apply(Path path)
	{
		if (path.path.Count != path.vectorPath.Count)
		{
			PFLog.Pathfinding.Error("ReplaceNodeLinkPositionsModifier: path.path.Count != path.vectorPath.Count: path was already processed by some other modifier that does not preserve index correspondece");
			return;
		}
		for (int i = 0; i < path.path.Count; i++)
		{
			if (!(path.path[i] is LinkNode))
			{
				continue;
			}
			if (TurnBased)
			{
				if (i != 0 && i != path.path.Count - 1)
				{
					if (path.path[i - 1] is LinkNode)
					{
						path.vectorPath[i] = path.vectorPath[i + 1];
					}
					else
					{
						path.vectorPath[i] = path.vectorPath[i - 1];
					}
				}
			}
			else
			{
				path.vectorPath[i] = (Vector3)AstarPath.active.data.gridGraph.GetNearest(path.vectorPath[i]).node.position;
			}
		}
	}
}
