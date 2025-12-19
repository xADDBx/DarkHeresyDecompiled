using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Lighting;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Lighting\\TranslucencyProfilesConstantBuffer.cs", needAccessors = false, generateCBuffer = true)]
public struct TranslucencyProfilesConstantBuffer
{
	public float4 _TranslucencyProfileCount;

	[HLSLArray(16, typeof(Vector4))]
	public unsafe fixed float _TranslucencyProfileColors[64];

	[HLSLArray(16, typeof(Vector4))]
	public unsafe fixed float _TranslucencyProfileSelfShadowingWorkaroundData[64];
}
