using System;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Categorization;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.PipelineResources;

[Serializable]
[SupportedOnRenderPipeline(typeof(WaaaghPipelineAsset))]
[CategoryInfo(Name = "R: Waaagh Pipeline Runtime Resources", Order = 1000)]
public class PipelineRuntimeResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[HideInInspector]
	private int m_Version;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/Materials/UI-default.mat", SearchType.ProjectPath)]
	public Material DefaultUIMaterial;

	[ResourcePath("Shaders/Utils/Texture3DAtlas.compute", SearchType.ProjectPath)]
	public ComputeShader Texture3DAtlasCS;

	[Header("Virtual Texture Resources")]
	[ResourcePath("Shaders/VirtualTexture/DrawPageTable.shader", SearchType.ProjectPath)]
	public Shader VTDrawPageTablePS;

	[ResourcePath("Shaders/VirtualTexture/VTFeedback.compute", SearchType.ProjectPath)]
	public ComputeShader VTFeedbackCS;

	[ResourcePath("Shaders/VirtualTexture/VTCopyTile.compute", SearchType.ProjectPath)]
	public ComputeShader VTCopyTileCS;

	[Header("GPU Driven Render Resources")]
	[ResourcePath("Runtime/GPUDrivenBRG/GPUDrivenRawBufferClear.compute", SearchType.ProjectPath)]
	public ComputeShader RawBufferClearCS;

	[ResourcePath("Runtime/GPUDrivenBRG/GPUDrivenRawBufferCopy.compute", SearchType.ProjectPath)]
	public ComputeShader RawBufferCopyCS;

	[ResourcePath("Runtime/GPUDrivenBRG/GPUDrivenDataUpload.compute", SearchType.ProjectPath)]
	public ComputeShader DataUploadCS;

	[ResourcePath("Runtime/GPUDrivenBRG/GPUDrivenUpdateInstancesCreatedThisFrame.compute", SearchType.ProjectPath)]
	public ComputeShader UpdateInstancesCreatedThisFrameCS;

	[ResourcePath("Runtime/GPUDrivenBRG/GPUDrivenBRGCulling.compute", SearchType.ProjectPath)]
	public ComputeShader CullingCS;

	[ResourcePath("Runtime/GPUDrivenBRG/GPUDrivenBRGFixupIndirectArgs.compute", SearchType.ProjectPath)]
	public ComputeShader FixupIndirectArgsCS;

	[ResourcePath("Runtime/GPUDrivenBRG/GPUDrivenBRGFindForwardReflectionProbes.compute", SearchType.ProjectPath)]
	public ComputeShader FindForwardReflectionProbesCS;

	[ResourcePath("Shaders/Utils/BRGPicking.shader", SearchType.ProjectPath)]
	public Shader BRGPickingPS;

	[ResourcePath("Runtime/GPUDrivenBRG/DOTS-BRG-Internal-ErrorShader.shader", SearchType.ProjectPath)]
	public Shader DOTSBRGErrorShaderPS;

	[ResourcePath("Runtime/GPUDrivenBRG/GPUDrivenBRGDepthReprojection.compute", SearchType.ProjectPath)]
	public ComputeShader DepthReprojectionCS;

	public int version => m_Version;

	bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;
}
