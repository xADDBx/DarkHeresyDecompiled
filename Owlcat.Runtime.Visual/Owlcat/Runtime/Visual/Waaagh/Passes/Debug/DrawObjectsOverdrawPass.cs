using Owlcat.Runtime.Visual.IndirectRendering;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.ShaderLibrary.Visual.Debug;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug;

public class DrawObjectsOverdrawPass : DrawRendererListPass<DrawObjectsOverdrawPassData>
{
	private ShaderTagId[] m_OverdrawShaderTags = new ShaderTagId[4]
	{
		new ShaderTagId("SRPDefaultUnlit"),
		new ShaderTagId("ForwardLit"),
		new ShaderTagId("DecalDeferred"),
		new ShaderTagId("DecalGUI")
	};

	private DebugOverdrawMode m_OverdrawMode;

	private Color m_ClearColor = new Color(0f, 0f, 0f, 0f);

	private Color m_DebugColor = new Color(0.1f, 0.01f, 0.01f, 1f);

	public override string Name => "DrawObjectsOverdrawPass";

	public DebugOverdrawMode OverdrawMode
	{
		get
		{
			return m_OverdrawMode;
		}
		set
		{
			m_OverdrawMode = value;
		}
	}

	public DrawObjectsOverdrawPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void GetOrCreateRendererList(ScriptableRenderContext context, ContextContainer frameData, out RendererList rendererList, out RendererListParams rendererListParams)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghRendererListData waaaghRendererListData = frameData.Get<WaaaghRendererListData>();
		rendererListParams = RenderingUtils.CreateRendererListParams(waaaghRenderingData.CullResults, waaaghCameraData.camera, m_OverdrawShaderTags, -1, waaaghRenderingData.PerObjectData, WaaaghRenderQueue.Transparent, SortingCriteria.CommonTransparent);
		rendererListParams.filteringSettings.batchLayerMask = 4294967283u;
		switch (m_OverdrawMode)
		{
		case DebugOverdrawMode.All:
			rendererListParams.filteringSettings.renderQueueRange = WaaaghRenderQueue.All;
			break;
		case DebugOverdrawMode.TransparentOnly:
			rendererListParams.filteringSettings.renderQueueRange = WaaaghRenderQueue.Transparent;
			break;
		case DebugOverdrawMode.OpaqueOnly:
			rendererListParams.filteringSettings.renderQueueRange = new RenderQueueRange(waaaghRendererListData.OpaqueGBuffer.ListParams.filteringSettings.renderQueueRange.lowerBound, waaaghRendererListData.OpaqueAlphaTestGBuffer.ListParams.filteringSettings.renderQueueRange.upperBound);
			break;
		}
		rendererListParams.drawSettings.perObjectData = PerObjectData.None;
		NativeArray<RenderStateBlock> value = new NativeArray<RenderStateBlock>(1, Allocator.Temp);
		NativeArray<ShaderTagId> value2 = new NativeArray<ShaderTagId>(1, Allocator.Temp);
		value[0] = new RenderStateBlock
		{
			depthState = new DepthState(writeEnabled: false, CompareFunction.Always),
			blendState = new BlendState(separateMRTBlend: false, alphaToMask: false)
			{
				blendState0 = new RenderTargetBlendState
				{
					writeMask = ColorWriteMask.All,
					colorBlendOperation = BlendOp.Add,
					alphaBlendOperation = BlendOp.Add,
					sourceColorBlendMode = BlendMode.One,
					sourceAlphaBlendMode = BlendMode.One,
					destinationColorBlendMode = BlendMode.One,
					destinationAlphaBlendMode = BlendMode.Zero
				}
			},
			stencilState = new StencilState(enabled: false),
			mask = (RenderStateMask.Blend | RenderStateMask.Depth | RenderStateMask.Stencil)
		};
		value2[0] = ShaderTagId.none;
		rendererListParams.stateBlocks = value;
		rendererListParams.tagValues = value2;
		rendererList = context.CreateRendererList(ref rendererListParams);
	}

	protected override void Setup(RenderGraphBuilder builder, DrawObjectsOverdrawPassData data, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		TextureHandle input = waaaghResourceData.CameraResolveColorBuffer;
		data.RenderTarget = builder.UseColorBuffer(in input, 0);
		data.ClearColor = m_ClearColor;
		data.DebugColor = m_DebugColor;
		builder.AllowRendererListCulling(value: false);
		data.CameraType = waaaghCameraData.cameraType;
		data.IsIndirectRenderingEnabled = waaaghCameraData.IrsData.Enabled;
		data.IsSceneViewInPrefabEditMode = waaaghCameraData.IsSceneViewInPrefabEditMode;
	}

	protected override void Render(DrawObjectsOverdrawPassData data, RenderGraphContext context)
	{
		context.cmd.ClearRenderTarget(clearDepth: true, clearColor: true, data.ClearColor);
		context.cmd.SetGlobalColor(ShaderPropertyId._DebugColor, data.DebugColor);
		context.cmd.DrawRendererList(data.RendererList);
		context.renderContext.ExecuteCommandBuffer(context.cmd);
		context.cmd.Clear();
		IndirectRenderingSystem.Instance.DrawPass(context.cmd, data.CameraType, data.IsIndirectRenderingEnabled, data.IsSceneViewInPrefabEditMode, data.RendererListParams, debugOverdraw: true);
	}
}
