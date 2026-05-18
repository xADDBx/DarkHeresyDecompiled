using CatmullRomSplines;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[ExecuteAlways]
[RequireComponent(typeof(VectorSpline))]
[RequireComponent(typeof(LineRenderer))]
[AddComponentMenu("Cutscene Markers/Spline Line Renderer")]
public class VectorSplineLineRenderer : MonoBehaviour
{
	[Min(4f)]
	public int Samples = 48;

	public bool ProjectOnGround = true;

	public LayerMask GroundMask = 256;

	public float RaycastOriginOffset = 20f;

	private VectorSpline _spline;

	private LineRenderer _lineRenderer;

	private void OnEnable()
	{
		_spline = GetComponent<VectorSpline>();
		_lineRenderer = GetComponent<LineRenderer>();
		Refresh();
	}

	private Vector3 ProjectPoint(Vector3 point)
	{
		if (Physics.Raycast(point + Vector3.up * RaycastOriginOffset, Vector3.down, out var hitInfo, RaycastOriginOffset * 2f, GroundMask, QueryTriggerInteraction.Ignore))
		{
			return hitInfo.point;
		}
		return point;
	}

	private void Refresh()
	{
		if (!(_spline == null) && !(_lineRenderer == null) && _spline.Count >= 2)
		{
			_lineRenderer.positionCount = Samples + 1;
			for (int i = 0; i <= Samples; i++)
			{
				float t = (float)i / (float)Samples;
				Vector3 vector = _spline.EvaluatePosition(t);
				_lineRenderer.SetPosition(i, ProjectOnGround ? ProjectPoint(vector) : vector);
			}
		}
	}
}
