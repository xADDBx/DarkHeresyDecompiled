using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Experimental.Geometry;

[BurstCompile]
public static class GeometryMath
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Overlaps(Aabb a, Aabb b)
	{
		if (math.all(a.Min <= b.Max))
		{
			return math.all(b.Min <= a.Max);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Overlaps(Obb obb, float4 sphereSq)
	{
		return math.lengthsq(sphereSq.xyz - obb.ClosestPoint(sphereSq)) <= sphereSq.w;
	}

	public static bool Overlaps(Obb boxA, Obb boxB)
	{
		ref float3 extents = ref boxA.Extents;
		ref float3 extents2 = ref boxB.Extents;
		ref float3x3 orientation = ref boxA.Orientation;
		ref float3x3 orientation2 = ref boxB.Orientation;
		float3 x = boxB.Position - boxA.Position;
		float3 @float = new float3(math.dot(x, orientation[0]), math.dot(x, orientation[1]), math.dot(x, orientation[2]));
		float3x3 float3x = new float3x3(new float3(math.dot(orientation[0], orientation2[0]), math.dot(orientation[0], orientation2[1]), math.dot(orientation[0], orientation2[2])), new float3(math.dot(orientation[1], orientation2[0]), math.dot(orientation[1], orientation2[1]), math.dot(orientation[1], orientation2[2])), new float3(math.dot(orientation[2], orientation2[0]), math.dot(orientation[2], orientation2[1]), math.dot(orientation[2], orientation2[2])));
		float num = extents[0];
		float num2 = extents2[0] * math.abs(float3x[0][0]) + extents2[1] * math.abs(float3x[0][1]) + extents2[2] * math.abs(float3x[0][2]);
		if (math.abs(@float[0]) > num + num2)
		{
			return false;
		}
		num = extents[1];
		num2 = extents2[0] * math.abs(float3x[1][0]) + extents2[1] * math.abs(float3x[1][1]) + extents2[2] * math.abs(float3x[1][2]);
		if (math.abs(@float[1]) > num + num2)
		{
			return false;
		}
		num = extents[2];
		num2 = extents2[0] * math.abs(float3x[2][0]) + extents2[1] * math.abs(float3x[2][1]) + extents2[2] * math.abs(float3x[2][2]);
		if (math.abs(@float[2]) > num + num2)
		{
			return false;
		}
		num = extents[0] * math.abs(float3x[0][0]) + extents[1] * math.abs(float3x[1][0]) + extents[2] * math.abs(float3x[2][0]);
		num2 = extents2[0];
		if (math.abs(@float[0] * float3x[0][0] + @float[1] * float3x[1][0] + @float[2] * float3x[2][0]) > num + num2)
		{
			return false;
		}
		num = extents[0] * math.abs(float3x[0][1]) + extents[1] * math.abs(float3x[1][1]) + extents[2] * math.abs(float3x[2][1]);
		num2 = extents2[1];
		if (math.abs(@float[0] * float3x[0][1] + @float[1] * float3x[1][1] + @float[2] * float3x[2][1]) > num + num2)
		{
			return false;
		}
		num = extents[0] * math.abs(float3x[0][2]) + extents[1] * math.abs(float3x[1][2]) + extents[2] * math.abs(float3x[2][2]);
		num2 = extents2[2];
		if (math.abs(@float[0] * float3x[0][2] + @float[1] * float3x[1][2] + @float[2] * float3x[2][2]) > num + num2)
		{
			return false;
		}
		num = extents[1] * math.abs(float3x[2][0]) + extents[2] * math.abs(float3x[1][0]);
		num2 = extents2[1] * math.abs(float3x[0][2]) + extents2[2] * math.abs(float3x[0][1]);
		if (math.abs(float3x[1][0] * @float[2] - float3x[2][0] * @float[1]) > num + num2)
		{
			return false;
		}
		num = extents[1] * math.abs(float3x[2][1]) + extents[2] * math.abs(float3x[1][1]);
		num2 = extents2[0] * math.abs(float3x[0][2]) + extents2[2] * math.abs(float3x[0][0]);
		if (math.abs(float3x[1][1] * @float[2] - float3x[2][1] * @float[1]) > num + num2)
		{
			return false;
		}
		num = extents[1] * math.abs(float3x[2][2]) + extents[2] * math.abs(float3x[1][2]);
		num2 = extents2[0] * math.abs(float3x[0][1]) + extents2[1] * math.abs(float3x[0][0]);
		if (math.abs(float3x[1][2] * @float[2] - float3x[2][2] * @float[1]) > num + num2)
		{
			return false;
		}
		num = extents[0] * math.abs(float3x[2][0]) + extents[2] * math.abs(float3x[0][0]);
		num2 = extents2[1] * math.abs(float3x[1][2]) + extents2[2] * math.abs(float3x[1][1]);
		if (math.abs(float3x[2][0] * @float[0] - float3x[0][0] * @float[2]) > num + num2)
		{
			return false;
		}
		num = extents[0] * math.abs(float3x[2][1]) + extents[2] * math.abs(float3x[0][1]);
		num2 = extents2[0] * math.abs(float3x[1][2]) + extents2[2] * math.abs(float3x[1][0]);
		if (math.abs(float3x[2][1] * @float[0] - float3x[0][1] * @float[2]) > num + num2)
		{
			return false;
		}
		num = extents[0] * math.abs(float3x[2][2]) + extents[2] * math.abs(float3x[0][2]);
		num2 = extents2[0] * math.abs(float3x[1][1]) + extents2[1] * math.abs(float3x[1][0]);
		if (math.abs(float3x[2][2] * @float[0] - float3x[0][2] * @float[2]) > num + num2)
		{
			return false;
		}
		num = extents[0] * math.abs(float3x[1][0]) + extents[1] * math.abs(float3x[0][0]);
		num2 = extents2[1] * math.abs(float3x[2][2]) + extents2[2] * math.abs(float3x[2][1]);
		if (math.abs(float3x[0][0] * @float[1] - float3x[1][0] * @float[0]) > num + num2)
		{
			return false;
		}
		num = extents[0] * math.abs(float3x[1][1]) + extents[1] * math.abs(float3x[0][1]);
		num2 = extents2[0] * math.abs(float3x[2][2]) + extents2[2] * math.abs(float3x[2][0]);
		if (math.abs(float3x[0][1] * @float[1] - float3x[1][1] * @float[0]) > num + num2)
		{
			return false;
		}
		num = extents[0] * math.abs(float3x[1][2]) + extents[1] * math.abs(float3x[0][2]);
		num2 = extents2[0] * math.abs(float3x[2][1]) + extents2[1] * math.abs(float3x[2][0]);
		if (math.abs(float3x[0][2] * @float[1] - float3x[1][2] * @float[0]) > num + num2)
		{
			return false;
		}
		return true;
	}
}
