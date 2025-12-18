using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

public class WaaaghCameraStack : IDisposable
{
	private const int kMaxUnusedFramesCount = 4;

	private List<Camera> m_Cameras = new List<Camera>();

	private List<WaaaghAdditionalCameraData> m_AdditionaCameraDataList = new List<WaaaghAdditionalCameraData>();

	private Dictionary<Camera, WaaaghCameraStackBuffer> m_Buffers = new Dictionary<Camera, WaaaghCameraStackBuffer>();

	private List<Camera> m_UnusedBuffers = new List<Camera>();

	private Camera m_BaseCamera;

	private WaaaghAdditionalCameraData m_BaseAdditionalCameraData;

	private int m_LastCameraIndex;

	private int m_LastScaledCameraIndex;

	public Camera BaseCamera => m_BaseCamera;

	public WaaaghAdditionalCameraData BaseAdditionalCameraData => m_BaseAdditionalCameraData;

	public List<Camera> Cameras => m_Cameras;

	public List<WaaaghAdditionalCameraData> AdditionalCameraDataList => m_AdditionaCameraDataList;

	public int LastCameraIndex => m_LastCameraIndex;

	public int LastScaledCameraIndex => m_LastScaledCameraIndex;

	public bool AnyPostProcessingEnabled { get; private set; }

	public WaaaghCameraStackBuffer Buffer { get; private set; }

	public bool Build(Camera baseCamera)
	{
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.BuildCameraStack)))
		{
			m_BaseCamera = baseCamera;
			m_BaseCamera.TryGetComponent<WaaaghAdditionalCameraData>(out m_BaseAdditionalCameraData);
			AnyPostProcessingEnabled = false;
			CleanUnusedBuffers();
			m_Cameras.Clear();
			m_AdditionaCameraDataList.Clear();
			m_LastCameraIndex = 0;
			m_LastScaledCameraIndex = -1;
			return Build();
		}
	}

	private void CleanUnusedBuffers()
	{
		foreach (KeyValuePair<Camera, WaaaghCameraStackBuffer> buffer in m_Buffers)
		{
			Camera key = buffer.Key;
			WaaaghCameraStackBuffer value = buffer.Value;
			if (key == null)
			{
				value.UnusedFramesCount++;
			}
			else if (key.cameraType == CameraType.Game && !key.isActiveAndEnabled)
			{
				value.UnusedFramesCount++;
			}
			if (value.UnusedFramesCount >= 4)
			{
				m_UnusedBuffers.Add(key);
			}
		}
		foreach (Camera unusedBuffer in m_UnusedBuffers)
		{
			m_Buffers[unusedBuffer].Dispose();
			m_Buffers.Remove(unusedBuffer);
		}
		m_UnusedBuffers.Clear();
	}

	private bool Build()
	{
		if (m_BaseCamera == null)
		{
			throw new ArgumentNullException("baseCamera");
		}
		m_Cameras.Add(m_BaseCamera);
		if (m_BaseCamera.cameraType != CameraType.Game || m_BaseAdditionalCameraData == null)
		{
			m_AdditionaCameraDataList.Add(m_BaseAdditionalCameraData);
			return true;
		}
		if (m_BaseAdditionalCameraData.RenderType != 0)
		{
			return false;
		}
		m_AdditionaCameraDataList.Add(m_BaseAdditionalCameraData);
		m_BaseAdditionalCameraData.UpdateCameraStack();
		List<Camera> cameraStack = m_BaseAdditionalCameraData.CameraStack;
		if (cameraStack != null)
		{
			Type type = m_BaseAdditionalCameraData.ScriptableRenderer.GetType();
			foreach (Camera item in cameraStack)
			{
				if (!(item == null) && item.isActiveAndEnabled && item.TryGetComponent<WaaaghAdditionalCameraData>(out var component))
				{
					if (component.RenderType != CameraRenderType.Overlay)
					{
						Debug.LogWarning("Stack can only contain Overlay cameras. " + item.name + " will skip rendering.", item);
						continue;
					}
					if (type != component.ScriptableRenderer.GetType())
					{
						Debug.LogWarning("Only cameras with compatible renderer types can be stacked. " + item.name + " will skip rendering", item);
						continue;
					}
					m_Cameras.Add(item);
					m_AdditionaCameraDataList.Add(component);
				}
			}
		}
		for (int i = 0; i < m_AdditionaCameraDataList.Count; i++)
		{
			WaaaghAdditionalCameraData waaaghAdditionalCameraData = m_AdditionaCameraDataList[i];
			if (waaaghAdditionalCameraData.AllowRenderScaling)
			{
				m_LastScaledCameraIndex = i;
			}
			AnyPostProcessingEnabled |= waaaghAdditionalCameraData.RenderPostProcessing;
		}
		m_LastCameraIndex = m_AdditionaCameraDataList.Count - 1;
		return true;
	}

	public void EnsureStackBuffer(WaaaghCameraData cameraData)
	{
		if (!m_Buffers.TryGetValue(cameraData.camera, out var value))
		{
			value = new WaaaghCameraStackBuffer(cameraData);
			m_Buffers.Add(cameraData.camera, value);
		}
		else
		{
			value.Update(cameraData);
		}
		Buffer = value;
	}

	public void Dispose()
	{
		foreach (KeyValuePair<Camera, WaaaghCameraStackBuffer> buffer in m_Buffers)
		{
			buffer.Value?.Dispose();
		}
	}
}
