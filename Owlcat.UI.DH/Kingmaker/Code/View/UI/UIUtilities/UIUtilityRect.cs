using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.GameModes;
using Kingmaker.UI;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UIUtilityRect
{
	public static Vector3 GetNormalizedPositionInCamera(Vector3 objectPosition)
	{
		if (Game.Instance.CurrentModeType == GameModeType.Cutscene)
		{
			return CameraRig.WorldToViewportMainCamera(objectPosition);
		}
		return CameraRig.Instance.WorldToViewport(objectPosition);
	}

	public static Vector2 NormalizedToPixelPosition(Rect parentRect, Vector3 position)
	{
		return position * parentRect.size;
	}

	public static Vector2 LimitPositionRectInRect(Vector2 nPos, RectTransform parent, RectTransform child)
	{
		return LimitPositionRectInRect(nPos, parent.rect, child.rect.size, child.pivot);
	}

	public static void SetPopupWindowPosition(RectTransform windowTransform, RectTransform sourceTransform, Vector2 shiftPosition, List<Vector2> priorityPivots = null)
	{
		if (sourceTransform == null)
		{
			PFLog.UI.Error("Tooltip doesn't have sourceTransform");
			return;
		}
		List<Vector2> list = new List<Vector2>();
		TooltipEngine.SetPivots(list, priorityPivots);
		Vector3 vector = UICamera.Instance.WorldToViewportPoint(sourceTransform.position);
		RectTransform component = windowTransform.parent.gameObject.GetComponent<RectTransform>();
		vector = new Vector2(vector.x - sourceTransform.rect.width * (sourceTransform.pivot.x - 0.5f) / component.rect.width, vector.y - sourceTransform.rect.height * (sourceTransform.pivot.y - 0.5f) / component.rect.height);
		Vector2 anchorMax = (windowTransform.anchorMin = vector);
		windowTransform.anchorMax = anchorMax;
		Vector2 vector3 = new Vector2(sourceTransform.rect.width / 2f + shiftPosition.x, sourceTransform.rect.height / 2f + shiftPosition.y);
		Vector2 vector4 = new Vector2(vector3.x / component.rect.width, vector3.y / component.rect.height);
		foreach (Vector2 item in list)
		{
			Vector2 vector5 = new Vector2(1f - item.x * 2f, 1f - item.y * 2f);
			Vector2 vector6 = new Vector2(vector.x + vector4.x * vector5.x, vector.y + vector4.y * vector5.y);
			anchorMax = (windowTransform.anchorMin = vector6);
			windowTransform.anchorMax = anchorMax;
			windowTransform.pivot = item;
			windowTransform.anchoredPosition = new Vector2(0f, 0f);
			if (IsTransformInScreen(windowTransform))
			{
				return;
			}
		}
		SetInTheMiddle(windowTransform);
	}

	public static Vector3 GetWorldCenter(RectTransform rectTransform)
	{
		Vector3 position = rectTransform.position;
		Vector2 size = rectTransform.rect.size;
		Vector2 pivot = rectTransform.pivot;
		Vector2 b = new Vector2(0.5f - pivot.x, 0.5f - pivot.y);
		Vector2 vector = Vector2.Scale(size, b);
		Vector3 vector2 = rectTransform.TransformVector(vector);
		return position + vector2;
	}

	public static bool Intersects(Rect r1, Rect r2, out Rect area)
	{
		area = default(Rect);
		if (r2.Overlaps(r1))
		{
			float num = Mathf.Min(r1.xMax, r2.xMax);
			float num2 = Mathf.Max(r1.xMin, r2.xMin);
			float num3 = Mathf.Min(r1.yMax, r2.yMax);
			float num4 = Mathf.Max(r1.yMin, r2.yMin);
			area.x = Mathf.Min(num, num2);
			area.y = Mathf.Min(num3, num4);
			area.width = Mathf.Max(0f, num - num2);
			area.height = Mathf.Max(0f, num3 - num4);
			return true;
		}
		return false;
	}

	public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max)
	{
		return Vector2.Min(Vector2.Max(value, min), max);
	}

	public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
	{
		return Vector3.Min(Vector3.Max(value, min), max);
	}

	private static Vector2 LimitPositionRectInRect(Vector2 nPos, Rect parent, Vector2 childSize, Vector2 childPivot)
	{
		float width = parent.width;
		float height = parent.height;
		float x = childSize.x;
		float y = childSize.y;
		if (nPos.x + width / 2f - childPivot.x * x <= 0f)
		{
			nPos.x = (0f - width) / 2f + childPivot.x * x;
		}
		else if (nPos.x + width / 2f + (1f - childPivot.x) * x >= width)
		{
			nPos.x = width / 2f - (1f - childPivot.x) * x;
		}
		if (nPos.y + height / 2f - childPivot.y * y <= 0f)
		{
			nPos.y = (0f - height) / 2f + childPivot.x * y;
		}
		else if (nPos.y + height / 2f + (1f - childPivot.y) * y >= height)
		{
			nPos.y = height / 2f - (1f - childPivot.y) * y;
		}
		return nPos;
	}

	private static bool CheckIfFitsToScreen(Vector2 screenPoint)
	{
		if (screenPoint.x < 0f || screenPoint.y < 0f)
		{
			return false;
		}
		if (screenPoint.x > (float)UICamera.Instance.pixelWidth)
		{
			return false;
		}
		if (screenPoint.y > (float)UICamera.Instance.pixelHeight)
		{
			return false;
		}
		return true;
	}

	private static bool IsTransformInScreen(Transform transform)
	{
		RectTransform component = transform.GetComponent<RectTransform>();
		Vector3[] array = new Vector3[4];
		Canvas.ForceUpdateCanvases();
		component.GetWorldCorners(array);
		return array.Select((Vector3 v) => RectTransformUtility.WorldToScreenPoint(UICamera.Instance, v)).All(CheckIfFitsToScreen);
	}

	private static void SetInTheMiddle(RectTransform windowTransform)
	{
		Vector2 anchorMax = (windowTransform.anchorMin = new Vector2(0.5f, 0.5f));
		windowTransform.anchorMax = anchorMax;
		windowTransform.pivot = new Vector2(0.5f, 0.5f);
		windowTransform.anchoredPosition = new Vector2(0f, 0f);
	}
}
