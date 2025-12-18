using UnityEngine;

namespace Kingmaker.Framework.Pathfinding;

public class GridObstacleGeneratedContainer : MonoBehaviour
{
	private void OnValidate()
	{
		if (!base.name.Contains("GENERATED_OBSTACLES"))
		{
			base.name = "GENERATED_OBSTACLES";
		}
		base.transform.localPosition = Vector3.zero;
		base.transform.localScale = Vector3.one;
	}
}
