using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.PostProcess;

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

	public static void SetupGlobalShaderParameters(UnsafeCommandBuffer cmd, in Parameters parameters)
	{
		cmd.SetGlobalVector(ShaderPropertyAccessibilityParams, new Vector4(Mathf.Clamp01(parameters.ColorBlindProtanopiaFactor), Mathf.Clamp01(parameters.ColorBlindDeuteranopiaFactor), Mathf.Clamp01(parameters.ColorBlindTritanopiaFactor), Mathf.Clamp01(parameters.ColorBlindDaltonizeFactor)));
		var (x, y) = BuildBrightnessContrastMad(parameters.BrightnessFactor, parameters.ContrastFactor);
		cmd.SetGlobalVector(ShaderPropertyAccessibilityParams1, new Vector4(x, y, 0f, 0f));
	}

	public static (float mul, float add) BuildBrightnessContrastMad(float brightnessFactor, float contrastFactor)
	{
		brightnessFactor = Mathf.Clamp01(brightnessFactor);
		contrastFactor = Mathf.Clamp01(contrastFactor);
		float num = 1f - Mathf.Abs(brightnessFactor - 0.5f);
		bool num2 = (double)brightnessFactor >= 0.5;
		float num3 = contrastFactor + 0.5f;
		float num4 = num;
		float num5 = (float)(num2 ? 1 : 0) * (1f - num);
		float num6 = num3;
		float num7 = 0.5f * (1f - num3);
		float item = num4 * num6;
		float item2 = num5 * num6 + num7;
		return (mul: item, add: item2);
	}
}
