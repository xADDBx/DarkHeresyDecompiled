using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Owlcat.UI.Navigation;

internal class CompositeNavigationGraph : INavigationGraph
{
	private readonly INavigationGraph[] m_Graphs;

	public CompositeNavigationGraph(params INavigationGraph[] graphs)
	{
		m_Graphs = graphs;
	}

	public bool TryGet([NotNull] GameObject selected, Vector2 dir, out GameObject result)
	{
		INavigationGraph[] graphs = m_Graphs;
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i].TryGet(selected, dir, out result))
			{
				return true;
			}
		}
		result = null;
		return false;
	}
}
