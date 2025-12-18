using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class WarhammerXPath : ABPath, ILinkTraversePath
{
	public ILinkTraversalProvider LinkTraversalProvider { get; set; }

	public new static WarhammerXPath Construct(Vector3 start, Vector3 end, OnPathDelegate callback = null)
	{
		WarhammerXPath warhammerXPath = PathPool.GetPath<WarhammerXPath>();
		warhammerXPath.Setup(start, end, callback);
		return warhammerXPath;
	}

	protected override void OnEnterPool()
	{
		base.OnEnterPool();
		LinkTraversalProvider = null;
	}
}
