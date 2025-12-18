using System;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.Components.Layout;

[ExecuteAlways]
public class CurvedVerticalLayoutGroup : LayoutGroup
{
	[Header("Arc")]
	[SerializeField]
	private float m_Radius = 200f;

	[Tooltip("if true, angle between elements calculates as (endAngle - startAngle) / (childCount - 1)")]
	[SerializeField]
	private bool m_UseDynamicAngle = true;

	[ShowIf("m_UseDynamicAngle")]
	[SerializeField]
	private float m_StartAngle = -90f;

	[ShowIf("m_UseDynamicAngle")]
	[SerializeField]
	private float m_EndAngle = 90f;

	[HideIf("m_UseDynamicAngle")]
	[SerializeField]
	private float m_AngleStep = 20f;

	[Space]
	[SerializeField]
	private float m_HorizontalOffset;

	[SerializeField]
	private float m_VerticalOffset;

	[Space]
	[SerializeField]
	private bool m_RotateChildren;

	protected override void OnEnable()
	{
		base.OnEnable();
		SetDirty();
	}

	protected override void OnTransformChildrenChanged()
	{
		base.OnTransformChildrenChanged();
		SetDirty();
	}

	public override void CalculateLayoutInputVertical()
	{
	}

	public override void SetLayoutHorizontal()
	{
	}

	public override void SetLayoutVertical()
	{
		int count = base.rectChildren.Count;
		if (count == 0)
		{
			return;
		}
		float num;
		float num2;
		if (m_UseDynamicAngle && count > 1)
		{
			num = m_EndAngle - m_StartAngle;
			num2 = num / (float)(count - 1);
		}
		else
		{
			num = m_AngleStep * (float)(count - 1);
			num2 = m_AngleStep;
		}
		for (int i = 0; i < count; i++)
		{
			float f = (m_StartAngle + (float)i * num2) * (MathF.PI / 180f);
			float num3 = Mathf.Sin(f) * m_Radius + m_HorizontalOffset;
			float num4 = Mathf.Cos(f) * m_Radius + m_VerticalOffset;
			RectTransform rectTransform = base.rectChildren[i];
			float num5 = LayoutUtility.GetPreferredWidth(rectTransform);
			float num6 = LayoutUtility.GetPreferredHeight(rectTransform);
			SetChildAlongAxis(rectTransform, 0, num3 - num5 * 0.5f, num5);
			SetChildAlongAxis(rectTransform, 1, num4 - num6 * 0.5f, num6);
			if (m_RotateChildren)
			{
				float num7 = num / 2f - num2 * (float)i;
				rectTransform.localEulerAngles = new Vector3(0f, 0f, 0f - num7);
			}
			else
			{
				rectTransform.localEulerAngles = Vector3.zero;
			}
		}
	}

	protected new void SetDirty()
	{
		if (IsActive())
		{
			LayoutRebuilder.MarkLayoutForRebuild(base.rectTransform);
		}
	}
}
