using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

public static class ShaderGlobalKeywords
{
	public static GlobalKeyword PROBE_VOLUMES_L1;

	public static GlobalKeyword PROBE_VOLUMES_L2;

	public static GlobalKeyword EVALUATE_SH_VERTEX;

	public static GlobalKeyword _LINEAR_TO_SRGB_CONVERSION;

	public static void Initialize()
	{
		PROBE_VOLUMES_L1 = GlobalKeyword.Create("PROBE_VOLUMES_L1");
		PROBE_VOLUMES_L2 = GlobalKeyword.Create("PROBE_VOLUMES_L2");
		EVALUATE_SH_VERTEX = GlobalKeyword.Create("EVALUATE_SH_VERTEX");
		_LINEAR_TO_SRGB_CONVERSION = GlobalKeyword.Create("_LINEAR_TO_SRGB_CONVERSION");
	}
}
