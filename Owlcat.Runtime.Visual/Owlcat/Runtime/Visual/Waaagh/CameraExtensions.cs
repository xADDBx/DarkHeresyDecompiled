using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

public static class CameraExtensions
{
	public static bool TryGetScriptableRenderer<T>(this Camera camera, out T result) where T : ScriptableRenderer
	{
		if (camera.TryGetComponent<WaaaghAdditionalCameraData>(out var component) && component.ScriptableRenderer is T val)
		{
			result = val;
			return true;
		}
		result = null;
		return false;
	}

	public static WaaaghAdditionalCameraData GetWaaaghAdditionalCameraData(this Camera camera)
	{
		GameObject gameObject = camera.gameObject;
		if (!gameObject.TryGetComponent<WaaaghAdditionalCameraData>(out var component))
		{
			return gameObject.AddComponent<WaaaghAdditionalCameraData>();
		}
		return component;
	}

	public static VolumeFrameworkUpdateMode GetVolumeFrameworkUpdateMode(this Camera camera)
	{
		return camera.GetWaaaghAdditionalCameraData().VolumeFrameworkUpdateMode;
	}

	public static void SetVolumeFrameworkUpdateMode(this Camera camera, VolumeFrameworkUpdateMode mode)
	{
		WaaaghAdditionalCameraData waaaghAdditionalCameraData = camera.GetWaaaghAdditionalCameraData();
		if (waaaghAdditionalCameraData.VolumeFrameworkUpdateMode != mode)
		{
			waaaghAdditionalCameraData.VolumeFrameworkUpdateMode = mode;
			if (!waaaghAdditionalCameraData.RequiresVolumeFrameworkUpdate)
			{
				camera.UpdateVolumeStack(waaaghAdditionalCameraData);
			}
		}
	}

	public static void UpdateVolumeStack(this Camera camera)
	{
		WaaaghAdditionalCameraData waaaghAdditionalCameraData = camera.GetWaaaghAdditionalCameraData();
		camera.UpdateVolumeStack(waaaghAdditionalCameraData);
	}

	public static void UpdateVolumeStack(this Camera camera, WaaaghAdditionalCameraData cameraData)
	{
		if (!cameraData.RequiresVolumeFrameworkUpdate)
		{
			if (cameraData.VolumeStack == null)
			{
				cameraData.GetOrCreateVolumeStack();
			}
			camera.GetVolumeLayerMaskAndTrigger(cameraData, out var layerMask, out var trigger);
			VolumeManager.instance.Update(cameraData.VolumeStack, trigger, layerMask);
		}
	}

	public static void DestroyVolumeStack(this Camera camera, WaaaghAdditionalCameraData cameraData)
	{
		if (!(cameraData == null) && cameraData.VolumeStack != null)
		{
			cameraData.VolumeStack = null;
		}
	}

	internal static void GetVolumeLayerMaskAndTrigger(this Camera camera, WaaaghAdditionalCameraData cameraData, out LayerMask layerMask, out Transform trigger)
	{
		if (cameraData != null)
		{
			layerMask = cameraData.VolumeLayerMask;
			trigger = ((cameraData.VolumeTrigger != null) ? cameraData.VolumeTrigger : camera.transform);
		}
		else
		{
			layerMask = 1;
			trigger = camera.transform;
		}
	}
}
