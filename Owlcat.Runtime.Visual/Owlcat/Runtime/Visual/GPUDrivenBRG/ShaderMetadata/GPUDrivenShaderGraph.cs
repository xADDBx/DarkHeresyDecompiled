using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.ShaderMetadata;

public static class GPUDrivenShaderGraph
{
	public const string kGuidTagName = "ShaderGraphGUID";

	private static readonly ShaderTagId s_GuidTag = new ShaderTagId("ShaderGraphGUID");

	[MustUseReturnValue]
	public static string GetGuidOrDefault(Shader shader)
	{
		string name = shader.FindSubshaderTagValue(0, s_GuidTag).name;
		if (string.IsNullOrWhiteSpace(name))
		{
			return string.Empty;
		}
		return name;
	}
}
