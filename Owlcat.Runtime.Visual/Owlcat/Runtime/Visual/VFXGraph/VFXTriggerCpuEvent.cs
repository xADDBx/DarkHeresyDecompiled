using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.VFXGraph;

[RequireComponent(typeof(VisualEffect))]
public class VFXTriggerCpuEvent : MonoBehaviour
{
	private const int kMaxEventCount = 16;

	private int[] m_CounterResetData = new int[1];

	private GraphicsBuffer m_Buffer;

	private VisualEffect m_Vfx;

	private AsyncGPUReadbackRequest m_Readback;

	private int m_EventBufferNameId;

	private NativeArray<float3> m_Positions;

	public string EventBufferName = "EventBuffer";

	public Action<uint, NativeArray<float3>> OnEventReceived;

	private void OnEnable()
	{
		int count = 49;
		m_Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, count, 4);
		ResetCounter();
		m_EventBufferNameId = Shader.PropertyToID(EventBufferName);
		m_Vfx = GetComponent<VisualEffect>();
		m_Vfx.SetGraphicsBuffer(m_EventBufferNameId, m_Buffer);
		m_Positions = new NativeArray<float3>(16, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	private void Update()
	{
		if (m_Readback.done)
		{
			m_Readback = AsyncGPUReadback.Request(m_Buffer, OnReadback);
			ResetCounter();
		}
	}

	private void ResetCounter()
	{
		m_Buffer.SetData(m_CounterResetData);
	}

	private void OnReadback(AsyncGPUReadbackRequest request)
	{
		NativeArray<float> data = request.GetData<float>();
		if (!data.IsCreated)
		{
			return;
		}
		uint num = math.asuint(data[0]);
		if (num != 0)
		{
			for (int i = 0; i < num; i++)
			{
				m_Positions[i] = new float3(data[1 + i * 3], data[2 + i * 3], data[3 + i * 3]);
			}
			OnEventReceived?.Invoke(num, m_Positions);
		}
	}

	private void OnDisable()
	{
		m_Buffer?.Release();
		if (m_Positions.IsCreated)
		{
			m_Positions.Dispose();
		}
	}
}
