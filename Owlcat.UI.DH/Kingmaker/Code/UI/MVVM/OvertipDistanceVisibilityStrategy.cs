using System;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipDistanceVisibilityStrategy
{
	private readonly Func<Vector2> m_GetFocusPosition;

	public OvertipDistanceVisibilityStrategy(Func<Vector2> getFocusPosition)
	{
		m_GetFocusPosition = getFocusPosition;
	}

	public float GetVisibilityRate(Vector3 viewportPos)
	{
		Vector3 vector = m_GetFocusPosition();
		float num = (float)Mathf.Min(Screen.width, Screen.height) * 0.5f;
		float num2 = num * num;
		return (CameraRig.ViewportToScreenMainCamera(viewportPos) - vector).sqrMagnitude / num2;
	}
}
