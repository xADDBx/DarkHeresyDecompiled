using System;
using JetBrains.Annotations;
using Unity.Burst;
using UnityEngine;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.BakedLighting;

[Serializable]
public struct LightmapArraysCollection : IEquatable<LightmapArraysCollection>
{
	[CanBeNull]
	public Texture2DArray Color;

	[CanBeNull]
	public Texture2DArray Dir;

	[CanBeNull]
	public Texture2DArray Shadowmask;

	[BurstDiscard]
	public bool Equals(LightmapArraysCollection other)
	{
		if (object.Equals(Color, other.Color) && object.Equals(Dir, other.Dir))
		{
			return object.Equals(Shadowmask, other.Shadowmask);
		}
		return false;
	}

	[BurstDiscard]
	public override bool Equals(object obj)
	{
		if (obj is LightmapArraysCollection other)
		{
			return Equals(other);
		}
		return false;
	}

	[BurstDiscard]
	public override int GetHashCode()
	{
		return HashCode.Combine(Color, Dir, Shadowmask);
	}
}
