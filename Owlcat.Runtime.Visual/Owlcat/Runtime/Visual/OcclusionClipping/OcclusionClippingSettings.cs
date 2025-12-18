using System;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Categorization;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

[Serializable]
[SupportedOnRenderPipeline(typeof(WaaaghPipelineAsset))]
[CategoryInfo(Name = "Occlusion Clipping Settings", Order = 0)]
public sealed class OcclusionClippingSettings : IRenderPipelineGraphicsSettings
{
	public bool Enabled;

	public OcclusionClippingType ClippingType;

	public OcclusionClippingShadowType ShadowClippingType;

	public float RendererFadeAnimationDuration = 0.5f;

	[Min(0f)]
	public float TriggerUpdateInterval = 0.5f;

	[Min(0f)]
	public float ActiveProbeExpansion = 1f;

	public int version => 2;

	public bool isAvailableInPlayerBuild => true;
}
