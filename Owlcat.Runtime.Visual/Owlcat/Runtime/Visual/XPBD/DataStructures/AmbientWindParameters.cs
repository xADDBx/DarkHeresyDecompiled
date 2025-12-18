using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.DataStructures;

[BurstCompile]
public struct AmbientWindParameters
{
	public float2 Velocity;

	public float StrengthNoiseWeight;

	public float StrengthNoiseContrast;

	public float4 StrengthOctave0;

	public float4 StrengthOctave1;

	public float4 ShiftOctave0;

	public float4 ShiftOctave1;
}
