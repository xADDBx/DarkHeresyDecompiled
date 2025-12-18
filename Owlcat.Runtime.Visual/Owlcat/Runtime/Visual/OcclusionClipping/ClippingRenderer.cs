using JetBrains.Annotations;
using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.Waaagh;
using UnityEngine;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

internal sealed class ClippingRenderer : ClippingRendererBase
{
	private static GPUDrivenMaterialPropertyBlock s_Properties;

	private Renderer m_Renderer;

	[UsedImplicitly]
	private void Awake()
	{
		m_Renderer = GetComponent<Renderer>();
	}

	protected override void OnOpacityChanged(float opacity)
	{
		if (m_Renderer != null)
		{
			if (s_Properties == null)
			{
				s_Properties = new GPUDrivenMaterialPropertyBlock();
			}
			m_Renderer.GetPropertyBlock(s_Properties);
			s_Properties.SetFloat(ShaderPropertyId._OccluderObjectOpacity, opacity);
			m_Renderer.SetPropertyBlock(s_Properties);
		}
	}
}
