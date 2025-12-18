using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DrawDecalsPassData : DrawRendererListPassData
{
	public Material DBufferBlitMaterial;

	public bool DrawGUIDecals;

	public TextureHandle CameraColorRT;

	public TextureHandle CameraDepthRT;

	public TextureHandle CameraDepthCopyRT;

	public TextureHandle CameraAlbedoRT;

	public TextureHandle CameraSpecularRT;

	public TextureHandle CameraNormalsRT;

	public TextureHandle CameraEmissionRT;

	public TextureHandle CameraTranslucencyRT;

	public TextureHandle DBuffer0RT;

	public TextureHandle DBuffer1RT;

	public TextureHandle VTFeedbackRT;

	public Color ClearColor;

	public readonly List<ICustomDecalDrawer> CustomDecalDrawer = new List<ICustomDecalDrawer>();
}
