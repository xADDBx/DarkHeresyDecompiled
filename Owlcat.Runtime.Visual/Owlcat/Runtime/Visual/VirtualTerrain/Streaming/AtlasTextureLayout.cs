using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTerrain.Streaming;

internal sealed class AtlasTextureLayout
{
	public struct LodLayout
	{
		public int2 RegionPosition;

		public int SlotSize;

		public int SlotCountX;

		public int SlotCountY;

		public Vector4[] Redirections;
	}

	private const int kSlotsPerColumnAtLevel0 = 4;

	public readonly int LayerTextureSize;

	public readonly int2 AtlasTextureSize;

	public readonly float2 AtlasTextureSizeRcp;

	public readonly int AtlasTextureMipCount;

	public readonly LodLayout[] LodLayouts;

	public AtlasTextureLayout(int textureSize, int capacityLod0, int capacityLod1, int capacityLod2)
	{
		LayerTextureSize = textureSize;
		AtlasTextureSize.y = (textureSize + 32) * 4;
		AtlasTextureSize.x = 0;
		AtlasTextureMipCount = 3;
		LodLayouts = new LodLayout[3];
		for (int num = 2; num >= 0; num--)
		{
			ref LodLayout reference = ref LodLayouts[num];
			int num2 = num * 2;
			reference.RegionPosition = new int2(AtlasTextureSize.x, 0);
			reference.SlotSize = (textureSize >> num2) + 32;
			reference.SlotCountY = AtlasTextureSize.y / reference.SlotSize;
			reference.SlotCountX = (GetCapacity(num) + (reference.SlotCountY - 1)) / reference.SlotCountY;
			reference.Redirections = new Vector4[reference.SlotCountX * reference.SlotCountY];
			AtlasTextureSize.x += reference.SlotCountX * reference.SlotSize;
		}
		AtlasTextureSizeRcp = new float2(1f) / AtlasTextureSize;
		for (int i = 0; i < 3; i++)
		{
			ref LodLayout reference2 = ref LodLayouts[i];
			for (int j = 0; j < reference2.SlotCountY; j++)
			{
				for (int k = 0; k < reference2.SlotCountX; k++)
				{
					int num3 = k + j * reference2.SlotCountX;
					reference2.Redirections[num3] = MakeRedirectionMad(reference2.SlotSize - 32, AtlasTextureSizeRcp, reference2.RegionPosition + new int2(k * reference2.SlotSize, j * reference2.SlotSize));
				}
			}
		}
		int GetCapacity(int pageLod)
		{
			return pageLod switch
			{
				0 => capacityLod0, 
				1 => capacityLod1, 
				2 => capacityLod2, 
				_ => capacityLod2, 
			};
		}
	}

	public int GetSlotCount(int pageLod)
	{
		ref LodLayout reference = ref LodLayouts[pageLod];
		return reference.SlotCountX * reference.SlotCountY;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int2 GetSlotPosition(int pageLod, int slotId)
	{
		ref LodLayout reference = ref LodLayouts[pageLod];
		return reference.RegionPosition + reference.SlotSize * new int2(slotId % reference.SlotCountX, slotId / reference.SlotCountX);
	}

	public Vector4 GetRedirection(int pageLod, int slotId)
	{
		int num = pageLod * 2;
		return MakeRedirectionMad(LayerTextureSize >> num, slotPosition: GetSlotPosition(pageLod, slotId), atlasTextureSizeRcp: AtlasTextureSizeRcp);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Vector4 MakeRedirectionMad(int slotSizeWithoutPadding, float2 atlasTextureSizeRcp, int2 slotPosition)
	{
		float2 @float = slotSizeWithoutPadding * atlasTextureSizeRcp;
		float2 float2 = (slotPosition + 16) * atlasTextureSizeRcp;
		return new Vector4(@float.x, @float.y, float2.x, float2.y);
	}
}
