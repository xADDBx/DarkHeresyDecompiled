using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

[DisallowMultipleComponent]
[DefaultExecutionOrder(100)]
internal sealed class ClippingLinker : MonoBehaviour
{
	private readonly List<int> m_TriggerInstanceIds = new List<int>();

	private readonly List<int> m_VolumeInstanceIds = new List<int>();

	private void OnEnable()
	{
		List<ClippingTrigger> value;
		using (CollectionPool<List<ClippingTrigger>, ClippingTrigger>.Get(out value))
		{
			GetComponentsInChildren(includeInactive: false, value);
			foreach (ClippingTrigger item in value)
			{
				m_TriggerInstanceIds.Add(item.GetInstanceID());
			}
		}
		List<ClippingVolume> value2;
		using (CollectionPool<List<ClippingVolume>, ClippingVolume>.Get(out value2))
		{
			GetComponentsInChildren(includeInactive: false, value2);
			foreach (ClippingVolume item2 in value2)
			{
				m_VolumeInstanceIds.Add(item2.GetInstanceID());
			}
		}
		foreach (int triggerInstanceId in m_TriggerInstanceIds)
		{
			foreach (int volumeInstanceId in m_VolumeInstanceIds)
			{
				ClippingSystem.CreateTriggerVolumeLink(triggerInstanceId, volumeInstanceId);
			}
		}
	}

	private void OnDisable()
	{
		foreach (int triggerInstanceId in m_TriggerInstanceIds)
		{
			foreach (int volumeInstanceId in m_VolumeInstanceIds)
			{
				ClippingSystem.DestroyTriggerVolumeLink(triggerInstanceId, volumeInstanceId);
			}
		}
		m_TriggerInstanceIds.Clear();
		m_VolumeInstanceIds.Clear();
	}
}
