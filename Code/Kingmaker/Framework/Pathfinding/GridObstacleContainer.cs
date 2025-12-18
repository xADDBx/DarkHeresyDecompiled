using UnityEngine;

namespace Kingmaker.Framework.Pathfinding;

public class GridObstacleContainer : MonoBehaviour
{
	private void OnValidate()
	{
		if (!base.name.Contains("OBSTACLES"))
		{
			base.name = "OBSTACLES";
		}
		base.transform.localPosition = Vector3.zero;
		base.transform.localScale = Vector3.one;
	}
}
