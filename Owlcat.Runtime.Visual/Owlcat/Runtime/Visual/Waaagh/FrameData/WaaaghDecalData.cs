using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.FrameData;

public sealed class WaaaghDecalData : ContextItem
{
	public List<ICustomDecalDrawer> DecalRenderers { get; } = new List<ICustomDecalDrawer>();


	public override void Reset()
	{
		DecalRenderers.Clear();
	}
}
