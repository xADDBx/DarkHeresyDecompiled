using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.PostProcess;

public class StaticState
{
	public GraphicsFormat DefaultColorFormat;

	public GraphicsFormat SMAAEdgeFormat;

	public GraphicsFormat GaussianCoCFormat;

	public bool DefaultColorFormatUseRGBM;

	public bool DefaultColorFormatUseAlpha;

	public string[] BloomMipDownName;

	public string[] BloomMipUpName;

	public TextureHandle[] BloomMipDown;

	public TextureHandle[] BloomMipUp;
}
