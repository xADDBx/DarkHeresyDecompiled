using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU;

public static class LocalOptimization
{
	public struct SurfacePoint
	{
		public float4 Bary;

		public float4 Point;

		public float4 Normal;
	}

	public interface IDistanceFunction
	{
		void Evaluate(float4 point, float4 radii, ref SurfacePoint projectedPoint);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void GetInterpolatedSimplexData(in int4 simplexIndices, in int simplexSize, NativeArray<float3> positions, NativeArray<float> radii, in float4 convexBary, out float4 convexPoint, out float4 convexRadii)
	{
		convexPoint = float4.zero;
		convexRadii = float4.zero;
		for (int i = 0; i < simplexSize; i++)
		{
			int index = simplexIndices[i];
			convexPoint.xyz += positions[index] * convexBary[i];
			convexRadii.xyz += radii[index] * convexBary[i];
		}
	}

	public static SurfacePoint Optimize<T>(ref T distanceFunction, in int4 simplexIndices, in int simplexSize, NativeArray<float3> positions, NativeArray<float> radii, ref float4 convexBary, out float4 convexPoint, in int maxIterations, in float tolerance = 0.004f) where T : struct, IDistanceFunction
	{
		SurfacePoint pointInFunction = default(SurfacePoint);
		GetInterpolatedSimplexData(in simplexIndices, in simplexSize, positions, radii, in convexBary, out convexPoint, out var convexRadii);
		if (simplexSize == 1 || maxIterations < 1)
		{
			distanceFunction.Evaluate(convexPoint, convexRadii, ref pointInFunction);
		}
		else if (simplexSize == 2)
		{
			GoldenSearch(ref distanceFunction, simplexIndices, simplexSize, positions, radii, ref convexPoint, ref convexRadii, ref convexBary, ref pointInFunction, maxIterations, tolerance * 10f);
		}
		else
		{
			FrankWolfe(ref distanceFunction, simplexIndices, simplexSize, positions, radii, ref convexPoint, ref convexRadii, ref convexBary, ref pointInFunction, maxIterations, tolerance);
		}
		return pointInFunction;
	}

	private static void FrankWolfe<T>(ref T distanceFunction, int4 simplexIndices, int simplexSize, NativeArray<float3> positions, NativeArray<float> radii, ref float4 convexPoint, ref float4 convexThickness, ref float4 convexBary, ref SurfacePoint pointInFunction, int maxIterations, float tolerance) where T : struct, IDistanceFunction
	{
		for (int i = 0; i < maxIterations; i++)
		{
			distanceFunction.Evaluate(convexPoint, convexThickness, ref pointInFunction);
			int index = 0;
			float num = float.MinValue;
			for (int j = 0; j < simplexSize; j++)
			{
				int index2 = simplexIndices[j];
				float4 y = new float4(positions[index2], 0f) - convexPoint;
				y -= pointInFunction.Normal * (radii[index2] - convexThickness.x);
				float num2 = math.dot(-pointInFunction.Normal, y);
				if (num2 > num)
				{
					index = j;
					num = num2;
				}
			}
			if (!(num < tolerance))
			{
				float num3 = 0.6f / (float)(i + 2);
				convexBary *= 1f - num3;
				convexBary[index] += num3;
				GetInterpolatedSimplexData(in simplexIndices, in simplexSize, positions, radii, in convexBary, out convexPoint, out convexThickness);
				continue;
			}
			break;
		}
	}

	private static void GoldenSearch<T>(ref T distanceFunction, int4 simplexIndices, int simplexSize, NativeArray<float3> positions, NativeArray<float> radii, ref float4 convexPoint, ref float4 convexThickness, ref float4 convexBary, ref SurfacePoint pointInFunction, int maxIterations, float tolerance) where T : struct, IDistanceFunction
	{
		SurfacePoint projectedPoint = default(SurfacePoint);
		float num = (math.sqrt(5f) + 1f) / 2f;
		float num2 = 0f;
		float num3 = 1f;
		float num4 = num3 - (num3 - num2) / num;
		float num5 = num2 + (num3 - num2) / num;
		for (int i = 0; i < maxIterations; i++)
		{
			if (math.abs(num3 - num2) < tolerance * (math.abs(num4) + math.abs(num5)))
			{
				break;
			}
			NativeArray<float3> positions2 = positions;
			NativeArray<float> radii2 = radii;
			float4 convexBary2 = new float4(num4, 1f - num4, 0f, 0f);
			GetInterpolatedSimplexData(in simplexIndices, in simplexSize, positions2, radii2, in convexBary2, out convexPoint, out convexThickness);
			NativeArray<float3> positions3 = positions;
			NativeArray<float> radii3 = radii;
			convexBary2 = new float4(num5, 1f - num5, 0f, 0f);
			GetInterpolatedSimplexData(in simplexIndices, in simplexSize, positions3, radii3, in convexBary2, out var convexPoint2, out var convexRadii);
			distanceFunction.Evaluate(convexPoint, convexThickness, ref pointInFunction);
			distanceFunction.Evaluate(convexPoint2, convexRadii, ref projectedPoint);
			float4 y = new float4(positions[simplexIndices.x], 0f) - pointInFunction.Point;
			float4 y2 = new float4(positions[simplexIndices.y], 0f) - projectedPoint.Point;
			y -= pointInFunction.Normal * (radii[simplexIndices.x] - convexThickness.x);
			y2 -= projectedPoint.Normal * (radii[simplexIndices.y] - convexRadii.x);
			if (math.dot(-pointInFunction.Normal, y) < math.dot(-projectedPoint.Normal, y2))
			{
				num3 = num5;
			}
			else
			{
				num2 = num4;
			}
			num4 = num3 - (num3 - num2) / num;
			num5 = num2 + (num3 - num2) / num;
		}
		convexBary.y = 1f - (convexBary.x = (num3 + num2) * 0.5f);
		GetInterpolatedSimplexData(in simplexIndices, in simplexSize, positions, radii, in convexBary, out convexPoint, out convexThickness);
		distanceFunction.Evaluate(convexPoint, convexThickness, ref pointInFunction);
	}
}
