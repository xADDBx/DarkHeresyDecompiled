using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.FrameData;

public class CameraStackTargets
{
	private TextureHandle m_Color;

	private TextureHandle m_Depth;

	private TextureHandle m_UnscaledColor;

	private TextureHandle m_UnscaledDepth;

	private TextureHandle m_CurrentPostProcessSource;

	public TextureHandle Color => m_Color;

	public TextureHandle Depth => m_Depth;

	public TextureHandle UnscaledColor => m_UnscaledColor;

	public TextureHandle UnscaledDepth => m_UnscaledDepth;

	public TextureHandle CurrentPostProcessSource => m_CurrentPostProcessSource;

	internal void SetCurrentPostProcessSource(TextureHandle source)
	{
		m_CurrentPostProcessSource = source;
	}

	internal void SetTargets(TextureHandle color, TextureHandle depth, TextureHandle unscaledColor, TextureHandle unscaledDepth)
	{
		m_Color = color;
		m_Depth = depth;
		m_UnscaledColor = unscaledColor;
		m_UnscaledDepth = unscaledDepth;
		m_CurrentPostProcessSource = color;
	}

	internal void ReplaceScaledTargetsByUnscaled()
	{
		m_Color = m_UnscaledColor;
		m_Depth = m_UnscaledDepth;
	}
}
