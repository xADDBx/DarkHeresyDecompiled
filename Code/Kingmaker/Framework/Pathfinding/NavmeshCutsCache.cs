using System;
using System.Linq;
using Pathfinding;
using Unity.Collections;
using UnityEngine;

namespace Kingmaker.Framework.Pathfinding;

public sealed class NavmeshCutsCache : IDisposable
{
	private readonly GridGraph _graph;

	private NativeArray<Rect> _bounds;

	private bool _dirty = true;

	public NativeArray<Rect> Bounds
	{
		get
		{
			Update();
			return _bounds;
		}
	}

	public NavmeshCutsCache(GridGraph graph)
	{
		_graph = graph;
	}

	private void Update(bool force = false)
	{
		if (_dirty || force)
		{
			Rect[] array = (from i in NavmeshClipper.allEnabled.OfType<NavmeshCut>()
				select i.GetBounds(_graph.transform, 0f)).ToArray();
			_bounds.Dispose();
			_bounds = new NativeArray<Rect>(array.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			for (int j = 0; j < array.Length; j++)
			{
				_bounds[j] = array[j];
			}
		}
	}

	public void Invalidate()
	{
		_dirty = true;
	}

	public void Dispose()
	{
		_bounds.Dispose();
	}
}
