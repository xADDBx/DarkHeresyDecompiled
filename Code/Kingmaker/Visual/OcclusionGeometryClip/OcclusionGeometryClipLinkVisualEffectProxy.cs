using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace Kingmaker.Visual.OcclusionGeometryClip;

public class OcclusionGeometryClipLinkVisualEffectProxy : OcclusionGeometryClipLinkProxy
{
	[SerializeField]
	private ExposedProperty m_OpacityProperty = "OcclusionGeometryClipOpacity";

	private VisualEffect m_VisualEffect;

	private void OnEnable()
	{
		m_VisualEffect = GetComponent<VisualEffect>();
	}

	public override void SetOpacity(float value)
	{
		if (m_VisualEffect != null && m_VisualEffect.HasFloat(m_OpacityProperty))
		{
			m_VisualEffect.SetFloat(m_OpacityProperty, value);
		}
	}
}
