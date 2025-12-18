using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class WarhammerABPath : ABPath, ILinkTraversePath
{
	public ILinkTraversalProvider LinkTraversalProvider { get; set; }

	public new static WarhammerABPath Construct(Vector3 start, Vector3 end, OnPathDelegate callback = null)
	{
		WarhammerABPath warhammerABPath = PathPool.GetPath<WarhammerABPath>();
		warhammerABPath.Setup(start, end, callback);
		return warhammerABPath;
	}

	protected override void OnEnterPool()
	{
		base.OnEnterPool();
		LinkTraversalProvider = null;
	}
}
