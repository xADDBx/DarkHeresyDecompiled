using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.RendererFeatureDelegates;
using Unity.Collections;
using Unity.Jobs;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures;

internal sealed class RendererFeatureExtensions : IDisposable
{
	private readonly List<SetupDelegate> m_SetupDelegates = new List<SetupDelegate>();

	private readonly List<CleanupDelegate> m_CleanupDelegates = new List<CleanupDelegate>();

	private readonly List<IRendererFeature> m_RendererFeatures = new List<IRendererFeature>();

	private readonly List<ScheduleSetupJobsDelegate> m_ScheduleSetupJobsDelegates = new List<ScheduleSetupJobsDelegate>();

	private readonly List<CompleteSetupJobsDelegate> m_CompleteSetupJobsDelegates = new List<CompleteSetupJobsDelegate>();

	private readonly List<RecordDelegate>[] m_RecordDelegates;

	private readonly List<RecordWithExtensionPointDelegate> m_RecordWithExtensionPointDelegates = new List<RecordWithExtensionPointDelegate>();

	public RendererFeatureExtensions(IEnumerable<RendererFeatureAsset> rendererFeatureAssets)
	{
		int length = Enum.GetValues(typeof(RecordExtensionPoint)).Length;
		m_RecordDelegates = new List<RecordDelegate>[length];
		RendererFeatureExtensionRegistry registry = new RendererFeatureExtensionRegistry(m_SetupDelegates, m_CleanupDelegates, m_ScheduleSetupJobsDelegates, m_CompleteSetupJobsDelegates, m_RecordDelegates, m_RecordWithExtensionPointDelegates);
		try
		{
			foreach (RendererFeatureAsset rendererFeatureAsset in rendererFeatureAssets)
			{
				if (!(rendererFeatureAsset == null))
				{
					IRendererFeature rendererFeature = rendererFeatureAsset.CreateRendererFeature();
					if (rendererFeature != null)
					{
						rendererFeature.RegisterExtensions(registry);
						m_RendererFeatures.Add(rendererFeature);
					}
				}
			}
		}
		catch
		{
			foreach (IRendererFeature rendererFeature2 in m_RendererFeatures)
			{
				rendererFeature2.Dispose();
			}
			throw;
		}
	}

	public void Dispose()
	{
		foreach (IRendererFeature rendererFeature in m_RendererFeatures)
		{
			rendererFeature.Dispose();
		}
	}

	public void Setup(in SetupContext context)
	{
		foreach (SetupDelegate setupDelegate in m_SetupDelegates)
		{
			setupDelegate(in context);
		}
	}

	public void Cleanup()
	{
		foreach (CleanupDelegate cleanupDelegate in m_CleanupDelegates)
		{
			cleanupDelegate();
		}
	}

	public JobHandle ScheduleSetupJobs(in SetupContext context, JobHandle dependency)
	{
		if (m_ScheduleSetupJobsDelegates.Count == 0)
		{
			return dependency;
		}
		if (m_ScheduleSetupJobsDelegates.Count == 1)
		{
			return m_ScheduleSetupJobsDelegates[0](in context, dependency);
		}
		NativeArray<JobHandle> jobs = new NativeArray<JobHandle>(m_ScheduleSetupJobsDelegates.Count, Allocator.Temp);
		for (int i = 0; i < m_ScheduleSetupJobsDelegates.Count; i++)
		{
			jobs[i] = m_ScheduleSetupJobsDelegates[i](in context, dependency);
		}
		JobHandle result = JobHandle.CombineDependencies(jobs);
		jobs.Dispose();
		return result;
	}

	public void CompleteSetupJobs(in SetupContext context)
	{
		foreach (CompleteSetupJobsDelegate completeSetupJobsDelegate in m_CompleteSetupJobsDelegates)
		{
			completeSetupJobsDelegate(in context);
		}
	}

	public void Record(in RecordContext context, RecordExtensionPoint extensionPoint)
	{
		List<RecordDelegate> list = m_RecordDelegates[(int)extensionPoint];
		if (list != null)
		{
			foreach (RecordDelegate item in list)
			{
				item(in context);
			}
		}
		foreach (RecordWithExtensionPointDelegate recordWithExtensionPointDelegate in m_RecordWithExtensionPointDelegates)
		{
			recordWithExtensionPointDelegate(extensionPoint, in context);
		}
	}
}
