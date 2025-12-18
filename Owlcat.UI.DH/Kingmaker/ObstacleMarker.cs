using Kingmaker.Controllers.Clicks;
using Kingmaker.Framework.Pathfinding;
using Kingmaker.View.Covers;
using UnityEngine;

namespace Kingmaker;

public class ObstacleMarker : MonoBehaviour, IDetectClicks
{
	[HideInInspector]
	public LosCalculations.CoverType Type;

	[HideInInspector]
	public GridObstacle OwnerObstacle;

	[HideInInspector]
	public Renderer ObstacleRenderer;

	[HideInInspector]
	public Collider RaycastCollider;

	public GameObject Target
	{
		get
		{
			if (!((Object)(object)OwnerObstacle != null))
			{
				return base.gameObject;
			}
			return ((Component)(object)OwnerObstacle).gameObject;
		}
	}

	public void EnableMarker(bool enable)
	{
		if ((bool)ObstacleRenderer)
		{
			ObstacleRenderer.enabled = enable;
		}
		if ((bool)RaycastCollider)
		{
			RaycastCollider.enabled = enable;
		}
	}

	void IDetectClicks.HandleClick()
	{
	}
}
