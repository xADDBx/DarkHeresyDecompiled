using System;
using Owlcat.Runtime.Visual.Lighting;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Lighting;

public sealed class WaaaghReflectionProbes : IDisposable
{
	private ReflectionProbeManager m_ReflectionProbeManager = ReflectionProbeManager.Create();

	public void Dispose()
	{
		m_ReflectionProbeManager.Dispose();
	}

	public void UpdateGpuData(CommandBuffer cmd, ref CullingResults cullingResults)
	{
		m_ReflectionProbeManager.UpdateGpuData(cmd, ref cullingResults);
	}

	public bool TryMapVisibleProbeToGpuDataIndex(int visibleProbeIndex, out int dataIndex)
	{
		return m_ReflectionProbeManager.TryMapVisibleProbeToGpuDataIndex(visibleProbeIndex, out dataIndex);
	}
}
