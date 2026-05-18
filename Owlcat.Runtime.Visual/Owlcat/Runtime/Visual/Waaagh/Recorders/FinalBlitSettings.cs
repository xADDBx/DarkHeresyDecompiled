using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public struct FinalBlitSettings
{
	public bool isFxaaEnabled;

	public bool isFsrEnabled;

	public bool isTaaSharpeningEnabled;

	public bool requireHDROutput;

	public bool resolveToDebugScreen;

	public bool isAlphaOutputEnabled;

	public HDROutputUtils.Operation hdrOperations;

	public static FinalBlitSettings Create()
	{
		FinalBlitSettings result = default(FinalBlitSettings);
		result.isFxaaEnabled = false;
		result.isFsrEnabled = false;
		result.isTaaSharpeningEnabled = false;
		result.requireHDROutput = false;
		result.resolveToDebugScreen = false;
		result.isAlphaOutputEnabled = false;
		result.hdrOperations = HDROutputUtils.Operation.None;
		return result;
	}
}
