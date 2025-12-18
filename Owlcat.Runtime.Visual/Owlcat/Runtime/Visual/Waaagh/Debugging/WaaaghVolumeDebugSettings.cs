using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

public class WaaaghVolumeDebugSettings : VolumeDebugSettings<WaaaghAdditionalCameraData>
{
	public override VolumeStack selectedCameraVolumeStack
	{
		get
		{
			if (base.selectedCamera == null)
			{
				return null;
			}
			WaaaghAdditionalCameraData component = base.selectedCamera.GetComponent<WaaaghAdditionalCameraData>();
			if (component == null)
			{
				return null;
			}
			VolumeStack volumeStack = component.VolumeStack;
			if (volumeStack != null)
			{
				return volumeStack;
			}
			return VolumeManager.instance.stack;
		}
	}

	public override LayerMask selectedCameraLayerMask
	{
		get
		{
			if (base.selectedCamera != null && base.selectedCamera.TryGetComponent<WaaaghAdditionalCameraData>(out var component))
			{
				return component.VolumeLayerMask;
			}
			return 1;
		}
	}

	public override Vector3 selectedCameraPosition
	{
		get
		{
			if (!(base.selectedCamera != null))
			{
				return Vector3.zero;
			}
			return base.selectedCamera.transform.position;
		}
	}
}
