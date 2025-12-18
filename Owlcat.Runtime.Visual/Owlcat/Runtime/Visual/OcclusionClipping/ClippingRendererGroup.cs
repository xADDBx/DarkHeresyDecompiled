using System;
using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.Waaagh;
using UnityEngine;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

internal sealed class ClippingRendererGroup : ClippingRendererBase
{
	private static GPUDrivenMaterialPropertyBlock s_Properties;

	[SerializeField]
	internal Renderer[] m_Renderers = Array.Empty<Renderer>();

	protected override void OnOpacityChanged(float opacity)
	{
		if (m_Renderers == null)
		{
			return;
		}
		if (s_Properties == null)
		{
			s_Properties = new GPUDrivenMaterialPropertyBlock();
		}
		Renderer[] renderers = m_Renderers;
		foreach (Renderer renderer in renderers)
		{
			if (renderer != null)
			{
				renderer.GetPropertyBlock(s_Properties);
				s_Properties.SetFloat(ShaderPropertyId._OccluderObjectOpacity, opacity);
				renderer.SetPropertyBlock(s_Properties);
			}
		}
	}
}
