using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.Visual.CustomPostProcess;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides.CustomPostProcess;

[Serializable]
public class PostProcessCompositeParameter : CompositeParameter
{
	private Dictionary<ShaderPropertyParameter, ShaderPropertyDescriptor> m_ParameterPropertyMap;

	private Dictionary<int, int> m_CompositionMap;

	public CustomPostProcessEffect Effect;

	public List<ShaderPropertyParameter> Parameters = new List<ShaderPropertyParameter>();

	public override object Clone()
	{
		PostProcessCompositeParameter postProcessCompositeParameter = new PostProcessCompositeParameter();
		postProcessCompositeParameter.Effect = Effect;
		postProcessCompositeParameter.ResetToDefault();
		return postProcessCompositeParameter;
	}

	public override int GetContentHash()
	{
		return Effect.GetHashCode();
	}

	public override void Interp(CompositeParameter to, float t)
	{
		PostProcessCompositeParameter obj = to as PostProcessCompositeParameter;
		if (m_CompositionMap == null)
		{
			m_CompositionMap = new Dictionary<int, int>();
		}
		foreach (ShaderPropertyParameter parameter in obj.Parameters)
		{
			if (parameter.OverrideState)
			{
				ShaderPropertyParameter suitableShaderParameter = GetSuitableShaderParameter(parameter);
				suitableShaderParameter.OverrideState = parameter.OverrideState;
				suitableShaderParameter.Property.Interp(suitableShaderParameter.Property, parameter.Property, t);
			}
		}
	}

	private ShaderPropertyParameter GetSuitableShaderParameter(ShaderPropertyParameter toParameter)
	{
		if (!m_CompositionMap.TryGetValue(toParameter.GetContentHash(), out var value))
		{
			value = Parameters.Count;
			ShaderPropertyParameter item = toParameter.Clone();
			Parameters.Add(item);
			m_CompositionMap.Add(toParameter.GetContentHash(), value);
		}
		return Parameters[value];
	}

	public override void ResetToDefault()
	{
		if (m_ParameterPropertyMap == null)
		{
			InitDefaultParameters();
		}
		foreach (KeyValuePair<ShaderPropertyParameter, ShaderPropertyDescriptor> item in m_ParameterPropertyMap)
		{
			item.Key.OverrideState = false;
			if (item.Value != null)
			{
				item.Key.Property.SetValue(item.Value);
			}
		}
	}

	private void InitDefaultParameters()
	{
		m_ParameterPropertyMap = new Dictionary<ShaderPropertyParameter, ShaderPropertyDescriptor>();
		if (m_CompositionMap == null)
		{
			m_CompositionMap = new Dictionary<int, int>();
		}
		for (int i = 0; i < Effect.Passes.Count; i++)
		{
			foreach (ShaderPropertyDescriptor property in Effect.Passes[i].Properties)
			{
				ShaderPropertyParameter shaderPropertyParameter = new ShaderPropertyParameter
				{
					PassIndex = i,
					Property = new ShaderPropertyDescriptor(property)
				};
				m_ParameterPropertyMap.Add(shaderPropertyParameter, property);
				if (!m_CompositionMap.ContainsKey(shaderPropertyParameter.GetContentHash()))
				{
					m_CompositionMap.Add(shaderPropertyParameter.GetContentHash(), Parameters.Count);
					Parameters.Add(shaderPropertyParameter);
				}
			}
		}
	}

	internal bool Has(string passName, string parameterName, ShaderPropertyType parameterType)
	{
		return Parameters.Any((ShaderPropertyParameter p) => p.PassIndex < Effect.Passes.Count && Effect.Passes[p.PassIndex].Name == passName && p.Property.Name == parameterName && p.Property.Type == parameterType);
	}
}
