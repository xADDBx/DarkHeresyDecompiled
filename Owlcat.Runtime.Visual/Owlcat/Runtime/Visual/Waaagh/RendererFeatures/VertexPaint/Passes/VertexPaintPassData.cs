using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VertexPaint.Passes;

public class VertexPaintPassData : PassDataBase
{
	public readonly List<VertexPaintManager.DataSourceAllocation> DirtyDataSources = new List<VertexPaintManager.DataSourceAllocation>();

	public BufferHandle Buffer;
}
