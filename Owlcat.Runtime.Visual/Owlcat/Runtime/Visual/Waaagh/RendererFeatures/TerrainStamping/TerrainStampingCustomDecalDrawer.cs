using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Terrain;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping;

internal sealed class TerrainStampingCustomDecalDrawer : ICustomDecalDrawer, IDisposable
{
	private static class Profiling
	{
		public static readonly ProfilingSampler TerrainStamping = new ProfilingSampler("TerrainStamping");

		public static readonly ProfilingSampler FillMask = new ProfilingSampler("FillMask");

		public static readonly ProfilingSampler ClearMask = new ProfilingSampler("ClearMask");

		public static readonly ProfilingSampler StampingDecals = new ProfilingSampler("StampingDecals");
	}

	private class PassData
	{
		public readonly RendererListData RendererListData = new RendererListData();

		public bool TransitionActive;
	}

	private class RendererListData
	{
		private readonly FilteringSettings m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque)
		{
			batchLayerMask = 4294967283u
		};

		private readonly ShaderTagId[] m_ShaderTags = new ShaderTagId[1]
		{
			new ShaderTagId("TerrainStampingDecalDeferred")
		};

		public RendererList RendererList;

		public void Init(ScriptableRenderContext context, ContextContainer frameData)
		{
			WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
			DrawingSettings drawSettings = CreateDrawingSettings(m_ShaderTags, frameData, SortingCriteria.RenderQueue);
			RendererListParams param = new RendererListParams(waaaghRenderingData.CullResults, drawSettings, m_FilteringSettings);
			RendererList = context.CreateRendererList(ref param);
		}

		private static DrawingSettings CreateDrawingSettings(ShaderTagId[] shaderTags, ContextContainer frameData, SortingCriteria sortingCriteria)
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

		public void Reset()
		{
			RendererList = default(RendererList);
		}
	}

	private readonly TerrainStampingManagerParameters m_Parameters;

	private readonly PassData m_PassData = new PassData();

	private readonly Material m_StencilMaskMaterial;

	public TerrainStampingCustomDecalDrawer(TerrainStampingManagerParameters parameters)
	{
		m_Parameters = parameters;
		m_StencilMaskMaterial = CoreUtils.CreateEngineMaterial(m_Parameters.StencilMaskShader);
	}

	public bool CanBeCulled()
	{
		return false;
	}

	public bool PreventParentPassCulling(ScriptableRenderContext context)
	{
		return context.QueryRendererListStatus(m_PassData.RendererListData.RendererList) == RendererListStatus.kRendererListPopulated;
	}

	public void Draw(CommandBuffer cmd, ScriptableRenderContext context, CustomDecalSubset subset)
	{
		if (subset != m_Parameters.DecalSubset || context.QueryRendererListStatus(m_PassData.RendererListData.RendererList) != RendererListStatus.kRendererListPopulated)
		{
			return;
		}
		using (new ProfilingScope(cmd, Profiling.TerrainStamping))
		{
			if (m_PassData.TransitionActive && m_Parameters.TransitionBlendDithering)
			{
				cmd.EnableShaderKeyword("_TERRAIN_TRANSITION_BLEND_DITHERING");
			}
			else
			{
				cmd.DisableShaderKeyword("_TERRAIN_TRANSITION_BLEND_DITHERING");
			}
			StampingPass(cmd, m_PassData.RendererListData);
		}
	}

	public void ConfigureRendererLists(ScriptableRenderContext context, ContextContainer frameData, List<RendererList> outRendererLists)
	{
		m_PassData.RendererListData.Init(context, frameData);
		outRendererLists.Add(m_PassData.RendererListData.RendererList);
		m_PassData.TransitionActive = OwlcatTerrainTransition.Active;
	}

	public void Dispose()
	{
		CoreUtils.Destroy(m_StencilMaskMaterial);
	}

	private void StampingPass(CommandBuffer cmd, RendererListData rendererListData)
	{
		using (new ProfilingScope(cmd, Profiling.FillMask))
		{
			DrawStencilMaskPass(cmd, 0);
		}
		using (new ProfilingScope(cmd, Profiling.StampingDecals))
		{
			cmd.DrawRendererList(rendererListData.RendererList);
		}
		using (new ProfilingScope(cmd, Profiling.ClearMask))
		{
			DrawStencilMaskPass(cmd, 1);
		}
	}

	private void DrawStencilMaskPass(CommandBuffer cmd, int shaderPass)
	{
		cmd.DrawProcedural(Matrix4x4.identity, m_StencilMaskMaterial, shaderPass, MeshTopology.Triangles, 3);
	}
}
