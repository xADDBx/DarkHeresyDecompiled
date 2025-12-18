using System;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Categorization;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.PipelineResources;

[Serializable]
[SupportedOnRenderPipeline(typeof(WaaaghPipelineAsset))]
[CategoryInfo(Name = "R: Debug Runtime Resources", Order = 1000)]
public class DebugResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[HideInInspector]
	private int m_Version = 1;

	[ResourcePath("Runtime/Waaagh/Debugging/DebugFullscreen.shader", SearchType.ProjectPath)]
	public Shader DebugFullscreenPS;

	[ResourcePath("Runtime/Waaagh/Debugging/ShadowsDebug.shader", SearchType.ProjectPath)]
	public Shader ShadowsDebugPS;

	[ResourcePath("Runtime/Waaagh/Debugging/ShowLightSortingCurve.shader", SearchType.ProjectPath)]
	public Shader ShowLightSortingCurvePS;

	[ResourcePath("Runtime/Waaagh/Debugging/ClearDebugBuffers.compute", SearchType.ProjectPath)]
	public ComputeShader ClearDebugBuffersCS;

	[ResourcePath("Runtime/Waaagh/Debugging/DebugOverdraw.shader", SearchType.ProjectPath)]
	public Shader DebugOverdrawPS;

	[ResourcePath("Runtime/Waaagh/Debugging/VirtualTextureDebug.shader", SearchType.ProjectPath)]
	public Shader VirtualTextureDebugPS;

	[ResourcePath("Runtime/Waaagh/Debugging/GPUDrivenDebug.shader", SearchType.ProjectPath)]
	public Shader GPUDrivenDebugPS;

	[ResourcePath("Runtime/Waaagh/Debugging/ProbeVolumeSamplingDebugPositionNormal.compute", SearchType.ProjectPath)]
	public ComputeShader ProbeVolumeSamplingDebugCS;

	public int version => m_Version;

	bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;
}
