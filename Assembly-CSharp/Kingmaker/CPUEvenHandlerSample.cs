using System;
using Owlcat.Runtime.Visual.VFXGraph;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Kingmaker;

public class CPUEvenHandlerSample : MonoBehaviour
{
	private VFXTriggerCpuEvent m_VfxEvent;

	private void OnEnable()
	{
		m_VfxEvent = GetComponent<VFXTriggerCpuEvent>();
		VFXTriggerCpuEvent vfxEvent = m_VfxEvent;
		vfxEvent.OnEventReceived = (Action<uint, NativeArray<float3>>)Delegate.Combine(vfxEvent.OnEventReceived, new Action<uint, NativeArray<float3>>(OnEventReceived));
	}

	private void OnDisable()
	{
		VFXTriggerCpuEvent vfxEvent = m_VfxEvent;
		vfxEvent.OnEventReceived = (Action<uint, NativeArray<float3>>)Delegate.Remove(vfxEvent.OnEventReceived, new Action<uint, NativeArray<float3>>(OnEventReceived));
	}

	private void OnEventReceived(uint count, NativeArray<float3> positions)
	{
		Debug.Log($"CPU Event Received: {count} events");
	}
}
