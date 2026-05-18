using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class TooltipElementStatPieWidgetView : MaskableGraphic
{
	[Range(3f, 12f)]
	public int SegmentCount = 6;

	[Range(0f, 1f)]
	public float[] Values = new float[6];

	public Color Color = Color.white;

	public float Radius = 120f;

	public float InnerRadius = 25f;

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		if (Values == null || Values.Length != SegmentCount)
		{
			Values = new float[SegmentCount];
		}
		Vector2 center = base.rectTransform.rect.center;
		float num = 360f / (float)SegmentCount;
		for (int i = 0; i < SegmentCount; i++)
		{
			float num2 = Mathf.Lerp(InnerRadius, Radius, Mathf.Clamp01(Values[i]));
			float f = MathF.PI / 180f * (num * (float)i - 90f);
			float f2 = MathF.PI / 180f * (num * (float)(i + 1) - 90f);
			Vector2 vector = center + new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * InnerRadius;
			Vector2 vector2 = center + new Vector2(Mathf.Cos(f2), Mathf.Sin(f2)) * InnerRadius;
			Vector2 vector3 = center + new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * num2;
			Vector2 vector4 = center + new Vector2(Mathf.Cos(f2), Mathf.Sin(f2)) * num2;
			int currentVertCount = vh.currentVertCount;
			UIVertex simpleVert = UIVertex.simpleVert;
			simpleVert.color = Color;
			simpleVert.position = vector;
			vh.AddVert(simpleVert);
			simpleVert.position = vector2;
			vh.AddVert(simpleVert);
			simpleVert.position = vector4;
			vh.AddVert(simpleVert);
			simpleVert.position = vector3;
			vh.AddVert(simpleVert);
			vh.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
			vh.AddTriangle(currentVertCount, currentVertCount + 2, currentVertCount + 3);
		}
	}
}
