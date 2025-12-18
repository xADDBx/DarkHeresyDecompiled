using System.Collections.Generic;
using Owlcat.Runtime.Visual.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.History;

public static class WaaaghCameraHistoryManager
{
	private const int kMaxUnusedFrames = 10;

	private static Dictionary<Camera, WaaaghCameraHistory> m_All = new Dictionary<Camera, WaaaghCameraHistory>();

	public static WaaaghCameraHistory EnsureCamera(Camera camera)
	{
		if (!m_All.TryGetValue(camera, out var value))
		{
			value = new WaaaghCameraHistory();
			m_All.Add(camera, value);
		}
		return value;
	}

	public static void GC()
	{
		List<Camera> value;
		using (ListPool<Camera>.Get(out value))
		{
			foreach (KeyValuePair<Camera, WaaaghCameraHistory> item in m_All)
			{
				if (item.Key == null || item.Value == null || FrameId.FrameCount - item.Value.LastFrameRequested >= 10)
				{
					value.Add(item.Key);
				}
			}
			for (int i = 0; i < value.Count; i++)
			{
				Camera key = value[i];
				m_All[key]?.Dispose();
				m_All.Remove(key);
			}
			value.Clear();
		}
	}

	internal static void DisposeAll()
	{
		foreach (KeyValuePair<Camera, WaaaghCameraHistory> item in m_All)
		{
			if (item.Value != null)
			{
				item.Value.Dispose();
			}
		}
		m_All.Clear();
	}

	internal static void OnAdditionalCameraDataDestroy(WaaaghAdditionalCameraData waaaghAdditionalCameraData)
	{
		Camera camera = waaaghAdditionalCameraData.camera;
		if (m_All.ContainsKey(camera))
		{
			m_All[camera]?.Dispose();
			m_All.Remove(camera);
		}
	}
}
