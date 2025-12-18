using System;
using UnityEngine;

namespace Kingmaker.Visual.OcclusionGeometryClip;

internal static class OcclusionGeometryClipLinkSystem
{
	private const int kInitialCapacity = 16;

	private static OcclusionGeometryClipLinkVolumeProxy[] s_Volumes = new OcclusionGeometryClipLinkVolumeProxy[16];

	private static PlaneBox[] s_VolumeBounds = new PlaneBox[16];

	private static int s_VolumeCount = 0;

	public static void AddVolume(OcclusionGeometryClipLinkVolumeProxy volume)
	{
		if (s_VolumeCount == s_Volumes.Length)
		{
			int num = s_VolumeCount * 2;
			OcclusionGeometryClipLinkVolumeProxy[] destinationArray = new OcclusionGeometryClipLinkVolumeProxy[num];
			PlaneBox[] destinationArray2 = new PlaneBox[num];
			Array.Copy(s_Volumes, destinationArray, s_VolumeCount);
			Array.Copy(s_VolumeBounds, destinationArray2, s_VolumeCount);
			s_Volumes = destinationArray;
			s_VolumeBounds = destinationArray2;
		}
		int num2 = (volume.RegistryIndex = s_VolumeCount);
		s_VolumeCount++;
		s_Volumes[num2] = volume;
		s_VolumeBounds[num2] = volume.Bounds;
	}

	public static void RemoveVolume(OcclusionGeometryClipLinkVolumeProxy volume)
	{
		int registryIndex = volume.RegistryIndex;
		int num = s_VolumeCount;
		if (registryIndex != num)
		{
			s_Volumes[registryIndex] = s_Volumes[num];
			s_VolumeBounds[registryIndex] = s_VolumeBounds[num];
			s_Volumes[registryIndex].RegistryIndex = registryIndex;
		}
		s_Volumes[num] = null;
		s_VolumeCount--;
		volume.RegistryIndex = -1;
	}

	public static void UpdateVolume(OcclusionGeometryClipLinkVolumeProxy volume)
	{
		if (volume.RegistryIndex >= 0 && volume.RegistryIndex < s_VolumeCount)
		{
			s_VolumeBounds[volume.RegistryIndex] = volume.Bounds;
		}
	}

	public static void AddObject(OcclusionGeometryClipLinkProxy proxy)
	{
		if (TryGetVolumeAtPoint(proxy.transform.position, out var result))
		{
			result.AddProxy(proxy);
			proxy.LinkedVolume = result;
		}
	}

	public static void RemoveObject(OcclusionGeometryClipLinkProxy proxy)
	{
		if ((object)proxy.LinkedVolume != null)
		{
			proxy.LinkedVolume.RemoveProxy(proxy);
			proxy.LinkedVolume = null;
		}
	}

	private static bool TryGetVolumeAtPoint(Vector3 point, out OcclusionGeometryClipLinkVolumeProxy result)
	{
		Vector4 point2 = new Vector4(point.x, point.y, point.z, 1f);
		for (int i = 0; i < s_VolumeCount; i++)
		{
			if (s_VolumeBounds[i].ContainsPoint(in point2))
			{
				result = s_Volumes[i];
				return true;
			}
		}
		result = null;
		return false;
	}
}
