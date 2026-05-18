using System.Collections.Generic;
using Pathfinding;

namespace Kingmaker.Pathfinding;

public class TraversalProviderWithBusyLinkPenalties : ITraversalProvider
{
	private class BusyLink
	{
		public int SystemStep;

		public int TraverserId;
	}

	private Dictionary<LinkNode, List<BusyLink>> m_BusyLinks = new Dictionary<LinkNode, List<BusyLink>>();

	public uint GetTraversalCost(Path path, GraphNode node)
	{
		uint traversalCost = DefaultITraversalProvider.GetTraversalCost(path, node);
		if (!(node is LinkNode key))
		{
			return traversalCost;
		}
		lock (m_BusyLinks)
		{
			if (!m_BusyLinks.TryGetValue(key, out var value))
			{
				return traversalCost;
			}
			uint num = 0u;
			foreach (BusyLink item in value)
			{
				if (Game.Instance.RealTimeController.CurrentSystemStepIndex - item.SystemStep <= 100)
				{
					num++;
				}
			}
			return traversalCost + num * 1000;
		}
	}

	public void RegisterPath(Path path, int traverserId)
	{
		if (path.path == null)
		{
			return;
		}
		lock (m_BusyLinks)
		{
			foreach (GraphNode item in path.path)
			{
				if (!(item is LinkNode key))
				{
					continue;
				}
				if (!m_BusyLinks.TryGetValue(key, out var value))
				{
					value = new List<BusyLink>();
					m_BusyLinks[key] = value;
				}
				BusyLink busyLink = new BusyLink
				{
					SystemStep = Game.Instance.RealTimeController.CurrentSystemStepIndex,
					TraverserId = traverserId
				};
				for (int i = 0; i < value.Count; i++)
				{
					if (value[i].TraverserId == traverserId)
					{
						value[i] = busyLink;
						return;
					}
				}
				value.Add(busyLink);
			}
		}
	}

	public void UnregisterPath(ForcedPath path, int traverserId)
	{
		if (path.path == null)
		{
			return;
		}
		lock (m_BusyLinks)
		{
			foreach (GraphNode item in path.path)
			{
				if (!(item is LinkNode key) || !m_BusyLinks.TryGetValue(key, out var value))
				{
					continue;
				}
				for (int i = 0; i < value.Count; i++)
				{
					if (value[i].TraverserId == traverserId)
					{
						value.RemoveAt(i);
						return;
					}
				}
			}
		}
	}

	public void UnregisterNodes(IEnumerable<LinkNode> nodes, int traverserId)
	{
		lock (m_BusyLinks)
		{
			foreach (LinkNode node in nodes)
			{
				if (!m_BusyLinks.TryGetValue(node, out var value))
				{
					continue;
				}
				for (int i = 0; i < value.Count; i++)
				{
					if (value[i].TraverserId == traverserId)
					{
						value.RemoveAt(i);
						return;
					}
				}
			}
		}
	}
}
