using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.History;

public abstract class WaaaghHistoryItem : CameraHistoryItem
{
	private static string s_Name;

	private static RenderTextureDescriptor s_TextureDescriptor;

	protected void AllocHistory(int id, int count, ref RenderTextureDescriptor desc, string name)
	{
		s_Name = name;
		s_TextureDescriptor = desc;
		base.storage.AllocBuffer(id, Alloc, count);
	}

	private RTHandle Alloc(RTHandleSystem rtHandles, int historyIndex)
	{
		return Alloc(ref s_TextureDescriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, isShadow: false, 0, 0f, $"{s_Name}_{historyIndex}");
		RTHandle Alloc(ref RenderTextureDescriptor d, FilterMode fMode, TextureWrapMode wMode, bool isShadow, int aniso, float mipBias, string n)
		{
			return rtHandles.Alloc(d.width, d.height, d.volumeDepth, (DepthBits)d.depthBufferBits, d.graphicsFormat, fMode, wMode, d.dimension, d.enableRandomWrite, d.useMipMap, d.autoGenerateMips, isShadow, aniso, mipBias, (MSAASamples)d.msaaSamples, d.bindMS, d.useDynamicScale, d.useDynamicScaleExplicit, d.memoryless, d.vrUsage, n);
		}
	}
}
