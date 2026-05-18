using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

internal struct ReceiverPlanes
{
	public NativeList<Plane> Planes;

	public int LightFacingPlaneCount;

	private static bool IsSignBitSet(float x)
	{
		return math.asuint(x) >> 31 != 0;
	}

	internal NativeArray<Plane> LightFacingFrustumPlaneSubArray()
	{
		return Planes.AsArray().GetSubArray(0, LightFacingPlaneCount);
	}

	internal JobHandle Dispose(JobHandle job)
	{
		return Planes.Dispose(job);
	}

	internal void Init(in BatchCullingContext cc, NativeArray<Plane> cameraReceiverPlanes)
	{
		if (cc.viewType != BatchCullingViewType.Light)
		{
			return;
		}
		NativeArray<Plane> nativeArray = ((cc.receiverPlaneCount == 0) ? cameraReceiverPlanes : cc.cullingPlanes.GetSubArray(cc.receiverPlaneOffset, cc.receiverPlaneCount));
		if (nativeArray.Length == 0)
		{
			return;
		}
		bool flag = false;
		if (cc.cullingSplits.Length > 0)
		{
			Matrix4x4 cullingMatrix = cc.cullingSplits[0].cullingMatrix;
			flag = cullingMatrix[15] == 1f && cullingMatrix[11] == 0f && cullingMatrix[7] == 0f && cullingMatrix[3] == 0f;
		}
		if (flag)
		{
			Vector3 vector = -cc.localToWorldMatrix.GetColumn(2);
			int num = 0;
			for (int i = 0; i < nativeArray.Length; i++)
			{
				Plane value = nativeArray[i];
				if (IsSignBitSet(Vector3.Dot(value.normal, vector)))
				{
					num |= 1 << i;
				}
				else
				{
					Planes.Add(in value);
				}
			}
			LightFacingPlaneCount = Planes.Length;
			if (nativeArray.Length != 6)
			{
				return;
			}
			for (int j = 0; j < nativeArray.Length; j++)
			{
				for (int k = j + 1; k < nativeArray.Length; k++)
				{
					if (j / 2 != k / 2 && ((uint)((num >> j) ^ (num >> k)) & (true ? 1u : 0u)) != 0)
					{
						int index;
						int index2;
						if (((uint)(num >> j) & (true ? 1u : 0u)) != 0)
						{
							int num2 = k;
							int num3 = j;
							index = num2;
							index2 = num3;
						}
						else
						{
							int num4 = j;
							int num3 = k;
							index = num4;
							index2 = num3;
						}
						Plane plane = nativeArray[index];
						Plane plane2 = nativeArray[index2];
						float4 a = new float4(plane.normal, plane.distance);
						float4 b = new float4(plane2.normal, plane2.distance);
						float4 x = Line.PlaneContainingLineWithNormalPerpendicularToVector(Line.LineOfPlaneIntersectingPlane(a, b), vector);
						x /= math.length(x.xyz);
						if (!math.any(math.isnan(x)))
						{
							ref NativeList<Plane> planes = ref Planes;
							Plane value2 = new Plane((Vector3)x.xyz, x.w);
							planes.Add(in value2);
						}
					}
				}
			}
			return;
		}
		Vector3 position = cc.localToWorldMatrix.GetPosition();
		int num5 = 0;
		for (int l = 0; l < nativeArray.Length; l++)
		{
			Plane value3 = nativeArray[l];
			if (IsSignBitSet(value3.GetDistanceToPoint(position)))
			{
				num5 |= 1 << l;
			}
			else
			{
				Planes.Add(in value3);
			}
		}
		LightFacingPlaneCount = Planes.Length;
		if (nativeArray.Length != 6)
		{
			return;
		}
		for (int m = 0; m < nativeArray.Length; m++)
		{
			for (int n = m + 1; n < nativeArray.Length; n++)
			{
				if (m / 2 != n / 2 && ((uint)((num5 >> m) ^ (num5 >> n)) & (true ? 1u : 0u)) != 0)
				{
					int index3;
					int index4;
					if (((uint)(num5 >> m) & (true ? 1u : 0u)) != 0)
					{
						int num6 = n;
						int num3 = m;
						index3 = num6;
						index4 = num3;
					}
					else
					{
						int num7 = m;
						int num3 = n;
						index3 = num7;
						index4 = num3;
					}
					Plane plane3 = nativeArray[index3];
					Plane plane4 = nativeArray[index4];
					float4 a2 = new float4(plane3.normal, plane3.distance);
					float4 b2 = new float4(plane4.normal, plane4.distance);
					float4 x2 = Line.PlaneContainingLineAndPoint(Line.LineOfPlaneIntersectingPlane(a2, b2), position);
					x2 /= math.length(x2.xyz);
					if (!math.any(math.isnan(x2)))
					{
						ref NativeList<Plane> planes2 = ref Planes;
						Plane value2 = new Plane((Vector3)x2.xyz, x2.w);
						planes2.Add(in value2);
					}
				}
			}
		}
	}
}
