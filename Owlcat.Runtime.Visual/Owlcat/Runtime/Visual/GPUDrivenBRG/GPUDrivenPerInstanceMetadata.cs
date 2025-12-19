using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\GPUDrivenBRG\\GPUDrivenPerInstanceMetadata.cs", needAccessors = false)]
public struct GPUDrivenPerInstanceMetadata
{
	public uint InstanceDataOffset;

	public uint MaterialDataOffset;
}
