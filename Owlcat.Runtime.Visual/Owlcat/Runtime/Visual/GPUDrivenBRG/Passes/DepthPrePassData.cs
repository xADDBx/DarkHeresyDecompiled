using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;

public sealed class DepthPrePassData : DrawMultiRendererListPassData
{
	public CameraType CameraType;

	public bool IsIndirectRenderingEnabled;

	public bool IsSceneViewInPrefabEditMode;
}
