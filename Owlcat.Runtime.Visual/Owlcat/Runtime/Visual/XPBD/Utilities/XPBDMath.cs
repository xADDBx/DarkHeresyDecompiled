using System;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Utilities;

public static class XPBDMath
{
	public const float zero = 0f;

	public const float one = 1f;

	private static int[] s_PrimeNumbers = new int[29]
	{
		2, 5, 11, 17, 37, 67, 131, 257, 521, 1031,
		2053, 4099, 8209, 16411, 20261, 26407, 32771, 40577, 50021, 65539,
		80051, 95101, 110017, 120011, 131101, 262147, 524309, 1048583, 2097169
	};

	public static readonly Color32[] ColorAlphabet = new Color32[26]
	{
		new Color32(240, 163, byte.MaxValue, byte.MaxValue),
		new Color32(0, 117, 220, byte.MaxValue),
		new Color32(153, 63, 0, byte.MaxValue),
		new Color32(76, 0, 92, byte.MaxValue),
		new Color32(25, 25, 25, byte.MaxValue),
		new Color32(0, 92, 49, byte.MaxValue),
		new Color32(43, 206, 72, byte.MaxValue),
		new Color32(byte.MaxValue, 204, 153, byte.MaxValue),
		new Color32(128, 128, 128, byte.MaxValue),
		new Color32(148, byte.MaxValue, 181, byte.MaxValue),
		new Color32(143, 124, 0, byte.MaxValue),
		new Color32(157, 204, 0, byte.MaxValue),
		new Color32(194, 0, 136, byte.MaxValue),
		new Color32(0, 51, 128, byte.MaxValue),
		new Color32(byte.MaxValue, 164, 5, byte.MaxValue),
		new Color32(byte.MaxValue, 168, 187, byte.MaxValue),
		new Color32(66, 102, 0, byte.MaxValue),
		new Color32(byte.MaxValue, 0, 16, byte.MaxValue),
		new Color32(94, 241, 242, byte.MaxValue),
		new Color32(0, 153, 143, byte.MaxValue),
		new Color32(224, byte.MaxValue, 102, byte.MaxValue),
		new Color32(116, 10, byte.MaxValue, byte.MaxValue),
		new Color32(153, 0, 0, byte.MaxValue),
		new Color32(byte.MaxValue, byte.MaxValue, 128, byte.MaxValue),
		new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue),
		new Color32(byte.MaxValue, 80, 5, byte.MaxValue)
	};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3 IntegrateLinear(float3 position, float3 velocity, float dt)
	{
		return position + velocity * dt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3 DifferentiateLinear(float3 position, float3 prevPosition, float dt)
	{
		return (position - prevPosition) / dt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static quaternion AngularVelocityToSpinQuaternion(quaternion rotation, float4 angularVelocity, float dt)
	{
		quaternion a = new quaternion(angularVelocity.x, angularVelocity.y, angularVelocity.z, 0f);
		return new quaternion(0.5f * math.mul(a, rotation).value * dt);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static quaternion IntegrateAngular(quaternion rotation, float4 angularVelocity, float dt)
	{
		rotation.value += AngularVelocityToSpinQuaternion(rotation, angularVelocity, dt).value;
		return math.normalize(rotation);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float4 DifferentiateAngular(quaternion rotation, quaternion prevRotation, float dt)
	{
		return new float4((math.mul(rotation, math.inverse(prevRotation)).value * 2f / dt).xyz, 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Firstbitlow(uint bitMask)
	{
		if (bitMask == 0)
		{
			return -1;
		}
		int num = 0;
		while ((bitMask & 1) != 1)
		{
			bitMask >>= 1;
			num++;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsBitSetted(in uint bitMask, in int bitIndex)
	{
		return (bitMask & (uint)(1 << bitIndex)) != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static quaternion FromToRotation(in float3 from, in float3 to, in float t = 1f)
	{
		float3 x = math.normalize(from);
		float3 y = math.normalize(to);
		float num = math.dot(x, y);
		float num2 = math.acos(num);
		float3 x2 = math.cross(x, y);
		if (math.abs(1f + num) < 1E-06f)
		{
			num2 = MathF.PI;
			x2 = ((!(x.x > x.y) || !(x.x > x.z)) ? math.cross(x, new float3(1f, 0f, 0f)) : math.cross(x, new float3(0f, 1f, 0f)));
		}
		else if (math.abs(1f - num) < 1E-06f)
		{
			return quaternion.identity;
		}
		return quaternion.AxisAngle(math.normalize(x2), num2 * t);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float4 FromToRotationAxisAngle(in float3 from, in float3 to)
	{
		float3 x = math.normalize(from);
		float3 y = math.normalize(to);
		float num = math.dot(x, y);
		float w = math.acos(num);
		float3 x2 = math.cross(x, y);
		if (math.abs(1f + num) < 1E-06f)
		{
			w = MathF.PI;
			x2 = ((!(x.x > x.y) || !(x.x > x.z)) ? math.cross(x, new float3(1f, 0f, 0f)) : math.cross(x, new float3(0f, 1f, 0f)));
		}
		else if (math.abs(1f - num) < 1E-06f)
		{
			return new float4(1f, 0f, 0f, 0f);
		}
		return new float4(math.normalize(x2), w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Angle(in float3 from, in float3 to)
	{
		float3 x = math.normalize(from);
		float3 y = math.normalize(to);
		float num = math.dot(x, y);
		float result = math.acos(num);
		if (math.abs(1f + num) < 1E-06f)
		{
			result = MathF.PI;
		}
		else if (math.abs(1f - num) < 1E-06f)
		{
			result = 0f;
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float RestBendingConstraint(Vector3 positionA, Vector3 positionB, Vector3 positionC)
	{
		Vector3 vector = (positionA + positionB + positionC) / 3f;
		return (positionC - vector).magnitude;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float4 CreatePlane(float3 position, float3 normal)
	{
		float w = 0f - math.dot(position, normal);
		return new float4(normal, w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistanceToPlane(in float4 plane, in float3 point)
	{
		return math.dot(plane.xyz, point) + plane.w;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int NextPrimeNumber(int value)
	{
		for (int i = 0; i < s_PrimeNumbers.Length; i++)
		{
			if (s_PrimeNumbers[i] > value)
			{
				return s_PrimeNumbers[i];
			}
		}
		return s_PrimeNumbers[s_PrimeNumbers.Length - 1];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GetSimplexIndicesAndSize(ref int4 indices, in int offset, out int size)
	{
		size = 0;
		if (indices.x > -1)
		{
			indices.x += offset;
			size++;
		}
		if (indices.y > -1)
		{
			indices.y += offset;
			size++;
		}
		if (indices.z > -1)
		{
			indices.z += offset;
			size++;
		}
		if (indices.w > -1)
		{
			indices.w += offset;
			size++;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float4 BarycenterForSimplexOfSize(int simplexSize)
	{
		float value = 1f / (float)simplexSize;
		float4 result = float4.zero;
		for (int i = 0; i < simplexSize; i++)
		{
			result[i] = value;
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float BaryScale(float4 coords)
	{
		return math.rcp(math.dot(coords, coords));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float4 NearestPointOnEdge(in float4 a, in float4 b, in float4 p, out float mu, bool clampToSegment = true)
	{
		float4 x = p - a;
		float4 @float = b - a;
		mu = math.dot(x, @float) / math.dot(@float, @float);
		if (clampToSegment)
		{
			mu = math.saturate(mu);
		}
		return a + @float * mu;
	}

	public static void BarycentricInterpolation(in Vector3 p1, in Vector3 p2, in Vector3 p3, in Vector3 coords, out Vector3 result)
	{
		result.x = coords.x * p1.x + coords.y * p2.x + coords.z * p3.x;
		result.y = coords.x * p1.y + coords.y * p2.y + coords.z * p3.y;
		result.z = coords.x * p1.z + coords.y * p2.z + coords.z * p3.z;
	}

	public static bool LinePlaneIntersection(Vector3 planePoint, Vector3 planeNormal, Vector3 linePoint, Vector3 lineDirection, out Vector3 point)
	{
		point = linePoint;
		Vector3 normalized = lineDirection.normalized;
		float num = Vector3.Dot(planeNormal, normalized);
		if (Mathf.Approximately(num, 0f))
		{
			return false;
		}
		float num2 = (Vector3.Dot(planeNormal, planePoint) - Vector3.Dot(planeNormal, linePoint)) / num;
		point = linePoint + normalized * num2;
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void BarycentricInterpolation(in float3 p1, in float3 p2, in float3 p3, in float3 n1, in float3 n2, in float3 n3, in float3 coords, in float height, out float3 res)
	{
		res.x = coords.x * p1.x + coords.y * p2.x + coords.z * p3.x + (coords.x * n1.x + coords.y * n2.x + coords.z * n3.x) * height;
		res.y = coords.x * p1.y + coords.y * p2.y + coords.z * p3.y + (coords.x * n1.y + coords.y * n2.y + coords.z * n3.y) * height;
		res.z = coords.x * p1.z + coords.y * p2.z + coords.z * p3.z + (coords.x * n1.z + coords.y * n2.z + coords.z * n3.z) * height;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int DivRoundUp(int x, int y)
	{
		return (x + y - 1) / y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3 CalculateFrictionImpulse(in float3 prevPos, in float3 curPos, in float3 colliderVelocity, in float4 delta, in float friction)
	{
		float num = math.dot(delta, delta);
		float num2 = math.rsqrt(num + 1E-06f);
		float3 @float = delta.xyz * num2;
		float3 float2 = curPos - prevPos - colliderVelocity;
		float3 float3 = float2 - math.dot(float2, @float) * @float;
		float num3 = math.rsqrt(math.dot(float3, float3) * 1E-06f);
		float num4 = math.max((0f - friction) * num * num2 * num3, -1f);
		return float3 * num4;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float4 diagonal(this in float4x4 value)
	{
		return new float4(value.c0[0], value.c1[1], value.c2[2], value.c3[3]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float4 NearestPointOnTri(in CachedTri tri, float4 p, out float4 bary)
	{
		float4 y = tri.Vertex - p;
		float num = math.dot(tri.Edge0, y);
		float num2 = math.dot(tri.Edge1, y);
		float num3 = tri.Data[1] * num2 - tri.Data[2] * num;
		float num4 = tri.Data[1] * num - tri.Data[0] * num2;
		if (num3 + num4 <= tri.Data[3])
		{
			if (num3 < 0f)
			{
				if (num4 < 0f)
				{
					if (num < 0f)
					{
						num4 = 0f;
						num3 = ((!(0f - num >= tri.Data[0])) ? ((0f - num) / tri.Data[0]) : 1f);
					}
					else
					{
						num3 = 0f;
						num4 = ((num2 >= 0f) ? 0f : ((!(0f - num2 >= tri.Data[2])) ? ((0f - num2) / tri.Data[2]) : 1f));
					}
				}
				else
				{
					num3 = 0f;
					num4 = ((num2 >= 0f) ? 0f : ((!(0f - num2 >= tri.Data[2])) ? ((0f - num2) / tri.Data[2]) : 1f));
				}
			}
			else if (num4 < 0f)
			{
				num4 = 0f;
				num3 = ((num >= 0f) ? 0f : ((!(0f - num >= tri.Data[0])) ? ((0f - num) / tri.Data[0]) : 1f));
			}
			else
			{
				float num5 = 1f / tri.Data[3];
				num3 *= num5;
				num4 *= num5;
			}
		}
		else if (num3 < 0f)
		{
			float num6 = tri.Data[1] + num;
			float num7 = tri.Data[2] + num2;
			if (num7 > num6)
			{
				float num8 = num7 - num6;
				float num9 = tri.Data[0] - 2f * tri.Data[1] + tri.Data[2];
				if (num8 >= num9)
				{
					num3 = 1f;
					num4 = 0f;
				}
				else
				{
					num3 = num8 / num9;
					num4 = 1f - num3;
				}
			}
			else
			{
				num3 = 0f;
				num4 = ((num7 <= 0f) ? 1f : ((!(num2 >= 0f)) ? ((0f - num2) / tri.Data[2]) : 0f));
			}
		}
		else if (num4 < 0f)
		{
			float num6 = tri.Data[1] + num2;
			float num7 = tri.Data[0] + num;
			if (num7 > num6)
			{
				float num8 = num7 - num6;
				float num9 = tri.Data[0] - 2f * tri.Data[1] + tri.Data[2];
				if (num8 >= num9)
				{
					num4 = 1f;
					num3 = 0f;
				}
				else
				{
					num4 = num8 / num9;
					num3 = 1f - num4;
				}
			}
			else
			{
				num4 = 0f;
				num3 = ((num7 <= 0f) ? 1f : ((!(num >= 0f)) ? ((0f - num) / tri.Data[0]) : 0f));
			}
		}
		else
		{
			float num8 = tri.Data[2] + num2 - tri.Data[1] - num;
			if (num8 <= 0f)
			{
				num3 = 0f;
				num4 = 1f;
			}
			else
			{
				float num9 = tri.Data[0] - 2f * tri.Data[1] + tri.Data[2];
				if (num8 >= num9)
				{
					num3 = 1f;
					num4 = 0f;
				}
				else
				{
					num3 = num8 / num9;
					num4 = 1f - num3;
				}
			}
		}
		bary = new float4(1f - (num3 + num4), num3, num4, 0f);
		return tri.Vertex + num3 * tri.Edge0 + num4 * tri.Edge1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsIdentity(in float4x4 m)
	{
		float4x4 float4x = m - float4x4.identity;
		if (math.any(math.abs(float4x.c0) > 1E-05f) || math.any(math.abs(float4x.c1) > 1E-05f) || math.any(math.abs(float4x.c2) > 1E-05f) || math.any(math.abs(float4x.c3) > 1E-05f))
		{
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool AreEqual(in float4x4 m0, in float4x4 m1)
	{
		float4x4 float4x = m0 - m1;
		if (math.any(math.abs(float4x.c0) > 1E-05f) || math.any(math.abs(float4x.c1) > 1E-05f) || math.any(math.abs(float4x.c2) > 1E-05f) || math.any(math.abs(float4x.c3) > 1E-05f))
		{
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float PackWindFlagAndDamping(bool windEnabled, float damping)
	{
		uint num = math.f32tof16(damping);
		uint num2 = 0u;
		if (windEnabled)
		{
			num2 |= 1u;
		}
		return math.asfloat(num | (num2 << 16));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void UnpackWindFlagAndDamping(float packedFloat, out bool windEnabled, out float damping)
	{
		uint num = math.asuint(packedFloat);
		ushort num2 = (ushort)(num >> 16);
		windEnabled = (num2 & 1) != 0;
		damping = math.f16tof32(num & 0xFFFFu);
	}
}
