using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

internal static class PlatformAutoDetect
{
	internal static bool isXRMobile { get; private set; }

	internal static bool isShaderAPIMobileDefined { get; private set; }

	internal static bool isSwitch { get; private set; }

	internal static bool isShaderAPIGLESDefined { get; private set; }

	internal static bool isShaderHintNiceQualityDefined { get; private set; }

	internal static void Initialize()
	{
		isXRMobile = false;
		isShaderAPIMobileDefined = GraphicsSettings.HasShaderDefine(BuiltinShaderDefine.SHADER_API_MOBILE);
		isSwitch = Application.platform == RuntimePlatform.Switch;
		isShaderAPIGLESDefined = GraphicsSettings.HasShaderDefine(BuiltinShaderDefine.SHADER_API_GLES30);
		isShaderHintNiceQualityDefined = !isShaderAPIGLESDefined && !isSwitch;
	}
}
