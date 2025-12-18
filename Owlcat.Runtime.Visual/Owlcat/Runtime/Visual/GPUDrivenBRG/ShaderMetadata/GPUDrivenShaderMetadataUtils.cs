using System;
using System.IO;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.ShaderMetadata;

internal static class GPUDrivenShaderMetadataUtils
{
	public const int kPassNotFoundIndex = -1;

	public const int kSubshaderIndex = 0;

	public const string kBreaksBatchingAttributeName = "BreaksBatching";

	public const string kDoesNotBreakBatchingAttributeName = "DoesNotBreakBatching";

	private static readonly ShaderTagId s_LightModeTag = new ShaderTagId("LightMode");

	private static readonly ShaderTagId s_ShadowCasterPassTag = new ShaderTagId("ShadowCaster");

	private static readonly ShaderTagId s_DepthOnlyPassTag = new ShaderTagId("DepthOnly");

	private static readonly ShaderTagId s_MotionVectorsPassTag = new ShaderTagId("MotionVectors");

	public static string GetLightModeValue(Shader shader, int passIndex)
	{
		string text = shader.FindPassTagValue(0, passIndex, s_LightModeTag).name;
		if (string.IsNullOrWhiteSpace(text))
		{
			text = string.Empty;
		}
		return text;
	}

	private static ShaderTagId GetLightMode(Shader shader, int passIndex)
	{
		return shader.FindPassTagValue(0, passIndex, s_LightModeTag);
	}

	public static int FindShaderCasterPassIndex(Shader shader)
	{
		return FindPassIndex(shader, s_ShadowCasterPassTag);
	}

	public static int FindDepthOnlyPassIndex(Shader shader)
	{
		return FindPassIndex(shader, s_DepthOnlyPassTag);
	}

	public static int FindMotionVectorPassIndex(Shader shader)
	{
		return FindPassIndex(shader, s_MotionVectorsPassTag);
	}

	private static int FindPassIndex(Shader shader, ShaderTagId passTag)
	{
		int passCountInSubshader = shader.GetPassCountInSubshader(0);
		for (int i = 0; i < passCountInSubshader; i++)
		{
			if (GetLightMode(shader, i).Equals(passTag))
			{
				return i;
			}
		}
		return -1;
	}

	public static GPUDrivenShaderMetadata.TypeFlags CollectTypeFlags(GPUDrivenShaderMetadata.PassMetadata[] metadataPasses)
	{
		GPUDrivenShaderMetadata.TypeFlags typeFlags = GPUDrivenShaderMetadata.TypeFlags.None;
		for (int i = 0; i < metadataPasses.Length; i++)
		{
			GPUDrivenShaderMetadata.PassMetadata passMetadata = metadataPasses[i];
			if ((typeFlags & GPUDrivenShaderMetadata.TypeFlags.DecalAny) != 0)
			{
				continue;
			}
			string lightMode = passMetadata.LightMode;
			if (!(lightMode == "DecalDeferred"))
			{
				if (lightMode == "DecalGUI")
				{
					typeFlags |= GPUDrivenShaderMetadata.TypeFlags.DecalGUI;
				}
			}
			else
			{
				typeFlags |= GPUDrivenShaderMetadata.TypeFlags.DecalDeferred;
			}
		}
		return typeFlags;
	}

	public static GPUDrivenShaderMetadata.PassMetadata[] MigratePasses(Shader shader, GPUDrivenShaderMetadata.PassMetadata[] bakedPassMetadata, string[] bakedKeywordNames, string[] newKeywordNames)
	{
		GPUDrivenShaderMetadata.PassMetadata[] array = new GPUDrivenShaderMetadata.PassMetadata[shader.GetPassCountInSubshader(0)];
		for (int i = 0; i < array.Length; i++)
		{
			ref GPUDrivenShaderMetadata.PassMetadata reference = ref array[i];
			reference.LocalKeywordsMask = default(BitMask256);
			reference.LightMode = GetLightModeValue(shader, i);
			int num = FindPassByLightMode(bakedPassMetadata, reference.LightMode);
			if (num < 0)
			{
				continue;
			}
			BitMask256 localKeywordsMask = bakedPassMetadata[num].LocalKeywordsMask;
			foreach (int item in localKeywordsMask)
			{
				string value = bakedKeywordNames[item];
				int num2 = Array.IndexOf(newKeywordNames, value);
				if (num2 >= 0)
				{
					reference.LocalKeywordsMask.SetBit(num2, value: true);
				}
			}
		}
		return array;
	}

	private static int FindPassByLightMode(GPUDrivenShaderMetadata.PassMetadata[] passMetadata, string lightMode)
	{
		for (int i = 0; i < passMetadata.Length; i++)
		{
			if (passMetadata[i].LightMode == lightMode)
			{
				return i;
			}
		}
		return -1;
	}

	public static LocalKeyword[] CreateLocalKeywords(Shader shader, string[] localKeywordNames)
	{
		LocalKeyword[] array = new LocalKeyword[localKeywordNames.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new LocalKeyword(shader, localKeywordNames[i]);
		}
		return array;
	}

	public static void WriteToFile(GPUDrivenBakedShaderMetadata bakedShaderMetadata, string assetPath)
	{
		string contents = JsonUtility.ToJson(bakedShaderMetadata);
		File.WriteAllText(assetPath, contents);
	}

	public static GPUDrivenBakedShaderMetadata ReadFromFile(string assetPath)
	{
		return JsonUtility.FromJson<GPUDrivenBakedShaderMetadata>(File.ReadAllText(assetPath)) ?? new GPUDrivenBakedShaderMetadata();
	}
}
