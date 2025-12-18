using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Owlcat.Runtime.Visual.XPBD.DataStructures;

[BurstCompile]
public struct AffineTransform
{
	public float4 Translation;

	public float4 Scale;

	public quaternion Rotation;

	public AffineTransform(float4 translation, quaternion rotation, float4 scale)
	{
		translation[3] = 0f;
		scale[3] = 1f;
		Translation = translation;
		Rotation = rotation;
		Scale = scale;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FromTransform(TransformAccess source)
	{
		Translation = new float4(source.position, 0f);
		Rotation = source.rotation;
		Scale = new float4(source.localToWorldMatrix.lossyScale, 1f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FromTransform(Transform source)
	{
		Translation = new float4(source.position, 0f);
		Rotation = source.rotation;
		Scale = new float4(source.lossyScale, 1f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static AffineTransform operator *(AffineTransform a, AffineTransform b)
	{
		return new AffineTransform(a.TransformPoint(b.Translation), math.mul(a.Rotation, b.Rotation), a.Scale * b.Scale);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AffineTransform Inverse()
	{
		return new AffineTransform(new float4(math.rotate(math.conjugate(Rotation), (Translation / -Scale).xyz), 0f), math.conjugate(Rotation), 1f / Scale);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AffineTransform Interpolate(AffineTransform other, float translationalMu, float rotationalMu, float scaleMu)
	{
		return new AffineTransform(math.lerp(Translation, other.Translation, translationalMu), math.slerp(Rotation, other.Rotation, rotationalMu), math.lerp(Scale, other.Scale, scaleMu));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float4 TransformPoint(float4 point)
	{
		return new float4(Translation.xyz + math.rotate(Rotation, (point * Scale).xyz), 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float4 InverseTransformPoint(float4 point)
	{
		return new float4(math.rotate(math.conjugate(Rotation), (point - Translation).xyz) / Scale.xyz, 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float4 TransformPointUnscaled(float4 point)
	{
		return new float4(Translation.xyz + math.rotate(Rotation, point.xyz), 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float4 InverseTransformPointUnscaled(float4 point)
	{
		return new float4(math.rotate(math.conjugate(Rotation), (point - Translation).xyz), 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float4 TransformDirection(float4 direction)
	{
		return new float4(math.rotate(Rotation, direction.xyz), 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float4 InverseTransformDirection(float4 direction)
	{
		return new float4(math.rotate(math.conjugate(Rotation), direction.xyz), 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float4 TransformVector(float4 vector)
	{
		return new float4(math.rotate(Rotation, (vector * Scale).xyz), 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float4 InverseTransformVector(float4 vector)
	{
		return new float4(math.rotate(math.conjugate(Rotation), vector.xyz) / Scale.xyz, 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float4x4 TRS()
	{
		return float4x4.TRS(Translation.xyz, Rotation, Scale.xyz);
	}
}
