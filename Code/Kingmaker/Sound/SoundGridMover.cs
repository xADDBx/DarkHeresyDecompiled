using Kingmaker.Controllers;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Sound;

public class SoundGridMover : MonoBehaviour, IUpdatable
{
	public SoundGridDrawer gridDrawer;

	private float maxDistance = 25f;

	private Bounds m_Bounds;

	private readonly Vector3 m_Ray = new Vector3(0.5f, 0.5f, 0f);

	public void OnEnable()
	{
		if (gridDrawer == null && !TryGetComponent<SoundGridDrawer>(out gridDrawer))
		{
			PFLog.Audio.Error("Grid Drawer not set for " + base.gameObject.name);
			return;
		}
		m_Bounds = default(Bounds);
		for (int i = 0; i < gridDrawer.gridPoints.Count; i++)
		{
			m_Bounds.Encapsulate(gridDrawer.gridPoints[i]);
		}
		Game.Instance.Controllers.CustomUpdateController.Add(this);
	}

	public void OnDisable()
	{
		Game.Instance.Controllers.CustomUpdateController.Remove(this);
	}

	public void Tick(float delta)
	{
		using (ProfileScope.New("SoundGridMover"))
		{
			if (CameraRig.Instance.Camera == null)
			{
				return;
			}
			Ray ray = CameraRig.Instance.Camera.ViewportPointToRay(m_Ray);
			if (m_Bounds.IntersectRay(ray, out float distance))
			{
				Vector3 point = ray.GetPoint(distance);
				if (gridDrawer.IsPointInsidePolygon(point))
				{
					int num = Mathf.RoundToInt((point.x - m_Bounds.min.x) / gridDrawer.gridCellSize);
					int num2 = Mathf.RoundToInt((point.z - m_Bounds.min.z) / gridDrawer.gridCellSize);
					Vector3 position = new Vector3(m_Bounds.min.x + (float)num * gridDrawer.gridCellSize, gridDrawer.gridYPosition, m_Bounds.min.z + (float)num2 * gridDrawer.gridCellSize);
					base.transform.position = position;
					return;
				}
			}
			Vector3 point2 = ray.GetPoint(maxDistance);
			Vector3 position2 = Vector3.zero;
			float num3 = float.MaxValue;
			for (int i = 0; i < gridDrawer.gridPoints.Count; i++)
			{
				Vector3 vector = new Vector3(gridDrawer.gridPoints[i].x, gridDrawer.gridYPosition, gridDrawer.gridPoints[i].z);
				Vector3 vector2 = new Vector3(gridDrawer.gridPoints[(i + 1) % gridDrawer.gridPoints.Count].x, gridDrawer.gridYPosition, gridDrawer.gridPoints[(i + 1) % gridDrawer.gridPoints.Count].z) - vector;
				float num4 = Mathf.Clamp01(Vector2.Dot((Vector2)(point2 - vector), (Vector2)vector2) / vector2.sqrMagnitude);
				Vector3 vector3 = vector + num4 * vector2;
				float sqrMagnitude = (vector3 - point2).sqrMagnitude;
				if (sqrMagnitude < num3)
				{
					num3 = sqrMagnitude;
					position2 = vector3;
				}
			}
			base.transform.position = position2;
		}
	}
}
