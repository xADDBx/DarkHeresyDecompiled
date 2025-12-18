using System;
using Owlcat.Runtime.Visual.XPBD.Layouts.MeshSkinning;

namespace Owlcat.Runtime.Visual.XPBD.Authoring;

[Serializable]
public class MeshDeformerBinding
{
	public MeshBodyBase Master;

	public TriangleSkinmap Skinmap;
}
