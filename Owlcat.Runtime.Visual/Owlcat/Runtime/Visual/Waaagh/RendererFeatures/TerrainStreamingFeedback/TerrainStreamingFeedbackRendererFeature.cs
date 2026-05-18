using System;
using Owlcat.Runtime.Visual.VirtualTerrain.Streaming;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStreamingFeedback;

internal sealed class TerrainStreamingFeedbackRendererFeature : IRendererFeature, IDisposable
{
	private readonly TerrainStreamingFeedbackRendererFeatureAsset m_Asset;

	public TerrainStreamingFeedbackRendererFeature(TerrainStreamingFeedbackRendererFeatureAsset asset)
	{
		m_Asset = asset;
	}

	public void Dispose()
	{
	}

	public void RegisterExtensions(RendererFeatureExtensionRegistry registry)
	{
		registry.AddSetupDelegate(OnSetup);
	}

	private void OnSetup(in SetupContext context)
	{
		if (ShouldUpdateFeedbackPosition(context.CameraData))
		{
			Owlcat.Runtime.Visual.VirtualTerrain.Streaming.TerrainStreamingFeedback.FeedbackPosition = GetFeedbackPosition(context.CameraData.camera);
		}
	}

	private static bool ShouldUpdateFeedbackPosition(WaaaghCameraData cameraData)
	{
		if (cameraData.cameraType == CameraType.Game)
		{
			return cameraData.targetTexture == null;
		}
		return false;
	}

	private Vector3 GetFeedbackPosition(Camera camera)
	{
		Transform transform = camera.transform;
		Vector3 position = transform.position;
		Vector3 forward = transform.forward;
		position.y = 0f;
		forward.y = 0f;
		forward = ((forward.sqrMagnitude > 0f) ? forward.normalized : Vector3.forward);
		return position + forward * m_Asset.ForwardOffset;
	}
}
