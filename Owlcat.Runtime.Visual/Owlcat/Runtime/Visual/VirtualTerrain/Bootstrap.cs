using Owlcat.Runtime.Visual.VirtualTerrain.Streaming;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.VirtualTerrain;

internal static class Bootstrap
{
	[RuntimeInitializeOnLoadMethod]
	public static void Initialize()
	{
		if (GraphicsSettings.TryGetRenderPipelineSettings<VirtualTerrainGlobalSettings>(out var settings) && settings.Enabled)
		{
			TerrainStreamingSystem.Initialize();
		}
	}
}
