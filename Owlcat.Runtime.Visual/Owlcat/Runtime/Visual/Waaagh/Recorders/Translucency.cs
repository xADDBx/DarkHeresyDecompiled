using Owlcat.Runtime.Visual.Lighting;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class Translucency
{
	public unsafe static void FillGlobalShaderVariables(ref WaaaghShaderVariablesGlobal g)
	{
		WaaaghTranslucencyProfileSet instance = WaaaghTranslucencyProfileSet.Instance;
		if (instance == null)
		{
			g._TranslucencyProfileCount.x = 0f;
			return;
		}
		int num = math.min(instance.Profiles.Count, 16);
		g._TranslucencyProfileCount.x = num;
		fixed (WaaaghShaderVariablesGlobal* ptr = &g)
		{
			Vector4* ptr2 = (Vector4*)ptr->_TranslucencyProfileColors;
			Vector4* ptr3 = (Vector4*)ptr->_TranslucencyProfileSelfShadowingWorkaroundData;
			for (int i = 0; i < num; i++)
			{
				WaaaghTranslucencyProfileSet.TranslucencyProfile translucencyProfile = instance.Profiles[i];
				ptr2[i] = translucencyProfile.Color;
				ref WaaaghTranslucencyProfileSet.SelfShadowingWorkaroundData selfShadowingWorkaround = ref translucencyProfile.SelfShadowingWorkaround;
				ptr3[i] = new Vector4(selfShadowingWorkaround.DepthBias, selfShadowingWorkaround.DepthBiasPointSpot, selfShadowingWorkaround.NormalBias, selfShadowingWorkaround.NormalBiasPointSpot);
			}
		}
	}
}
