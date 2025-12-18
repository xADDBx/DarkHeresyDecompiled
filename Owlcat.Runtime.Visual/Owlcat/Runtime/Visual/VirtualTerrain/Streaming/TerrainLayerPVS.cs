using System;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTerrain.Streaming;

[Serializable]
internal struct TerrainLayerPVS
{
	public float2 Position;

	public float2 Size;

	public float Radius0;

	public float Radius1;

	public int2 NodesCount;

	public ulong[] Nodes0;

	public ulong[] Nodes1;

	public bool IsValid()
	{
		if (Size.x > 0f && Size.y > 0f && NodesCount.x > 0 && NodesCount.y > 0 && Nodes0 != null && Nodes0.Length == NodesCount.x * NodesCount.y && Nodes1 != null)
		{
			return Nodes1.Length == NodesCount.x * NodesCount.y;
		}
		return false;
	}

	public void GetVisibleLayerMasks(float2 worldPosition, out ulong mask0, out ulong mask1)
	{
		int2 valueToClamp = (int2)(NodesCount * (worldPosition - Position) / Size);
		valueToClamp = math.clamp(valueToClamp, 0, NodesCount - 1);
		int num = valueToClamp.x + valueToClamp.y * NodesCount.x;
		mask0 = Nodes0[num];
		mask1 = Nodes1[num];
	}
}
