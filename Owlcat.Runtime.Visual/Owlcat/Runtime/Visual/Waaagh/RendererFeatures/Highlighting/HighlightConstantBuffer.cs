using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting;

public static class HighlightConstantBuffer
{
	public static int _ZTest = Shader.PropertyToID("_ZTest");

	public static int _ZWrite = Shader.PropertyToID("_ZWrite");

	public static int _Color = Shader.PropertyToID("_Color");

	public static int _Alphatest = Shader.PropertyToID("_Alphatest");

	public static int _CullMode = Shader.PropertyToID("_CullMode");

	public static int _BaseMap = Shader.PropertyToID("_BaseMap");

	public static int _Cutoff = Shader.PropertyToID("_Cutoff");

	public static int _HighlightingBlurOffset = Shader.PropertyToID("_HighlightingBlurOffset");
}
