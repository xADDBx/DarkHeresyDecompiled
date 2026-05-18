using UnityEngine;

namespace Kingmaker.Controllers.Footprints;

internal readonly struct FootLocator
{
	public readonly Transform Transform;

	public readonly bool LeftSided;

	public FootLocator(Transform transform, bool leftSided)
	{
		Transform = transform;
		LeftSided = leftSided;
	}
}
