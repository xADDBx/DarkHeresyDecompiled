using System;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine.Categorization;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.IndirectRendering;

[Serializable]
[SupportedOnRenderPipeline(typeof(WaaaghPipelineAsset))]
[CategoryInfo(Name = "Indirect Rendering System Settings", Order = 4)]
public sealed class IndirectRenderingSystemSettings : IRenderPipelineGraphicsSettings
{
	public bool ShapeCullingEnabled;

	public bool DistanceCullingEnabled;

	public float CullingDistance = 100f;

	public int version => 1;

	public bool isAvailableInPlayerBuild => true;
}
