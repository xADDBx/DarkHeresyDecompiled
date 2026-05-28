using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Pool;

namespace Owlcat.UI.Navigation;

internal class HierarhyNavigationGraph : INavigationGraph
{
	private readonly GameObject m_Root;

	public HierarhyNavigationGraph(GameObject root)
	{
		m_Root = root;
	}

	public bool TryGet([NotNull] GameObject selected, Vector2 dir, out GameObject result)
	{
		List<INavigationGraph> value;
		using (CollectionPool<List<INavigationGraph>, INavigationGraph>.Get(out value))
		{
			FocusUtility.GetHierarhyNonAlloc(m_Root, selected, value);
			for (int num = value.Count - 1; num >= 0; num--)
			{
				if (value[num].TryGet(selected, dir, out result))
				{
					return true;
				}
			}
			result = null;
			return false;
		}
	}
}
