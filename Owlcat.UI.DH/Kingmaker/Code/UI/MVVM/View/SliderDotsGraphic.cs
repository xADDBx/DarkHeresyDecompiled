using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class SliderDotsGraphic : MaskableGraphic
{
	[SerializeField]
	private float m_DotSize = 10f;

	[SerializeField]
	private RectOffset m_Padding = new RectOffset();

	[SerializeField]
	private int m_Count;

	[SerializeField]
	private float m_TrackWidth;

	public void SetLayout(int count, float trackWidth)
	{
		if (m_Count != count || !Mathf.Approximately(m_TrackWidth, trackWidth))
		{
			m_Count = count;
			m_TrackWidth = trackWidth;
			SetVerticesDirty();
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		if (m_Count > 0)
		{
			float num = m_DotSize / 2f;
			float num2 = m_DotSize / 2f;
			float num3 = m_Padding?.left ?? 0;
			float num4 = m_Padding?.right ?? 0;
			float num5 = m_Padding?.top ?? 0;
			float num6 = m_Padding?.bottom ?? 0;
			float num7 = (0f - m_TrackWidth) / 2f + num + num3;
			float num8 = m_TrackWidth / 2f - num - num4;
			float num9 = ((m_Count == 1) ? 0f : ((num8 - num7) / (float)(m_Count - 1)));
			float num10 = (num6 - num5) / 2f;
			if (m_Count == 1)
			{
				num7 = (num7 + num8) / 2f;
			}
			for (int i = 0; i < m_Count; i++)
			{
				float num11 = num7 + num9 * (float)i;
				int currentVertCount = vh.currentVertCount;
				vh.AddVert(new Vector3(num11 - num, num10 - num2), color, new Vector2(0f, 0f));
				vh.AddVert(new Vector3(num11 - num, num10 + num2), color, new Vector2(0f, 1f));
				vh.AddVert(new Vector3(num11 + num, num10 + num2), color, new Vector2(1f, 1f));
				vh.AddVert(new Vector3(num11 + num, num10 - num2), color, new Vector2(1f, 0f));
				vh.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
				vh.AddTriangle(currentVertCount, currentVertCount + 2, currentVertCount + 3);
			}
		}
	}
}
