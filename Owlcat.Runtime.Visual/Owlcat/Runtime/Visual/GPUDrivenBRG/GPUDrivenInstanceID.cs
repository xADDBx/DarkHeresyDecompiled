using System;
using System.Runtime.CompilerServices;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public readonly struct GPUDrivenInstanceID : IEquatable<GPUDrivenInstanceID>
{
	public enum InstanceIDType
	{
		Invalid,
		UnityObject,
		Custom
	}

	public readonly int RawInstanceID;

	public readonly InstanceIDType Type;

	public GPUDrivenInstanceID(int instanceID, InstanceIDType type)
	{
		RawInstanceID = instanceID;
		Type = type;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GPUDrivenInstanceID Invalid()
	{
		return new GPUDrivenInstanceID(0, InstanceIDType.Invalid);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GPUDrivenInstanceID UnityObject(int instanceID)
	{
		return new GPUDrivenInstanceID(instanceID, InstanceIDType.UnityObject);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GPUDrivenInstanceID Custom(int instanceID)
	{
		return new GPUDrivenInstanceID(instanceID, InstanceIDType.Custom);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(GPUDrivenInstanceID other)
	{
		if (RawInstanceID == other.RawInstanceID)
		{
			return Type == other.Type;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is GPUDrivenInstanceID other)
		{
			return Equals(other);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode()
	{
		return (RawInstanceID * 397) ^ (int)Type;
	}

	public override string ToString()
	{
		return $"{Type}:{RawInstanceID}";
	}
}
