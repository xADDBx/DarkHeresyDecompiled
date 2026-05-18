using System;
using Owlcat.Runtime.Visual.Lighting;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Lighting;

public sealed class WaaaghReflectionProbes : IDisposable
{
	private ReflectionProbeManager m_ReflectionProbeManager = ReflectionProbeManager.Create();

	public RTHandle AtlasRTHandle => m_ReflectionProbeManager.atlasRTHandle;

	public void Dispose()
	{
		m_ReflectionProbeManager.Dispose();
	}

	public void PrepareCpuData(ref CullingResults cullingResults)
	{
		m_ReflectionProbeManager.PrepareCpuData(ref cullingResults);
	}

	public void BlitAndSetGlobals(CommandBuffer cmd)
	{
		m_ReflectionProbeManager.BlitAndSetGlobals(cmd);
	}

	public void FillGlobalShaderVariables(ref WaaaghShaderVariablesGlobal g)
	{
		m_ReflectionProbeManager.FillGlobalShaderVariables(ref g);
	}

	public bool TryMapVisibleProbeToGpuDataIndex(int visibleProbeIndex, out int dataIndex)
	{
		return m_ReflectionProbeManager.TryMapVisibleProbeToGpuDataIndex(visibleProbeIndex, out dataIndex);
	}
}
