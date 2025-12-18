using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public interface ICustomDecalDrawer
{
	void ConfigureRendererLists(ScriptableRenderContext context, ContextContainer frameData, List<RendererList> outRendererLists)
	{
	}

	bool CanBeCulled();

	bool PreventParentPassCulling(ScriptableRenderContext context);

	void Draw(CommandBuffer cmd, ScriptableRenderContext context, CustomDecalSubset subset);
}
