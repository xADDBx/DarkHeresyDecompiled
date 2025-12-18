using System.Collections.Generic;
using Owlcat.Runtime.Visual.VirtualTexture;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DrawDecalsPass : DrawRendererListPass<DrawDecalsPassData>
{
	private const int kUnpackGBufferPass = 0;

	private const int kPackGBufferPass = 1;

	private Material m_DBufferBlitMaterial;

	private bool m_DrawGUIDecals;

	private ShaderTagId[] m_ShaderTags;

	private FilteringSettings m_OpaqueFilterSettings;

	private RendererList m_OpaqueGBufferList;

	private RendererList m_OpaqueAlphaTestGBufferList;

	private RendererList m_TerrainGBufferList;

	private RendererList m_DecalsList;

	public override string Name => "DrawDecalsPass";

	private protected override WaaaghProfileId? ProfileId => WaaaghProfileId.DrawDecalsPass;

	public DrawDecalsPass(RenderPassEvent evt, Material dBufferBlitMaterial, bool drawGUIDecals)
		: base(evt)
	{
		m_DBufferBlitMaterial = dBufferBlitMaterial;
		m_DrawGUIDecals = drawGUIDecals;
		if (drawGUIDecals)
		{
			m_ShaderTags = new ShaderTagId[2]
			{
				new ShaderTagId("DecalGUI"),
				new ShaderTagId("DecalForwardOverlay")
			};
		}
		else
		{
			m_ShaderTags = new ShaderTagId[1]
			{
				new ShaderTagId("DecalDeferred")
			};
		}
		m_OpaqueFilterSettings = new FilteringSettings(RenderQueueRange.opaque);
		m_OpaqueFilterSettings.batchLayerMask = 4294967283u;
	}

	public override void ConfigureRendererLists(ScriptableRenderContext context, ContextContainer frameData)
	{
		base.ConfigureRendererLists(context, frameData);
		List<ICustomDecalDrawer> value;
		using (ListPool<ICustomDecalDrawer>.Get(out value))
		{
			List<RendererList> value2;
			using (ListPool<RendererList>.Get(out value2))
			{
				CollectCustomDecalDrawers(frameData, value);
				foreach (ICustomDecalDrawer item in value)
				{
					item.ConfigureRendererLists(context, frameData, value2);
				}
				foreach (RendererList item2 in value2)
				{
					RendererList rendererList = item2;
					DependsOn(in rendererList);
				}
			}
		}
	}

	private void CollectCustomDecalDrawers(ContextContainer frameData, List<ICustomDecalDrawer> drawers)
	{
		if (!m_DrawGUIDecals && frameData.Contains<WaaaghDecalData>())
		{
			WaaaghDecalData waaaghDecalData = frameData.Get<WaaaghDecalData>();
			drawers.AddRange(waaaghDecalData.DecalRenderers);
		}
	}

	protected override void GetOrCreateRendererList(ScriptableRenderContext context, ContextContainer frameData, out RendererList rendererList, out RendererListParams rendererListParams)
	{
		WaaaghRendererListData waaaghRendererListData = frameData.Get<WaaaghRendererListData>();
		DependsOn(in waaaghRendererListData.OpaqueGBuffer.List);
		DependsOn(in waaaghRendererListData.OpaqueAlphaTestGBuffer.List);
		DependsOn(in waaaghRendererListData.TerrainGBuffer.List);
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		DrawingSettings drawSettings = CreateDrawingSettings(m_ShaderTags, frameData, SortingCriteria.SortingLayer | SortingCriteria.RenderQueue);
		rendererListParams = new RendererListParams(waaaghRenderingData.CullResults, drawSettings, m_OpaqueFilterSettings);
		rendererList = context.CreateRendererList(ref rendererListParams);
		m_OpaqueGBufferList = waaaghRendererListData.OpaqueGBuffer.List;
		m_OpaqueAlphaTestGBufferList = waaaghRendererListData.OpaqueAlphaTestGBuffer.List;
		m_TerrainGBufferList = waaaghRendererListData.TerrainGBuffer.List;
		m_DecalsList = rendererList;
	}

	public override bool HasAnyCustomDependencyThatPreventsPassCulling(ScriptableRenderContext context, ContextContainer frameData)
	{
		if (frameData.Contains<WaaaghDecalData>())
		{
			foreach (ICustomDecalDrawer decalRenderer in frameData.Get<WaaaghDecalData>().DecalRenderers)
			{
				if (decalRenderer.PreventParentPassCulling(context))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool AreRendererListsEmpty(ScriptableRenderContext context)
	{
		if (context.QueryRendererListStatus(m_DecalsList) != RendererListStatus.kRendererListPopulated)
		{
			return true;
		}
		if (context.QueryRendererListStatus(m_OpaqueGBufferList) == RendererListStatus.kRendererListPopulated)
		{
			return false;
		}
		if (context.QueryRendererListStatus(m_OpaqueAlphaTestGBufferList) == RendererListStatus.kRendererListPopulated)
		{
			return false;
		}
		if (context.QueryRendererListStatus(m_TerrainGBufferList) == RendererListStatus.kRendererListPopulated)
		{
			return false;
		}
		return true;
	}

	protected override void Setup(RenderGraphBuilder builder, DrawDecalsPassData data, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		frameData.Get<WaaaghRenderingData>();
		data.DrawGUIDecals = m_DrawGUIDecals;
		TextureHandle input = waaaghResourceData.CameraDepthBuffer;
		data.CameraDepthRT = builder.UseDepthBuffer(in input, DepthAccess.Read);
		data.CameraDepthCopyRT = builder.ReadTexture(in waaaghResourceData.CameraDepthCopyRT);
		data.CameraNormalsRT = builder.ReadWriteTexture(in waaaghResourceData.CameraNormalsRT);
		data.CameraSpecularRT = builder.ReadWriteTexture(in waaaghResourceData.CameraSpecularRT);
		data.CameraTranslucencyRT = builder.ReadWriteTexture(in waaaghResourceData.CameraTranslucencyRT);
		input = waaaghResourceData.VTFeedbackRT;
		data.VTFeedbackRT = builder.WriteTexture(in input);
		data.CustomDecalDrawer.Clear();
		if (m_DrawGUIDecals)
		{
			input = waaaghResourceData.CameraColorBuffer;
			data.CameraColorRT = builder.UseColorBuffer(in input, 0);
			return;
		}
		data.DBufferBlitMaterial = m_DBufferBlitMaterial;
		TextureDesc baseDesc2 = RenderingUtils.CreateTextureDesc(null, waaaghCameraData.cameraTargetDescriptor);
		baseDesc2.filterMode = FilterMode.Bilinear;
		baseDesc2.wrapMode = TextureWrapMode.Clamp;
		TextureDesc desc = DBufferDesc(baseDesc2, GraphicsFormat.R8G8B8A8_UNorm, "DBuffer0RT");
		data.DBuffer0RT = builder.CreateTransientTexture(in desc);
		desc = DBufferDesc(baseDesc2, GraphicsFormat.R16G16B16A16_SFloat, "DBuffer1RT");
		data.DBuffer1RT = builder.CreateTransientTexture(in desc);
		data.CameraAlbedoRT = waaaghResourceData.CameraAlbedoRT;
		data.CameraEmissionRT = waaaghResourceData.CameraColorBuffer;
		data.ClearColor = new Color(0f, 0f, 0f, 0f);
		if (!frameData.Contains<WaaaghDecalData>())
		{
			return;
		}
		foreach (ICustomDecalDrawer decalRenderer in frameData.Get<WaaaghDecalData>().DecalRenderers)
		{
			if (!decalRenderer.CanBeCulled())
			{
				data.CustomDecalDrawer.Add(decalRenderer);
			}
		}
		static TextureDesc DBufferDesc(TextureDesc baseDesc, GraphicsFormat format, string name)
		{
			TextureDesc result = baseDesc;
			result.colorFormat = format;
			result.name = name;
			return result;
		}
	}

	protected override void Render(DrawDecalsPassData data, RenderGraphContext context)
	{
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDepthRT, data.CameraDepthCopyRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraNormalsRT, data.CameraNormalsRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraSpecularRT, data.CameraSpecularRT);
		VirtualTextureManager.SetFeedbackBufferRandomWriteTarget(context.cmd, data.VTFeedbackRT);
		if (data.DrawGUIDecals)
		{
			context.cmd.DrawRendererList(data.RendererList);
		}
		else
		{
			RenderTargetIdentifier[] tempArray = context.renderGraphPool.GetTempArray<RenderTargetIdentifier>(2);
			tempArray[0] = data.DBuffer0RT;
			tempArray[1] = data.DBuffer1RT;
			context.cmd.SetRenderTarget(tempArray, data.CameraDepthRT);
			context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, data.ClearColor);
			context.cmd.DrawProcedural(Matrix4x4.identity, data.DBufferBlitMaterial, 0, MeshTopology.Triangles, 3);
			RenderTargetIdentifier[] tempArray2 = context.renderGraphPool.GetTempArray<RenderTargetIdentifier>(4);
			tempArray2[0] = data.CameraAlbedoRT;
			tempArray2[1] = data.DBuffer0RT;
			tempArray2[2] = data.DBuffer1RT;
			tempArray2[3] = data.CameraEmissionRT;
			context.cmd.SetRenderTarget(tempArray2, data.CameraDepthRT);
			foreach (ICustomDecalDrawer item in data.CustomDecalDrawer)
			{
				item.Draw(context.cmd, context.renderContext, CustomDecalSubset.BeforeBuiltIn);
			}
			context.cmd.DrawRendererList(data.RendererList);
			foreach (ICustomDecalDrawer item2 in data.CustomDecalDrawer)
			{
				item2.Draw(context.cmd, context.renderContext, CustomDecalSubset.AfterBuiltIn);
			}
			context.cmd.SetGlobalTexture(ShaderPropertyId._DecalsMasksRT, data.DBuffer0RT);
			context.cmd.SetGlobalTexture(ShaderPropertyId._DecalsNormalsRT, data.DBuffer1RT);
			RenderTargetIdentifier[] tempArray3 = context.renderGraphPool.GetTempArray<RenderTargetIdentifier>(3);
			tempArray3[0] = data.CameraNormalsRT;
			tempArray3[1] = data.CameraSpecularRT;
			tempArray3[2] = data.CameraTranslucencyRT;
			context.cmd.SetRenderTarget(tempArray3, data.CameraDepthRT);
			context.cmd.DrawProcedural(Matrix4x4.identity, data.DBufferBlitMaterial, 1, MeshTopology.Triangles, 3);
		}
		context.cmd.ClearRandomWriteTargets();
	}

	public DrawingSettings CreateDrawingSettings(ShaderTagId[] shaderTags, ContextContainer frameData, SortingCriteria sortingCriteria)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		Camera camera = waaaghCameraData.camera;
		SortingSettings sortingSettings = new SortingSettings(camera);
		sortingSettings.criteria = sortingCriteria;
		SortingSettings sortingSettings2 = sortingSettings;
		DrawingSettings drawingSettings = new DrawingSettings(shaderTags[0], sortingSettings2);
		drawingSettings.perObjectData = waaaghRenderingData.PerObjectData;
		drawingSettings.enableInstancing = true;
		drawingSettings.enableDynamicBatching = false;
		DrawingSettings result = drawingSettings;
		for (int i = 1; i < shaderTags.Length; i++)
		{
			result.SetShaderPassName(i, shaderTags[i]);
		}
		return result;
	}
}
