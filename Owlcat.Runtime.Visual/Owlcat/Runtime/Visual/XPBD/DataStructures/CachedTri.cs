using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.DataStructures;

[BurstCompile]
public struct CachedTri
{
	public float4 Vertex;

	public float4 Edge0;

	public float4 Edge1;

	public float4 Data;

	public void Cache(float4 v1, float4 v2, float4 v3)
	{
		Vertex = v1;
		Edge0 = v2 - v1;
		Edge1 = v3 - v1;
		Data = float4.zero;
		Data[0] = math.dot(Edge0, Edge0);
		Data[1] = math.dot(Edge0, Edge1);
		Data[2] = math.dot(Edge1, Edge1);
		Data[3] = Data[0] * Data[2] - Data[1] * Data[1];
	}
}
