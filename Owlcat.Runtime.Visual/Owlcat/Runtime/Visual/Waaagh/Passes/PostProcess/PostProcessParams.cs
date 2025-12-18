using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.PostProcess;

public struct PostProcessParams
{
	public Material BlitMaterial;

	public GraphicsFormat RequestColorFormat;

	public static PostProcessParams Create()
	{
		PostProcessParams result = default(PostProcessParams);
		result.BlitMaterial = null;
		result.RequestColorFormat = GraphicsFormat.None;
		return result;
	}
}
