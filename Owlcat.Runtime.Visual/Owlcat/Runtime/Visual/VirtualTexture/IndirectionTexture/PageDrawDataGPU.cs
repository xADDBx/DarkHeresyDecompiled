using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.IndirectionTexture;

[BurstCompile]
internal struct PageDrawDataGPU
{
	public float4 PageData;

	public float4 ScaleBias;
}
