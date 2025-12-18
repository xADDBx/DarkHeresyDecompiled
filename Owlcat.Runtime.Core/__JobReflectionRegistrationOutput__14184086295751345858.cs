using System;
using Owlcat.Runtime.Core.Allocators.Guillotiere;
using Owlcat.Runtime.Core.Allocators.MaxRectsBinPack.Native;
using Unity.Jobs;
using UnityEngine;

[Unity.Jobs.DOTSCompilerGenerated]
internal class __JobReflectionRegistrationOutput__14184086295751345858
{
	public static void CreateJobReflectionData()
	{
		try
		{
			IJobExtensions.EarlyJobInit<NativeMaxRectsBinPack.AllocationJob>();
			IJobExtensions.EarlyJobInit<AllocationJob>();
		}
		catch (Exception ex)
		{
			EarlyInitHelpers.JobReflectionDataCreationFailed(ex);
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	public static void EarlyInit()
	{
		CreateJobReflectionData();
	}
}
