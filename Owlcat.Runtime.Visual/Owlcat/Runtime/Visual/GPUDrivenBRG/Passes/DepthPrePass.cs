using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.IndirectRendering;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;

public sealed class DepthPrePass : DrawMultiRendererListPass<DepthPrePassData>, IDisposable
{
	private static readonly ShaderTagId[] s_PassNames = new ShaderTagId[1]
	{
		new ShaderTagId("DepthOnly")
	};

	private readonly GBufferType m_GBufferType;

	public override string Name { get; }

	public DepthPrePass(RenderPassEvent evt, GBufferType gBufferType)
		: base(evt)
	{
		Name = "DepthPrePass." + gBufferType;
		m_GBufferType = gBufferType;
	}

	public void Dispose()
	{
	}

	protected override void GetOrCreateRendererLists(ScriptableRenderContext context, ContextContainer frameData, List<DrawMultiRendererListPassData.RendererListData> rendererLists)
	{
		WaaaghRendererListData waaaghRendererListData = frameData.Get<WaaaghRendererListData>();
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		List<(WaaaghRendererList, WaaaghProfileId)> value;
		using (ListPool<(WaaaghRendererList, WaaaghProfileId)>.Get(out value))
		{
			GBufferType gBufferType = m_GBufferType;
			if (gBufferType != 0 && gBufferType == GBufferType.OpaqueDistortion)
			{
				value.Add((waaaghRendererListData.OpaqueDistortionGBuffer, WaaaghProfileId.DepthPrePass_OpaqueDistortion));
			}
			else
			{
				value.Add((waaaghRendererListData.OpaqueGBuffer, WaaaghProfileId.DepthPrePass_OpaqueBase));
				value.Add((waaaghRendererListData.OpaqueAlphaTestGBuffer, WaaaghProfileId.DepthPrePass_OpaqueAlphaTest));
			}
			foreach (var item in value)
			{
				RendererListParams param = CreateRendererListDesc(item.Item1.ListParams, in waaaghRenderingData.CullResults, waaaghCameraData.camera);
				rendererLists.Add(new DrawMultiRendererListPassData.RendererListData
				{
					List = context.CreateRendererList(ref param),
					ListParams = param,
					ProfileId = item.Item2
				});
			}
		}
		static RendererListParams CreateRendererListDesc(RendererListParams sourceParams, in CullingResults cullingResults, Camera camera)
		{
			RendererListParams result = RenderingUtils.CreateRendererListParams(cullingResults, camera, s_PassNames, renderQueueRange: sourceParams.filteringSettings.renderQueueRange, layerMask: sourceParams.filteringSettings.layerMask, rendererConfiguration: PerObjectData.None, sortingCriteria: sourceParams.drawSettings.sortingSettings.criteria);
			result.filteringSettings.batchLayerMask = 5u;
			return result;
		}
	}

	protected override void Setup(RenderGraphBuilder builder, DepthPrePassData data, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		TextureHandle input = waaaghResourceData.CameraDepthBuffer;
		builder.UseDepthBuffer(in input, DepthAccess.ReadWrite);
		data.CameraType = waaaghCameraData.cameraType;
		data.IsIndirectRenderingEnabled = waaaghCameraData.IrsData.Enabled;
		data.IsSceneViewInPrefabEditMode = waaaghCameraData.IsSceneViewInPrefabEditMode;
		if (m_GBufferType == GBufferType.Opaque)
		{
			builder.AllowRendererListCulling(!waaaghCameraData.IrsData.IrsHasOpaques);
		}
		else
		{
			builder.AllowRendererListCulling(!waaaghCameraData.IrsData.IrsHasOpaqueDistortions);
		}
	}

	protected override void Render(DepthPrePassData data, RenderGraphContext context)
	{
		foreach (DrawMultiRendererListPassData.RendererListData rendererList in data.RendererLists)
		{
			using (new ProfilingScope(context.cmd, ProfilingSamplerStorage<WaaaghProfileId>.Get(Name, rendererList.ProfileId)))
			{
				DrawRendererList(context.cmd, rendererList.List);
				IndirectRenderingSystem.Instance.DrawPass(context.cmd, data.CameraType, data.IsIndirectRenderingEnabled, data.IsSceneViewInPrefabEditMode, rendererList.ListParams, debugOverdraw: false);
			}
		}
	}

	private static void DrawRendererList(CommandBuffer cmd, RendererList rendererList)
	{
		if (!rendererList.isValid)
		{
			throw new ArgumentException("Invalid renderer list provided to DrawRendererList");
		}
		cmd.DrawRendererList(rendererList);
	}
}
