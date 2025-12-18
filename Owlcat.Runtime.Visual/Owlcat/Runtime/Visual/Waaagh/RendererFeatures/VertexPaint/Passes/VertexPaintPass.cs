using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VertexPaint.Passes;

public class VertexPaintPass : ScriptableRenderPass<VertexPaintPassData>
{
	public override string Name => "VertexPaintPass";

	public VertexPaintPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, VertexPaintPassData data, ContextContainer frameData)
	{
		RenderGraph renderGraph = frameData.Get<WaaaghRenderingData>().RenderGraph;
		data.DirtyDataSources.Clear();
		VertexPaintManager.GetDirtyDataAndClear(data.DirtyDataSources);
		data.Buffer = (VertexPaintManager.TryGetBuffer(out var graphicsBuffer) ? renderGraph.ImportBuffer(graphicsBuffer) : default(BufferHandle));
	}

	protected override void Render(VertexPaintPassData data, RenderGraphContext context)
	{
		if (!data.Buffer.IsValid())
		{
			return;
		}
		foreach (VertexPaintManager.DataSourceAllocation dirtyDataSource in data.DirtyDataSources)
		{
			VertexPaintManager.DataSourceAllocation dataSourceAllocation = dirtyDataSource;
			VertexColorContainer.RawColorsData rawColorsData = GetFirstContainer(in dataSourceAllocation).GetRawColorsData();
			context.cmd.SetBufferData(data.Buffer, rawColorsData.Colors, 0, (int)dataSourceAllocation.Allocation.Offset, (int)dataSourceAllocation.Allocation.Size);
		}
		data.DirtyDataSources.Clear();
	}

	private VertexColorContainer GetFirstContainer(in VertexPaintManager.DataSourceAllocation dataSourceAllocation)
	{
		using (HashSet<VertexColorContainer>.Enumerator enumerator = dataSourceAllocation.Usages.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		throw new InvalidOperationException("Sent an unused data source for upload, which is not supported.");
	}
}
