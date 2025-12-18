using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug;

public class DrawObjectsOverdrawPassData : DrawRendererListPassData
{
	public TextureHandle RenderTarget;

	public Color ClearColor;

	public Color DebugColor;

	public CameraType CameraType;

	public bool IsIndirectRenderingEnabled;

	public bool IsSceneViewInPrefabEditMode;
}
