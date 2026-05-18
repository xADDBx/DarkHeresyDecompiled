using Owlcat.Runtime.Visual.Terrain;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

internal static class Terrain
{
	public static void ConfigureTerrainTransition(UnsafeCommandBuffer cmd)
	{
		if (OwlcatTerrainTransition.Active)
		{
			cmd.EnableShaderKeyword("_TERRAIN_TRANSITION");
			cmd.SetGlobalVector(OwlcatTerrainShader.TerrainTransitionShape, OwlcatTerrainTransition.MakeTransitionClipShape());
		}
		else
		{
			cmd.DisableShaderKeyword("_TERRAIN_TRANSITION");
		}
	}
}
