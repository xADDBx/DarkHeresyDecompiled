using System;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Categorization;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CameraObjectClip;

[Serializable]
[SupportedOnRenderPipeline(typeof(WaaaghPipelineAsset))]
[CategoryInfo(Name = "CameraObjectClip Runtime Resources", Order = 1000)]
public class CameraObjectClipFeatureResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[HideInInspector]
	private int m_Version;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/RendererFeatures/CameraObjectClip/Shaders/BakeNoise3D.shader", SearchType.ProjectPath)]
	public Shader m_NoiseBakeShader;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/RendererFeatures/CameraObjectClip/Shaders/OccludedObject.shader", SearchType.ProjectPath)]
	private Shader m_OccludedObjectShader;

	public int version => m_Version;

	bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;

	public Shader NoiseBakeShader
	{
		get
		{
			return m_NoiseBakeShader;
		}
		set
		{
			this.SetValueAndNotify(ref m_NoiseBakeShader, value, "NoiseBakeShader");
		}
	}

	public Shader OccludedObjectShader
	{
		get
		{
			return m_OccludedObjectShader;
		}
		set
		{
			this.SetValueAndNotify(ref m_OccludedObjectShader, value, "OccludedObjectShader");
		}
	}
}
