using Owlcat.Runtime.Visual.Overrides;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.PostProcess;

internal static class MakeevTonemapping
{
	private static class ShaderIDs
	{
		public static readonly int _MakeevTonemappingLUT = Shader.PropertyToID("_MakeevTonemappingLUT");

		public static readonly int _MakeevTonemappingLUTParams = Shader.PropertyToID("_MakeevTonemappingLUTParams");
	}

	public static bool TrySetupParameters(Material material, PostProcessResources resources, Tonemapping tonemapping)
	{
		Texture3D makeevLut = resources.Textures.MakeevLut;
		if (makeevLut == null)
		{
			return false;
		}
		float num = makeevLut.width;
		Vector4 vector = default(Vector4);
		vector.x = (num - 1f) / num;
		vector.y = 0.5f / num;
		vector.z = tonemapping.makeevRemapAggressiveness.value;
		Vector4 value = vector;
		material.SetTexture(ShaderIDs._MakeevTonemappingLUT, makeevLut);
		material.SetVector(ShaderIDs._MakeevTonemappingLUTParams, value);
		return true;
	}
}
