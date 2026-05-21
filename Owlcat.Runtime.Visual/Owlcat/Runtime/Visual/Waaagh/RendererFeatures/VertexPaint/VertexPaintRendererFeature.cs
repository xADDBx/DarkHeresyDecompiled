using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VertexPaint;

internal sealed class VertexPaintRendererFeature : IRendererFeature, IDisposable
{
	private sealed class PassData
	{
		public readonly List<VertexPaintManager.DataSourceAllocation> DirtyDataSources = new List<VertexPaintManager.DataSourceAllocation>();

		public BufferHandle Buffer;
	}

	private readonly VertexPaintRendererFeatureAsset m_Asset;

	private readonly RendererFeaturePipelineService m_ManagerService;

	public VertexPaintRendererFeature(VertexPaintRendererFeatureAsset asset)
	{
		m_Asset = asset;
		m_ManagerService = new RendererFeaturePipelineService(delegate(WaaaghPipeline pipeline)
		{
			VertexPaintManager.Init(pipeline, m_Asset.ManagerParameters);
		}, delegate
		{
			VertexPaintManager.Cleanup();
		}, () => VertexPaintManager.IsInitialized);
		m_ManagerService.OnCreate();
	}

	public void Dispose()
	{
		m_ManagerService.OnDispose();
	}

	public void RegisterExtensions(RendererFeatureExtensionRegistry registry)
	{
		registry.AddRecordDelegate(RecordExtensionPoint.BeforeRendering, OnBeforeRendering);
	}

	private void OnBeforeRendering(in RecordContext context)
	{
		PassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("Vertex Paint", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\VertexPaint\\VertexPaintRendererFeature.cs", 45);
		passData2.DirtyDataSources.Clear();
		VertexPaintManager.GetDirtyDataAndClear(passData2.DirtyDataSources);
		passData2.Buffer = (VertexPaintManager.TryGetBuffer(out var graphicsBuffer) ? context.RenderGraph.ImportBuffer(graphicsBuffer) : default(BufferHandle));
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData passData, UnsafeGraphContext context)
		{
			if (passData.Buffer.IsValid())
			{
				foreach (VertexPaintManager.DataSourceAllocation dirtyDataSource in passData.DirtyDataSources)
				{
					VertexPaintManager.DataSourceAllocation dataSourceAllocation = dirtyDataSource;
					if (TryGetFirstContainer(in dataSourceAllocation, out var result))
					{
						VertexColorContainer.RawColorsData rawColorsData = result.GetRawColorsData();
						context.cmd.SetBufferData(passData.Buffer, rawColorsData.Colors, 0, (int)dataSourceAllocation.Allocation.Offset, (int)dataSourceAllocation.Allocation.Size);
					}
				}
				passData.DirtyDataSources.Clear();
			}
		});
	}

	private static bool TryGetFirstContainer(in VertexPaintManager.DataSourceAllocation dataSourceAllocation, out VertexColorContainer result)
	{
		using (HashSet<VertexColorContainer>.Enumerator enumerator = dataSourceAllocation.Usages.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				VertexColorContainer current = enumerator.Current;
				result = current;
				return true;
			}
		}
		Debug.LogError("[VERTEX_PAINT] Sent an unused data source for upload, which is not supported.");
		result = null;
		return false;
	}
}
