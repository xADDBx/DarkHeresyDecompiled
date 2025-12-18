using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.FrameData;

public struct WaaaghRendererList
{
	public RendererListParams ListParams;

	public RendererList List;

	public void Reset()
	{
		ListParams = default(RendererListParams);
		List = default(RendererList);
	}
}
