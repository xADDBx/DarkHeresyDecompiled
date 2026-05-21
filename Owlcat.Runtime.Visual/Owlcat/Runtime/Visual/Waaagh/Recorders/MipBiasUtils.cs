using System;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

internal static class MipBiasUtils
{
	public static Vector2 CalculateGlobalMipBias(WaaaghCameraData cameraData, float extraBias = 0f)
	{
		float num = ((cameraData.StackInfo.RequiredTargets == CameraRequiredTargets.Unscaled) ? 0f : Math.Min((float)(0.0 - Math.Log(1f / cameraData.renderScale, 2.0)), 0f)) + extraBias;
		return new Vector2(num, Mathf.Pow(2f, num));
	}
}
