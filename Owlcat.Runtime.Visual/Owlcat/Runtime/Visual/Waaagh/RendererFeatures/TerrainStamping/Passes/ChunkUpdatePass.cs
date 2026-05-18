using JetBrains.Annotations;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping.Passes;

internal static class ChunkUpdatePass
{
	public static void Setup(in SetupContext context, TerrainStampingManager terrainStampingManager, TerrainStampingManagerParameters parameters)
	{
		Camera mainCamera = GetMainCamera(context.CameraData);
		if (mainCamera == null)
		{
			return;
		}
		terrainStampingManager.OnSetup();
		Plane plane = new Plane(Vector3.up, 0f);
		NativeArray<Ray> nativeArray = new NativeArray<Ray>(4, Allocator.Temp);
		nativeArray[0] = mainCamera.ViewportPointToRay(new Vector3(0f, 0f, 0f));
		nativeArray[1] = mainCamera.ViewportPointToRay(new Vector3(1f, 0f, 0f));
		nativeArray[2] = mainCamera.ViewportPointToRay(new Vector3(0f, 1f, 0f));
		nativeArray[3] = mainCamera.ViewportPointToRay(new Vector3(1f, 1f, 0f));
		NativeArray<Ray> nativeArray2 = nativeArray;
		float2 @float = float.PositiveInfinity;
		float2 float2 = float.NegativeInfinity;
		float num = math.min(parameters.ChunkAllocationMaxDistance, mainCamera.farClipPlane);
		foreach (Ray item in nativeArray2)
		{
			if (!plane.Raycast(item, out float enter))
			{
				float3 float3 = item.GetPoint(num);
				@float = math.min(@float, float3.xz);
				float2 = math.max(float2, float3.xz);
			}
			else
			{
				enter = math.min(num, enter);
				float3 float4 = item.GetPoint(enter);
				@float = math.min(@float, float4.xz);
				float2 = math.max(float2, float4.xz);
			}
		}
		if (math.all(float2 - @float > 0f))
		{
			terrainStampingManager.UpdateChunkExtents(@float, float2);
		}
	}

	[CanBeNull]
	private static Camera GetMainCamera(WaaaghCameraData cameraData)
	{
		if (cameraData.isGameCamera)
		{
			return cameraData.camera;
		}
		Camera main = Camera.main;
		if (main != null)
		{
			return main;
		}
		return null;
	}
}
