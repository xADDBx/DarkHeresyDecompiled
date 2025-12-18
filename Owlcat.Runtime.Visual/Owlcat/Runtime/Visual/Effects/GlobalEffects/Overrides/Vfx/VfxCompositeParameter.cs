using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Overrides;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.Overrides.Vfx;

[Serializable]
public class VfxCompositeParameter : CompositeParameter
{
	private Dictionary<int, int> m_CompositionMap;

	public VisualEffectAsset VfxAsset;

	public List<VfxPropertyParameter> Parameters = new List<VfxPropertyParameter>();

	public override object Clone()
	{
		return new VfxCompositeParameter
		{
			VfxAsset = VfxAsset
		};
	}

	public override int GetContentHash()
	{
		return VfxAsset.GetHashCode();
	}

	public override void Interp(CompositeParameter to, float t)
	{
		VfxCompositeParameter obj = to as VfxCompositeParameter;
		if (m_CompositionMap == null)
		{
			m_CompositionMap = new Dictionary<int, int>();
		}
		foreach (VfxPropertyParameter parameter in obj.Parameters)
		{
			if (parameter.OverrideState)
			{
				VfxPropertyParameter suitableVfxParameter = GetSuitableVfxParameter(parameter);
				suitableVfxParameter.OverrideState = parameter.OverrideState;
				suitableVfxParameter.Interp(parameter, t);
			}
		}
	}

	public override void ResetToDefault()
	{
		foreach (VfxPropertyParameter parameter in Parameters)
		{
			parameter.OverrideState = false;
			parameter.ResetToDefault();
		}
	}

	private VfxPropertyParameter GetSuitableVfxParameter(VfxPropertyParameter vfxParameter)
	{
		if (!m_CompositionMap.TryGetValue(vfxParameter.GetContentHash(), out var value))
		{
			value = Parameters.Count;
			Parameters.Add(vfxParameter.Clone());
			m_CompositionMap.Add(vfxParameter.GetContentHash(), value);
		}
		return Parameters[value];
	}
}
