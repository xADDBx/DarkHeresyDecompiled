using System;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Data;

[Serializable]
[ExcludeFromPreset]
public class WaaaghRendererData : ScriptableRendererData, ISerializationCallbackReceiver
{
	private RenderRuntimeShaders m_Shaders;

	private PostProcessResources m_PostProcessResources;

	[SerializeField]
	private TileSize m_TileSize = TileSize.Tile16;

	[SerializeField]
	private DeferredLightingMode m_DeferredLightingMode;

	[SerializeField]
	private DeferredReflectionsBatchingMode m_DeferredReflectionsBatchingMode = DeferredReflectionsBatchingMode.On;

	[SerializeField]
	private DepthPyramidGenerationAlgorithm m_DepthPyramidGenerationAlgorithm = DepthPyramidGenerationAlgorithm.AMDFidelityFX;

	[SerializeField]
	private UISubset m_OverlayUIMask = UISubset.All;

	[SerializeField]
	private bool m_DrawViaGPUDrivenBRG = true;

	public RenderRuntimeShaders Shaders => m_Shaders;

	public PostProcessResources PostProcessResources => m_PostProcessResources;

	public TileSize TileSize => m_TileSize;

	public DeferredLightingMode DeferredLightingMode
	{
		get
		{
			return m_DeferredLightingMode;
		}
		set
		{
			m_DeferredLightingMode = value;
		}
	}

	public DeferredReflectionsBatchingMode DeferredReflectionsBatchingMode
	{
		get
		{
			return m_DeferredReflectionsBatchingMode;
		}
		set
		{
			m_DeferredReflectionsBatchingMode = value;
		}
	}

	public DepthPyramidGenerationAlgorithm DepthPyramidGenerationAlgorithm
	{
		get
		{
			return m_DepthPyramidGenerationAlgorithm;
		}
		set
		{
			m_DepthPyramidGenerationAlgorithm = value;
		}
	}

	public UISubset OverlayUIMask => m_OverlayUIMask;

	public bool DrawViaGPUDrivenBRG
	{
		get
		{
			return m_DrawViaGPUDrivenBRG;
		}
		set
		{
			m_DrawViaGPUDrivenBRG = value;
		}
	}

	protected override ScriptableRenderer Create()
	{
		m_Shaders = GraphicsSettings.GetRenderPipelineSettings<RenderRuntimeShaders>();
		m_PostProcessResources = new PostProcessResources();
		return new WaaaghRenderer(this);
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
	}
}
