using System;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Categorization;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.HBAO;

[Serializable]
[SupportedOnRenderPipeline(typeof(WaaaghPipelineAsset))]
[CategoryInfo(Name = "HBAO Runtime Resources", Order = 1000)]
public class HBAOFeatureResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[HideInInspector]
	private int m_Version;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/RendererFeatures/HBAO/Shaders/HBAO.shader", SearchType.ProjectPath)]
	private Shader m_HbaoPS;

	public int version => m_Version;

	bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;

	public Shader HbaoPS
	{
		get
		{
			return m_HbaoPS;
		}
		set
		{
			this.SetValueAndNotify(ref m_HbaoPS, value, "m_HbaoPS");
		}
	}
}
