using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public struct GPUDrivenCullingContext
{
	public struct LODInfo
	{
		public bool IsOrtho;

		public bool UseSelectionForcedLOD;

		public float SqrScreenRelativeMetric;

		public Vector3 CameraPosition;

		public int MaxLOD;

		public int FixedLODIndex;

		public float LODBias;
	}

	public struct CullingBufferInfo
	{
		public GraphicsBufferHandle Handle;

		public int Capacity;

		public static CullingBufferInfo FromGraphicsBuffer(ResizableGraphicsBuffer graphicsBuffer)
		{
			CullingBufferInfo result = default(CullingBufferInfo);
			result.Capacity = graphicsBuffer.Count;
			result.Handle = graphicsBuffer.BufferHandle;
			return result;
		}
	}

	public BatchPackedCullingViewID ViewID;

	public BatchCullingViewType BatchCullingViewType;

	public GPUDrivenRendererGroupPool.ViewType ViewType;

	public ulong SceneCullingMask;

	public NativeArray<Plane> FrustumPlanes;

	public float4 CullingSphereLS;

	public float3x3 WorldToLightSpaceRotation;

	public Matrix4x4 CullingMatrix;

	public Vector3 CameraPosition;

	public int CullingResourcesIndex;

	public int PersistentIndicesOffset;

	public CullingBufferInfo VisibleIndicesBuffer;

	public int VisibleIndicesOffset;

	public int VisibleIndicesCount;

	public CullingBufferInfo IndirectArgsBuffer;

	public int IndirectArgsOffset;

	public int IndirectArgsCount;

	public CullingBufferInfo CPUInstanceVisibilityMaskBuffer;

	public int CPUInstanceVisibilityMaskOffset;

	public int CPUInstanceVisibilityMaskCount;

	public JobHandle CPUInstanceVisibilityJobHandle;

	public NativeArray<uint> CPUInstanceVisibilityMask;

	public bool OwnsCPUInstanceVisibilityMask;

	public int? InstanceVisibilityMaskIndex;

	public CameraType CameraType;

	public LODInfo LOD;

	public void ClearCPUInstanceVisibility()
	{
		if (!CPUInstanceVisibilityJobHandle.Equals(default(JobHandle)))
		{
			if (OwnsCPUInstanceVisibilityMask)
			{
				CPUInstanceVisibilityJobHandle.Complete();
			}
			CPUInstanceVisibilityJobHandle = default(JobHandle);
		}
		if (CPUInstanceVisibilityMask.IsCreated)
		{
			if (OwnsCPUInstanceVisibilityMask)
			{
				CPUInstanceVisibilityMask.Dispose();
			}
			CPUInstanceVisibilityMask = default(NativeArray<uint>);
		}
	}
}
