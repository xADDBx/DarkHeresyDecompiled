using System.Linq;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.SnapMap;

[VFXBinder("Global Effects/VfxGlobalSnapMapBinder")]
public class VfxGlobalSnapMapBinder : VFXBinderBase
{
	[VFXPropertyBinding(new string[] { "UnityEngine.GraphicsBuffer" })]
	public ExposedProperty SnapMapProperty = "SnapMap";

	private GraphicsBuffer m_SnapMapBuffer;

	public override bool IsValid(VisualEffect component)
	{
		return component.HasGraphicsBuffer(SnapMapProperty);
	}

	protected override void OnDisable()
	{
		m_SnapMapBuffer?.Dispose();
	}

	public override void UpdateBinding(VisualEffect component)
	{
		int num = VfxGlobalSnapMap.All.Sum((VfxGlobalSnapMap s) => s.SnapPoints.Count);
		if (num > 0)
		{
			if (m_SnapMapBuffer == null || !m_SnapMapBuffer.IsValid() || m_SnapMapBuffer.count < num)
			{
				m_SnapMapBuffer?.Dispose();
				m_SnapMapBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, num, 12);
			}
		}
		else if (m_SnapMapBuffer != null)
		{
			m_SnapMapBuffer.Dispose();
			m_SnapMapBuffer = null;
		}
		int num2 = 0;
		foreach (VfxGlobalSnapMap item in VfxGlobalSnapMap.All)
		{
			m_SnapMapBuffer.SetData(item.SnapPoints, 0, num2, item.SnapPoints.Count);
			num2 += item.SnapPoints.Count;
		}
		component.SetGraphicsBuffer(SnapMapProperty, m_SnapMapBuffer);
	}
}
