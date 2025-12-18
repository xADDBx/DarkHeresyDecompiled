using System;
using System.Collections.Generic;
using Unity.Jobs;

namespace Owlcat.Runtime.Visual.XPBD.Utilities;

public class JobCombiner<T> where T : Enum
{
	private Dictionary<T, JobHandle> m_GroupHandlesMap = new Dictionary<T, JobHandle>();

	public JobHandle Handle;

	public void CombineWithGroup(T groupId, JobHandle handle)
	{
		if (m_GroupHandlesMap.TryGetValue(groupId, out var value))
		{
			m_GroupHandlesMap[groupId] = JobHandle.CombineDependencies(value, handle);
		}
		else
		{
			m_GroupHandlesMap.Add(groupId, handle);
		}
	}

	public void FlushGroup(T groupId)
	{
		if (m_GroupHandlesMap.TryGetValue(groupId, out var value))
		{
			Handle = value;
			m_GroupHandlesMap.Remove(groupId);
		}
	}

	public void FlushAllGroups()
	{
		JobHandle jobHandle = default(JobHandle);
		foreach (KeyValuePair<T, JobHandle> item in m_GroupHandlesMap)
		{
			jobHandle = JobHandle.CombineDependencies(item.Value, jobHandle);
		}
		Handle = jobHandle;
		m_GroupHandlesMap.Clear();
	}

	public void Clear()
	{
		m_GroupHandlesMap.Clear();
		Handle = default(JobHandle);
	}
}
