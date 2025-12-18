using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Data;

[Serializable]
public class ProbeVolumesSettings
{
	public ProbeVolumeTextureMemoryBudget MemoryBudget = ProbeVolumeTextureMemoryBudget.MemoryBudgetMedium;

	public ProbeVolumeBlendingTextureMemoryBudget BlendingMemoryBudget = ProbeVolumeBlendingTextureMemoryBudget.MemoryBudgetMedium;

	public ProbeVolumeSHBands SHBands = ProbeVolumeSHBands.SphericalHarmonicsL1;

	public ShEvalMode EvaluationMode;

	public bool SupportGPUStreaming;

	public bool SupportDiskStreaming;

	public bool SupportScenarios;

	public bool SupportScenarioBlending;
}
