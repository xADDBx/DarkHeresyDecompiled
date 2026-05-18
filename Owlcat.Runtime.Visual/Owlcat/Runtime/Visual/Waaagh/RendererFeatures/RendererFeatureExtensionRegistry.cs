using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.RendererFeatureDelegates;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures;

public struct RendererFeatureExtensionRegistry
{
	private readonly List<SetupDelegate> m_SetupDelegates;

	private readonly List<CleanupDelegate> m_CleanupDelegates;

	private readonly List<ScheduleSetupJobsDelegate> m_ScheduleSetupJobsDelegates;

	private readonly List<CompleteSetupJobsDelegate> m_CompleteSetupJobsDelegates;

	private readonly List<RecordDelegate>[] m_RecordDelegates;

	private readonly List<RecordWithExtensionPointDelegate> m_RecordWithExtensionPointDelegates;

	public RendererFeatureExtensionRegistry(List<SetupDelegate> setupDelegates, List<CleanupDelegate> cleanupDelegates, List<ScheduleSetupJobsDelegate> scheduleSetupJobsDelegates, List<CompleteSetupJobsDelegate> completeSetupJobsDelegates, List<RecordDelegate>[] recordDelegates, List<RecordWithExtensionPointDelegate> recordWithExtensionPointDelegates)
	{
		m_SetupDelegates = setupDelegates;
		m_CleanupDelegates = cleanupDelegates;
		m_ScheduleSetupJobsDelegates = scheduleSetupJobsDelegates;
		m_CompleteSetupJobsDelegates = completeSetupJobsDelegates;
		m_RecordDelegates = recordDelegates;
		m_RecordWithExtensionPointDelegates = recordWithExtensionPointDelegates;
	}

	public void AddSetupDelegate(SetupDelegate setupDelegate)
	{
		m_SetupDelegates.Add(setupDelegate);
	}

	public void AddCleanupDelegate(CleanupDelegate cleanupDelegate)
	{
		m_CleanupDelegates.Add(cleanupDelegate);
	}

	public void AddScheduleSetupJobsDelegate(ScheduleSetupJobsDelegate scheduleSetupJobsDelegate)
	{
		m_ScheduleSetupJobsDelegates.Add(scheduleSetupJobsDelegate);
	}

	public void AddCompleteSetupJobsDelegate(CompleteSetupJobsDelegate completeSetupJobsDelegate)
	{
		m_CompleteSetupJobsDelegates.Add(completeSetupJobsDelegate);
	}

	public void AddRecordDelegate(RecordExtensionPoint extensionPoint, RecordDelegate recordDelegate)
	{
		List<RecordDelegate>[] recordDelegates = m_RecordDelegates;
		if (recordDelegates[(int)extensionPoint] == null)
		{
			recordDelegates[(int)extensionPoint] = new List<RecordDelegate>();
		}
		m_RecordDelegates[(int)extensionPoint].Add(recordDelegate);
	}

	public void AddRecordDelegate(RecordWithExtensionPointDelegate recordDelegate)
	{
		m_RecordWithExtensionPointDelegates.Add(recordDelegate);
	}
}
