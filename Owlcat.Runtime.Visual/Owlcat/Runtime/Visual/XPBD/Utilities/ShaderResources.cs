using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Utilities;

public class ShaderResources
{
	private List<ComputeShader> m_ComputeShaders = new List<ComputeShader>();

	public ComputeShader LoadComputeShader(string shaderName)
	{
		ComputeShader computeShader = Resources.Load<ComputeShader>(shaderName);
		m_ComputeShaders.Add(computeShader);
		return computeShader;
	}

	public void Dispose()
	{
	}
}
