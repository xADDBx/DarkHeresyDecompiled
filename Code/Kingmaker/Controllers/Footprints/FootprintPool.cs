using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Footprints;

internal class FootprintPool
{
	private readonly Dictionary<GameObject, List<Footprint>> _pools = new Dictionary<GameObject, List<Footprint>>();

	public Footprint Rent(GameObject prefab, bool leftSided)
	{
		if (!_pools.TryGetValue(prefab, out var value))
		{
			value = new List<Footprint>();
			_pools.Add(prefab, value);
		}
		Footprint footprint = value.LastItem();
		if (footprint == null)
		{
			GameObject gameObject = Object.Instantiate(prefab, FxHelper.FootprintsRoot);
			if (leftSided)
			{
				Vector3 localScale = gameObject.transform.localScale;
				localScale.x *= -1f;
				gameObject.transform.localScale = localScale;
			}
			footprint = new Footprint(value, gameObject.GetComponentNonAlloc<MeshRenderer>());
		}
		else
		{
			value.RemoveLast();
		}
		return footprint;
	}

	public void Return(Footprint footprint)
	{
		Cleanup(footprint);
		footprint.Pool.Add(footprint);
	}

	public void Clear()
	{
		foreach (KeyValuePair<GameObject, List<Footprint>> pool in _pools)
		{
			foreach (Footprint item in pool.Value)
			{
				if (item.GameObject != null)
				{
					Object.Destroy(item.GameObject);
				}
			}
		}
		_pools.Clear();
	}

	private static void Cleanup(Footprint footprint)
	{
		footprint.UnitList = null;
		footprint.TimeLeft = null;
		footprint.FadeOutTime = null;
		Object.Destroy(footprint.MeshRenderer.material);
		footprint.GameObject.SetActive(value: false);
	}
}
