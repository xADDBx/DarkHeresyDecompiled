using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.PostProcess;

public static class AccessibilityPostProcessing
{
	public struct Parameters
	{
		public float ColorBlindProtanopiaFactor;

		public float ColorBlindDeuteranopiaFactor;

		public float ColorBlindTritanopiaFactor;

		public float ColorBlindDaltonizeFactor;

		public float BrightnessFactor;

		public float ContrastFactor;

		public static readonly Parameters Default = new Parameters
		{
			BrightnessFactor = 0.5f,
			ContrastFactor = 0.5f
		};
	}

	private static readonly int ShaderPropertyAccessibilityParams = Shader.PropertyToID("_AccessibilityParams");

	private static readonly int ShaderPropertyAccessibilityParams1 = Shader.PropertyToID("_AccessibilityParams1");

	public static Parameters GetParameters(in WaaaghCameraData cameraData)
	{
		if (cameraData.isSceneViewCamera)
		{
			return Parameters.Default;
		}
		if (!cameraData.postProcessEnabled)
		{
			return Parameters.Default;
		}
		Daltonization component = VolumeManager.instance.stack.GetComponent<Daltonization>();
		if (!component.IsActive())
		{
			return Parameters.Default;
		}
		Parameters result = default(Parameters);
		result.ColorBlindProtanopiaFactor = component.ProtanopiaFactor.value;
		result.ColorBlindDeuteranopiaFactor = component.DeuteranopiaFactor.value;
		result.ColorBlindTritanopiaFactor = component.TritanopiaFactor.value;
		result.ColorBlindDaltonizeFactor = component.Intensity.value;
		result.BrightnessFactor = component.BrightnessFactor.value;
		result.ContrastFactor = component.ContrastFactor.value;
		return result;
	}

	public static void SetupGlobalShaderParameters(CommandBuffer cmd, in Parameters parameters)
	{
		cmd.SetGlobalVector(ShaderPropertyAccessibilityParams, new Vector4(Mathf.Clamp01(parameters.ColorBlindProtanopiaFactor), Mathf.Clamp01(parameters.ColorBlindDeuteranopiaFactor), Mathf.Clamp01(parameters.ColorBlindTritanopiaFactor), Mathf.Clamp01(parameters.ColorBlindDaltonizeFactor)));
		float num = Mathf.Clamp01(parameters.BrightnessFactor);
		float num2 = Mathf.Clamp01(parameters.ContrastFactor);
		float num3 = 1f - Mathf.Abs(num - 0.5f);
		bool num4 = (double)num >= 0.5;
		float num5 = num2 + 0.5f;
		float num6 = num3;
		float num7 = (float)(num4 ? 1 : 0) * (1f - num3);
		float num8 = num5;
		float num9 = 0.5f * (1f - num5);
		float x = num6 * num8;
		float y = num7 * num8 + num9;
		cmd.SetGlobalVector(ShaderPropertyAccessibilityParams1, new Vector4(x, y, 0f, 0f));
	}
}
