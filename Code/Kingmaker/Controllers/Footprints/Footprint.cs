using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.Controllers.Footprints;

internal class Footprint
{
	[NotNull]
	public readonly List<Footprint> Pool;

	public readonly GameObject GameObject;

	public readonly Transform Transform;

	public readonly MeshRenderer MeshRenderer;

	public List<Footprint> UnitList;

	public float? TimeLeft;

	public float? FadeOutTime;

	public Footprint(List<Footprint> pool, MeshRenderer meshRenderer)
	{
		Pool = pool;
		GameObject = meshRenderer.gameObject;
		Transform = meshRenderer.transform.transform;
		MeshRenderer = meshRenderer;
	}
}
