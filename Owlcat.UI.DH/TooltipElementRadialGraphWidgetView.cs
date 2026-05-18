using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class TooltipElementRadialGraphWidgetView : Graphic
{
	[Range(3f, 12f)]
	public int axisCount = 6;

	[Range(0f, 1f)]
	public float[] values = new float[6];

	public float radius = 100f;

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		if (values == null || values.Length != axisCount)
		{
			values = new float[axisCount];
		}
		Vector2 center = base.rectTransform.rect.center;
		float num = 360f / (float)axisCount;
		List<Vector2> list = new List<Vector2>();
		for (int i = 0; i < axisCount; i++)
		{
			float f = MathF.PI / 180f * (num * (float)i - 90f);
			float num2 = radius * Mathf.Clamp01(values[i]);
			Vector2 item = center + new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * num2;
			list.Add(item);
		}
		UIVertex simpleVert = UIVertex.simpleVert;
		simpleVert.color = color;
		simpleVert.position = center;
		vh.AddVert(simpleVert);
		int idx = 0;
		for (int j = 0; j < list.Count; j++)
		{
			simpleVert.position = list[j];
			vh.AddVert(simpleVert);
		}
		for (int k = 0; k < axisCount; k++)
		{
			int num3 = k + 1;
			if (num3 >= axisCount)
			{
				num3 = 0;
			}
			vh.AddTriangle(idx, k + 1, num3 + 1);
		}
	}
}
