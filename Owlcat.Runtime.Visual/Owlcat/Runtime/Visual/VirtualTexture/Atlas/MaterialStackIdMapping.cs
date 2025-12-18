using System;
using Unity.Burst;

namespace Owlcat.Runtime.Visual.VirtualTexture.Atlas;

[BurstCompile]
public struct MaterialStackIdMapping : IEquatable<MaterialStackIdMapping>
{
	public int MaterialId;

	public int IndexInMaterial;

	public TextureStackId StackId;

	public override bool Equals(object obj)
	{
		if (obj is MaterialStackIdMapping materialStackIdMapping && MaterialId == materialStackIdMapping.MaterialId && IndexInMaterial == materialStackIdMapping.IndexInMaterial)
		{
			return StackId.Equals(materialStackIdMapping.StackId);
		}
		return false;
	}

	public bool Equals(MaterialStackIdMapping other)
	{
		if (MaterialId == other.MaterialId && IndexInMaterial == other.IndexInMaterial)
		{
			return StackId.Equals(other.StackId);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(MaterialId, IndexInMaterial, StackId);
	}
}
