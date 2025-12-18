using System;
using UnityEngine.SceneManagement;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Scenes;

public struct GPUDrivenSceneHandle : IEquatable<GPUDrivenSceneHandle>, IComparable<GPUDrivenSceneHandle>
{
	public int NativeHandle;

	public static GPUDrivenSceneHandle FromScene(in Scene scene)
	{
		GPUDrivenSceneHandle result = default(GPUDrivenSceneHandle);
		result.NativeHandle = scene.handle;
		return result;
	}

	public bool Equals(GPUDrivenSceneHandle other)
	{
		return NativeHandle == other.NativeHandle;
	}

	public override bool Equals(object obj)
	{
		if (obj is GPUDrivenSceneHandle other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return NativeHandle;
	}

	public int CompareTo(GPUDrivenSceneHandle other)
	{
		return NativeHandle.CompareTo(other.NativeHandle);
	}
}
