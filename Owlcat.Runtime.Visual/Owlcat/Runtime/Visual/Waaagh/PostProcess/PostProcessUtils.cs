using Owlcat.Runtime.Visual.Overrides;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.PostProcess;

public static class PostProcessUtils
{
	private static class ShaderConstants
	{
		public static readonly int _Grain_Texture = Shader.PropertyToID("_Grain_Texture");

		public static readonly int _Grain_Params = Shader.PropertyToID("_Grain_Params");

		public static readonly int _Grain_TilingParams = Shader.PropertyToID("_Grain_TilingParams");

		public static readonly int _BlueNoise_Texture = Shader.PropertyToID("_BlueNoise_Texture");

		public static readonly int _Dithering_Params = Shader.PropertyToID("_Dithering_Params");

		public static readonly int _SourceSize = Shader.PropertyToID("_SourceSize");
	}

	public static int ConfigureDithering(PostProcessResources resources, int index, int targetWidth, int targetHeight, Material material)
	{
		Texture2D[] blueNoise16Textures = resources.Textures.BlueNoise16Textures;
		if (blueNoise16Textures == null || blueNoise16Textures.Length == 0)
		{
			return 0;
		}
		if (++index >= blueNoise16Textures.Length)
		{
			index = 0;
		}
		float value = Random.value;
		float value2 = Random.value;
		Texture2D texture2D = blueNoise16Textures[index];
		material.SetTexture(ShaderConstants._BlueNoise_Texture, texture2D);
		material.SetVector(ShaderConstants._Dithering_Params, new Vector4((float)targetWidth / (float)texture2D.width, (float)targetHeight / (float)texture2D.height, value, value2));
		return index;
	}

	public static void ConfigureFilmGrain(PostProcessResources resources, FilmGrain settings, int targetWidth, int targetHeight, Material material)
	{
		Texture texture = settings.texture.value;
		if (settings.type.value != FilmGrainLookup.Custom)
		{
			texture = resources.Textures.FilmGrainTex[(int)settings.type.value];
		}
		float value = Random.value;
		float value2 = Random.value;
		Vector4 value3 = ((texture == null) ? Vector4.zero : new Vector4((float)targetWidth / (float)texture.width, (float)targetHeight / (float)texture.height, value, value2));
		material.SetTexture(ShaderConstants._Grain_Texture, texture);
		material.SetVector(ShaderConstants._Grain_Params, new Vector2(settings.intensity.value * 4f, settings.response.value));
		material.SetVector(ShaderConstants._Grain_TilingParams, value3);
	}

	internal static void SetSourceSize(RasterCommandBuffer cmd, RTHandle source)
	{
		float num = source.rt.width;
		float num2 = source.rt.height;
		if (source.rt.useDynamicScale)
		{
			num *= ScalableBufferManager.widthScaleFactor;
			num2 *= ScalableBufferManager.heightScaleFactor;
		}
		cmd.SetGlobalVector(ShaderConstants._SourceSize, new Vector4(num, num2, 1f / num, 1f / num2));
	}
}
