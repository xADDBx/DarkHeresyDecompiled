using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.VirtualTerrain.Streaming;

[CreateAssetMenu(menuName = "Renderer Features/Waaagh/Terrain Streaming Feedback")]
internal sealed class TerrainStreamingFeedbackFeature : ScriptableRendererFeature
{
	private static readonly List<IFeedbackProvider> s_FeedbackProviders = new List<IFeedbackProvider>();

	private static Vector3 s_FeedbackPosition;

	[SerializeField]
	private float m_ForwardOffset = 10f;

	public static Vector3 FeedbackPosition => s_FeedbackPosition;

	public static void RegisterFeedbackProvider(IFeedbackProvider provider)
	{
		s_FeedbackProviders.Add(provider);
	}

	public static void UnregisterFeedbackProvider(IFeedbackProvider provider)
	{
		s_FeedbackProviders.Remove(provider);
	}

	public static void GetFeedback(Span<int> layerLods)
	{
		foreach (IFeedbackProvider s_FeedbackProvider in s_FeedbackProviders)
		{
			s_FeedbackProvider.GetFeedback(s_FeedbackPosition, layerLods);
		}
	}

	public override void Create()
	{
	}

	internal override void StartSetupJobs(ContextContainer frameData)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		if (waaaghCameraData.cameraType == CameraType.Game && waaaghCameraData.targetTexture == null)
		{
			s_FeedbackPosition = GetFeedbackPosition(waaaghCameraData.camera);
		}
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ContextContainer frameData)
	{
	}

	private Vector3 GetFeedbackPosition(Camera camera)
	{
		Transform transform = camera.transform;
		Vector3 position = transform.position;
		Vector3 forward = transform.forward;
		position.y = 0f;
		forward.y = 0f;
		forward = ((forward.sqrMagnitude > 0f) ? forward.normalized : Vector3.forward);
		return position + forward * m_ForwardOffset;
	}
}
