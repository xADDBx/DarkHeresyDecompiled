using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

public class CameraStackTargetHandles : IDisposable
{
	public static readonly string _CameraColorUnscaledName = "_CameraColorUnscaled";

	public static readonly string _CameraDepthUnscaledName = "_CameraDepthUnscaled";

	public static readonly string _CameraColorScaledName = "_CameraColorScaled";

	public static readonly string _CameraDepthScaledName = "_CameraDepthScaled";

	public RTHandle UnscaledColor;

	public RTHandle UnscaledDepth;

	public RTHandle ScaledColor;

	public RTHandle ScaledDepth;

	public void Dispose()
	{
		UnscaledColor?.Release();
		UnscaledDepth?.Release();
		ScaledColor?.Release();
		ScaledDepth?.Release();
	}
}
