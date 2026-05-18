using System;
using Owlcat.Runtime.Visual.OcclusionGeometryClip;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CameraObjectClip;

[CreateAssetMenu(menuName = "Renderer Features/Camera Object Clip Renderer Feature")]
public sealed class CameraObjectClipRendererFeatureAsset : RendererFeatureAsset
{
	[Serializable]
	public sealed class DepthClipSettings
	{
		public CameraType CameraTypes = CameraType.Game;

		[Range(0f, 1f)]
		public float ClipTreshold = 0.1f;

		public float NoiseTiling = 1f;

		public float AlphaScale = 1f;

		public float NearCameraClipDistance = 3f;

		public bool OcclusionGeometryClipEnabled;

		public Owlcat.Runtime.Visual.OcclusionGeometryClip.Settings OcclusionGeometryClipSettings = Owlcat.Runtime.Visual.OcclusionGeometryClip.Settings.Default;
	}

	[SerializeField]
	private DepthClipSettings m_DepthClipSettings = new DepthClipSettings();

	public DepthClipSettings Settings => m_DepthClipSettings;

	public override IRendererFeature CreateRendererFeature()
	{
		return new CameraObjectClipRendererFeature(this);
	}
}
