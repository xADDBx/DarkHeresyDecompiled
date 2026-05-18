using Dreamteck.Splines;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.Components;

[RequireComponent(typeof(CanvasRenderer))]
[RequireComponent(typeof(SplineComputer))]
public class SplineGraphic : MaskableGraphic
{
	[SerializeField]
	[Range(4f, 128f)]
	private int m_SampleCount = 32;

	[SerializeField]
	[Min(0.1f)]
	private float m_LineWidth = 2f;

	[Header("Dash")]
	[SerializeField]
	[Min(0f)]
	private float m_DashLength = 10f;

	[SerializeField]
	[Min(0f)]
	private float m_GapLength = 5f;

	private SplineComputer m_Spline;

	private float m_ClipFrom;

	private float m_ClipTo = 1f;

	public float ClipFrom
	{
		get
		{
			return m_ClipFrom;
		}
		set
		{
			m_ClipFrom = Mathf.Clamp01(value);
			SetVerticesDirty();
		}
	}

	public float ClipTo
	{
		get
		{
			return m_ClipTo;
		}
		set
		{
			m_ClipTo = Mathf.Clamp01(value);
			SetVerticesDirty();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_Spline = GetComponent<SplineComputer>();
	}

	public void ResetClip()
	{
		m_ClipFrom = 0f;
		m_ClipTo = 1f;
		SetVerticesDirty();
	}

	public void SetDirty()
	{
		SetVerticesDirty();
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		if (m_Spline == null || m_Spline.pointCount < 2)
		{
			return;
		}
		int num = m_SampleCount - 1;
		Vector2[] array = new Vector2[m_SampleCount];
		float[] array2 = new float[m_SampleCount];
		array2[0] = 0f;
		for (int i = 0; i < m_SampleCount; i++)
		{
			double percent = (double)i / (double)num;
			Vector3 position = m_Spline.EvaluatePosition(percent);
			Vector3 vector = base.transform.InverseTransformPoint(position);
			array[i] = new Vector2(vector.x, vector.y);
			if (i > 0)
			{
				array2[i] = array2[i - 1] + Vector2.Distance(array[i - 1], array[i]);
			}
		}
		float num2 = array2[num];
		if (num2 <= 0f)
		{
			return;
		}
		float num3 = m_ClipFrom * num2;
		float num4 = m_ClipTo * num2;
		if (num3 >= num4)
		{
			return;
		}
		if (!(m_DashLength > 0f) || !(m_GapLength > 0f))
		{
			BuildDashSegment(vh, array, array2, num, num3, num4, num2);
			return;
		}
		float num5 = m_DashLength + m_GapLength;
		for (float num6 = 0f; num6 < num4; num6 += num5)
		{
			float num7 = Mathf.Max(num6, num3);
			float num8 = Mathf.Min(num6 + m_DashLength, num4);
			if (num7 < num8)
			{
				BuildDashSegment(vh, array, array2, num, num7, num8, num2);
			}
		}
	}

	private void BuildLineStrip(VertexHelper vh, Vector2[] positions, int segmentCount)
	{
		float halfWidth = m_LineWidth * 0.5f;
		UIVertex vert = UIVertex.simpleVert;
		vert.color = color;
		for (int i = 0; i < segmentCount; i++)
		{
			AddQuad(vh, ref vert, positions[i], positions[i + 1], halfWidth, (float)i / (float)segmentCount, (float)(i + 1) / (float)segmentCount);
		}
	}

	private void BuildDashSegment(VertexHelper vh, Vector2[] positions, float[] distances, int segmentCount, float dashStart, float dashEnd, float totalLength)
	{
		float halfWidth = m_LineWidth * 0.5f;
		UIVertex vert = UIVertex.simpleVert;
		vert.color = color;
		Vector2 p = LerpAlongPolyline(positions, distances, segmentCount, dashStart);
		float u = dashStart / totalLength;
		for (int i = 0; i < segmentCount; i++)
		{
			if (!(distances[i + 1] <= dashStart))
			{
				if (distances[i] >= dashEnd)
				{
					break;
				}
				float num = Mathf.Min(distances[i + 1], dashEnd);
				Vector2 vector = ((distances[i + 1] <= dashEnd) ? positions[i + 1] : LerpAlongPolyline(positions, distances, segmentCount, dashEnd));
				float num2 = num / totalLength;
				AddQuad(vh, ref vert, p, vector, halfWidth, u, num2);
				p = vector;
				u = num2;
			}
		}
	}

	private static Vector2 LerpAlongPolyline(Vector2[] positions, float[] distances, int segmentCount, float distance)
	{
		for (int i = 0; i < segmentCount; i++)
		{
			if (!(distances[i + 1] < distance))
			{
				float num = distances[i + 1] - distances[i];
				float t = ((num > 0f) ? ((distance - distances[i]) / num) : 0f);
				return Vector2.Lerp(positions[i], positions[i + 1], t);
			}
		}
		return positions[segmentCount];
	}

	private static void AddQuad(VertexHelper vh, ref UIVertex vert, Vector2 p0, Vector2 p1, float halfWidth, float u0, float u1)
	{
		Vector2 normalized = (p1 - p0).normalized;
		Vector2 vector = new Vector2(0f - normalized.y, normalized.x);
		int currentVertCount = vh.currentVertCount;
		vert.position = p0 + vector * halfWidth;
		vert.uv0 = new Vector2(u0, 1f);
		vh.AddVert(vert);
		vert.position = p0 - vector * halfWidth;
		vert.uv0 = new Vector2(u0, 0f);
		vh.AddVert(vert);
		vert.position = p1 - vector * halfWidth;
		vert.uv0 = new Vector2(u1, 0f);
		vh.AddVert(vert);
		vert.position = p1 + vector * halfWidth;
		vert.uv0 = new Vector2(u1, 1f);
		vh.AddVert(vert);
		vh.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
		vh.AddTriangle(currentVertCount, currentVertCount + 2, currentVertCount + 3);
	}
}
