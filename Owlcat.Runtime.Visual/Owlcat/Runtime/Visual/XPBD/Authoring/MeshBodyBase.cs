using Owlcat.Runtime.Visual.XPBD.Layouts;
using Owlcat.Runtime.Visual.XPBD.Solvers;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Authoring;

public abstract class MeshBodyBase : AuthoringBase<MeshLayout>
{
	private const uint kHiddenRendererLayerMask = 0u;

	private bool m_RendererHidden;

	private uint m_HiddenRendererOriginalLayerMask;

	[SerializeField]
	private bool m_HideRenderer;

	[Tooltip("Hides renderer (not disable) by setting Renderer.renderingLayerMask = Nothing")]
	public bool HideRenderer
	{
		get
		{
			return m_HideRenderer;
		}
		set
		{
			m_HideRenderer = value;
			UpdateRendererVisibility();
		}
	}

	protected abstract Renderer Renderer { get; }

	private void UpdateRendererVisibility()
	{
		if (!(Renderer == null) && m_HideRenderer != m_RendererHidden)
		{
			if (m_HideRenderer)
			{
				m_RendererHidden = true;
				m_HiddenRendererOriginalLayerMask = Renderer.renderingLayerMask;
				Renderer.renderingLayerMask = 0u;
			}
			else
			{
				Renderer.renderingLayerMask = m_HiddenRendererOriginalLayerMask;
				m_RendererHidden = false;
			}
		}
	}

	private void OnValidate()
	{
		UpdateRendererVisibility();
	}

	protected override void OnRegister()
	{
		UpdateRendererVisibility();
	}

	internal override void AfterEnabledStateSync(Solver solver)
	{
		UpdateRendererVisibility();
	}
}
