using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Sound;

public class SoundGridDrawer : MonoBehaviour
{
	public List<Vector3> gridPoints = new List<Vector3>();

	[Range(0.5f, 2f)]
	public float gridCellSize = 1f;

	public float gridYPosition;

	public bool IsPointInsidePolygon(Vector3 point)
	{
		bool flag = false;
		int index = gridPoints.Count - 1;
		for (int i = 0; i < gridPoints.Count; i++)
		{
			if (gridPoints[i].z > point.z != gridPoints[index].z > point.z && point.x < (gridPoints[index].x - gridPoints[i].x) * (point.z - gridPoints[i].z) / (gridPoints[index].z - gridPoints[i].z) + gridPoints[i].x)
			{
				flag = !flag;
			}
			index = i;
		}
		return flag;
	}
}
