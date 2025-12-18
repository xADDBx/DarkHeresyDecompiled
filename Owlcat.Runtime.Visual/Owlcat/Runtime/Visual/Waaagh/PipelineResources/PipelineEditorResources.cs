using System;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Categorization;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.PipelineResources;

[Serializable]
[SupportedOnRenderPipeline(typeof(WaaaghPipelineAsset))]
[CategoryInfo(Name = "R: Waaagh Pipeline Editor Resources", Order = 1000)]
public class PipelineEditorResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[HideInInspector]
	private int m_Version = 1;

	[ResourcePath("Runtime/Waaagh/Materials/Lit-default.mat", SearchType.ProjectPath)]
	public Material Lit;

	[ResourcePath("Runtime/Waaagh/Materials/Particles-default.mat", SearchType.ProjectPath)]
	public Material Particles;

	[ResourcePath("Runtime/Waaagh/Materials/Terrain-default.mat", SearchType.ProjectPath)]
	public Material Terrain;

	[ResourcePath("Runtime/Waaagh/Materials/Decal-default.mat", SearchType.ProjectPath)]
	public Material Decal;

	[ResourcePath("Runtime/Waaagh/Textures/DefaultPreviewReflectionProbe.exr", SearchType.ProjectPath)]
	public Cubemap DefaultPreviewReflectionProbe;

	public Vector4 DefaultPreviewReflectionProbeHDRDecodeValues = new Vector4(1f, 1f, 0f, 0f);

	public int version => m_Version;
}
