using JetBrains.Annotations;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping.Passes;

public class TerrainStampingChunkUpdatePass : ScriptableRenderPass<TerrainStampingChunkUpdatePassData>
{
	private readonly TerrainStampingManagerParameters m_Parameters;

	public override string Name => "TerrainStampingChunkUpdatePass";

	public TerrainStampingChunkUpdatePass(TerrainStampingManagerParameters parameters, RenderPassEvent evt)
		: base(evt)
	{
		m_Parameters = parameters;
	}

	protected override void Setup(RenderGraphBuilder builder, TerrainStampingChunkUpdatePassData data, ContextContainer frameData)
	{
		WaaaghCameraData cameraData = frameData.Get<WaaaghCameraData>();
		if (!IsRendererEligible(cameraData))
		{
			return;
		}
		Camera mainCamera = GetMainCamera(cameraData);
		if (mainCamera == null)
		{
			return;
		}
		TerrainStampingManager.TryGetInstance(out var terrainStampingManager);
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
		float num = math.min(m_Parameters.ChunkAllocationMaxDistance, mainCamera.farClipPlane);
		foreach (Ray item in nativeArray2)
		{
			if (!plane.Raycast(item, out var enter))
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
			builder.AllowPassCulling(value: false);
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

	private static bool IsRendererEligible(WaaaghCameraData cameraData)
	{
		foreach (ScriptableRendererFeature rendererFeature in cameraData.renderer.RendererFeatures)
		{
			if (rendererFeature is TerrainStampingFeature)
			{
				return true;
			}
		}
		return false;
	}

	protected override void Render(TerrainStampingChunkUpdatePassData data, RenderGraphContext context)
	{
	}
}
