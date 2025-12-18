using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@01c4fcbc474f\\Runtime\\GPUDrivenBRG\\GPUDrivenPerInstanceMetadata.cs", needAccessors = false)]
public struct GPUDrivenPerInstanceMetadata
{
	public uint InstanceDataOffset;

	public uint MaterialDataOffset;
}
