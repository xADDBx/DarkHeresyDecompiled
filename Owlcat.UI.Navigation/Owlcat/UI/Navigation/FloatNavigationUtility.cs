using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Owlcat.UI.Navigation;

public static class FloatNavigationUtility
{
	public static GameObject FindSelectable([MaybeNull] RectTransform selected, [MaybeNull] Vector2? selectedPos, Vector3 dir, List<RectTransform> selectables)
	{
		dir = dir.normalized;
		Vector3 vector2;
		Vector3 vector;
		if (selectedPos.HasValue)
		{
			vector2 = (vector = selectedPos.Value);
		}
		else
		{
			Vector3 vector3 = Quaternion.Inverse(selected.rotation) * dir;
			selected.TransformPoint(GetPointOnRectEdge(selected, vector3));
			GetSegmentPointsInRect(selected.rect, out vector2, out vector);
			vector2 = selected.TransformPoint(vector2);
			vector = selected.TransformPoint(vector);
		}
		float num = float.NegativeInfinity;
		float num2 = float.NegativeInfinity;
		float num3 = 0f;
		bool flag = false;
		RectTransform rectTransform = null;
		RectTransform rectTransform2 = null;
		foreach (RectTransform selectable in selectables)
		{
			if (selectable == selected)
			{
				continue;
			}
			RectTransform rectTransform3 = selectable.transform as RectTransform;
			Vector3 a = default(Vector3);
			Vector3 b = default(Vector3);
			if (rectTransform3 != null)
			{
				GetSegmentPointsInRect(rectTransform3.rect, out a, out b);
			}
			a = selectable.transform.TransformPoint(a);
			b = selectable.transform.TransformPoint(b);
			Vector3 vector4 = ClosestPointOnSegment(vector2, vector, a);
			Vector3 vector5 = ClosestPointOnSegment(vector2, vector, b);
			Vector3 vector6 = ClosestPointOnSegment(a, b, vector2);
			Vector3 vector7 = ClosestPointOnSegment(a, b, vector);
			Vector3 vector8 = vector6 - vector4;
			Vector3 v2 = vector7 - vector4;
			Vector3 v3 = vector6 - vector5;
			Vector3 v4 = vector7 - vector5;
			Vector3 myVector = vector8;
			float dot = Vector3.Dot(dir, myVector);
			GetShortestVector(v2);
			GetShortestVector(v3);
			GetShortestVector(v4);
			if (flag && dot < 0f)
			{
				num3 = (0f - dot) * myVector.sqrMagnitude;
				if (num3 > num2)
				{
					num2 = num3;
					rectTransform2 = selectable;
				}
			}
			else if (!(dot <= 0f))
			{
				num3 = dot / myVector.sqrMagnitude;
				if (num3 > num)
				{
					num = num3;
					rectTransform = selectable;
				}
			}
			void GetShortestVector(Vector3 v)
			{
				if (!(v.sqrMagnitude >= myVector.sqrMagnitude))
				{
					dot = Vector3.Dot(dir, v);
					myVector = v;
				}
			}
		}
		if (flag && null == rectTransform)
		{
			return rectTransform2?.gameObject;
		}
		return rectTransform?.gameObject;
	}

	private static void GetSegmentPointsInRect(Rect r, out Vector3 a, out Vector3 b)
	{
		Vector2 min = r.min;
		Vector2 max = r.max;
		Vector2 vector = (max - min) * 0.5f;
		if (vector.x > vector.y)
		{
			a = new Vector2(min.x + vector.y, (max.y + min.y) * 0.5f);
			b = new Vector2(max.x - vector.y, (max.y + min.y) * 0.5f);
		}
		else
		{
			a = new Vector2((max.x + min.x) * 0.5f, min.y + vector.x);
			b = new Vector2((max.x + min.x) * 0.5f, max.y - vector.x);
		}
	}

	private static Vector3 ClosestPointOnSegment(Vector3 s0, Vector3 s1, Vector3 pt)
	{
		Vector3 vector = s1 - s0;
		float num = Vector3.Dot(vector, vector);
		float t = ((num < 1E-05f) ? 0f : Mathf.Clamp01(((pt.x - s0.x) * vector.x + (pt.y - s0.y) * vector.y + (pt.z - s0.z) * vector.z) / num));
		return Vector3.Lerp(s0, s1, t);
	}

	private static Vector3 GetPointOnRectEdge(RectTransform rect, Vector2 dir)
	{
		if (rect == null)
		{
			return Vector3.zero;
		}
		if (dir != Vector2.zero)
		{
			dir /= Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
		}
		dir = rect.rect.center + Vector2.Scale(rect.rect.size, dir * 0.5f);
		return dir;
	}
}
