using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DrawMultiRendererListPassData : PassDataBase
{
	public struct RendererListData
	{
		public RendererList List;

		public RendererListParams ListParams;

		public WaaaghProfileId? ProfileId;

		public RendererListData(in WaaaghRendererList waaaghRendererList, WaaaghProfileId? profileId = null)
		{
			List = waaaghRendererList.List;
			ListParams = waaaghRendererList.ListParams;
			ProfileId = profileId;
		}
	}

	public readonly List<RendererListData> RendererLists = new List<RendererListData>();
}
