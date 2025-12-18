using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public static class GPUDrivenMaterialOverrides
{
	private struct MaterialOverride
	{
		public Material Material;

		public NativeArray<GPUDrivenRenderer.PropertyData> PropertyData;
	}

	private static class Profiling
	{
		public static ProfilingSampler FlushSampler = new ProfilingSampler("FlushSampler");
	}

	private static readonly List<MaterialOverride> PendingOverrides = new List<MaterialOverride>();

	private static GPUDrivenBatchRendererGroup s_BRG;

	internal static void Init(GPUDrivenBatchRendererGroup batchRendererGroup)
	{
		s_BRG = batchRendererGroup;
	}

	internal static void Cleanup()
	{
		foreach (MaterialOverride pendingOverride in PendingOverrides)
		{
			Release(pendingOverride);
		}
		PendingOverrides.Clear();
		s_BRG = null;
	}

	public static void ScheduleWrite(Material material, NativeArray<GPUDrivenRenderer.PropertyData> propertyData)
	{
		NativeArray<GPUDrivenRenderer.PropertyData> propertyData2 = new NativeArray<GPUDrivenRenderer.PropertyData>(propertyData.Length, Allocator.TempJob);
		propertyData2.CopyFrom(propertyData);
		PendingOverrides.Add(new MaterialOverride
		{
			Material = material,
			PropertyData = propertyData2
		});
	}

	public static void ScheduleWriteDefault(Material material)
	{
		PendingOverrides.Add(new MaterialOverride
		{
			Material = material,
			PropertyData = default(NativeArray<GPUDrivenRenderer.PropertyData>)
		});
	}

	internal static void Flush()
	{
		using (new ProfilingScope(Profiling.FlushSampler))
		{
			if (!s_BRG.IsEnabledAndInitialized && PendingOverrides.Count > 0)
			{
				Debug.LogWarning("Scheduled material overrides while GPU Driven Rendering is disabled.");
				foreach (MaterialOverride pendingOverride in PendingOverrides)
				{
					Release(pendingOverride);
				}
			}
			else
			{
				foreach (MaterialOverride pendingOverride2 in PendingOverrides)
				{
					if (!(pendingOverride2.Material == null))
					{
						NativeArray<GPUDrivenRenderer.PropertyData> propertyData = pendingOverride2.PropertyData;
						if (propertyData.IsCreated)
						{
							s_BRG.WriteMaterialData(pendingOverride2.Material, pendingOverride2.PropertyData);
						}
						else
						{
							s_BRG.WriteDefaultMaterialData(pendingOverride2.Material);
						}
						Release(pendingOverride2);
					}
				}
			}
			PendingOverrides.Clear();
		}
	}

	private static void Release(MaterialOverride pendingOverride)
	{
		if (pendingOverride.PropertyData.IsCreated)
		{
			pendingOverride.PropertyData.Dispose();
		}
	}
}
