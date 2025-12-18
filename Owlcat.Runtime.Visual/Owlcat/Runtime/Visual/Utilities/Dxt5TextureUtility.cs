using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Utilities;

internal static class Dxt5TextureUtility
{
	public static void CopyBlocksRepeatPadding(Span<Dxt5Block> src, int2 srcSizeInBlocks, Span<Dxt5Block> dst, int2 dstSizeInBlocks, int2 dstPosInBlocks, int dstPaddingInBlocks)
	{
		CopyBlocks(src, srcSizeInBlocks, default(int2), srcSizeInBlocks, dst, dstSizeInBlocks, dstPosInBlocks + dstPaddingInBlocks);
		if (dstPaddingInBlocks > 0)
		{
			CopyBlocks(src, srcSizeInBlocks, new int2(srcSizeInBlocks.x - dstPaddingInBlocks, 0), new int2(dstPaddingInBlocks, srcSizeInBlocks.y), dst, dstSizeInBlocks, new int2(dstPosInBlocks.x, dstPosInBlocks.y + dstPaddingInBlocks));
			CopyBlocks(src, srcSizeInBlocks, new int2(0, 0), new int2(dstPaddingInBlocks, srcSizeInBlocks.y), dst, dstSizeInBlocks, new int2(dstPosInBlocks.x + dstPaddingInBlocks + srcSizeInBlocks.x, dstPosInBlocks.y + dstPaddingInBlocks));
			CopyBlocks(src, srcSizeInBlocks, new int2(0, srcSizeInBlocks.y - dstPaddingInBlocks), new int2(srcSizeInBlocks.x, dstPaddingInBlocks), dst, dstSizeInBlocks, new int2(dstPosInBlocks.x + dstPaddingInBlocks, dstPosInBlocks.y));
			CopyBlocks(src, srcSizeInBlocks, new int2(0, 0), new int2(srcSizeInBlocks.x, dstPaddingInBlocks), dst, dstSizeInBlocks, new int2(dstPosInBlocks.x + dstPaddingInBlocks, dstPosInBlocks.y + dstPaddingInBlocks + srcSizeInBlocks.y));
			CopyBlocks(src, srcSizeInBlocks, new int2(srcSizeInBlocks.x - dstPaddingInBlocks, srcSizeInBlocks.y - dstPaddingInBlocks), new int2(dstPaddingInBlocks, dstPaddingInBlocks), dst, dstSizeInBlocks, new int2(dstPosInBlocks.x, dstPosInBlocks.y));
			CopyBlocks(src, srcSizeInBlocks, new int2(0, srcSizeInBlocks.y - dstPaddingInBlocks), new int2(dstPaddingInBlocks, dstPaddingInBlocks), dst, dstSizeInBlocks, new int2(dstPosInBlocks.x + dstPaddingInBlocks + srcSizeInBlocks.x, dstPosInBlocks.y));
			CopyBlocks(src, srcSizeInBlocks, new int2(srcSizeInBlocks.x - dstPaddingInBlocks, 0), new int2(dstPaddingInBlocks, dstPaddingInBlocks), dst, dstSizeInBlocks, new int2(dstPosInBlocks.x, dstPosInBlocks.y + dstPaddingInBlocks + srcSizeInBlocks.y));
			CopyBlocks(src, srcSizeInBlocks, new int2(0, 0), new int2(dstPaddingInBlocks, dstPaddingInBlocks), dst, dstSizeInBlocks, new int2(dstPosInBlocks.x + dstPaddingInBlocks + srcSizeInBlocks.x, dstPosInBlocks.y + dstPaddingInBlocks + srcSizeInBlocks.y));
		}
	}

	public unsafe static void CopyBlocks(Span<Dxt5Block> src, int2 srcSizeInBlocks, int2 srcRegionPosInBlocks, int2 srcRegionSizeInBlocks, Span<Dxt5Block> dst, int2 dstSizeInBlocks, int2 dstRegionPosInBlocks)
	{
		int num = srcRegionPosInBlocks.x + srcRegionPosInBlocks.y * srcSizeInBlocks.x;
		int num2 = dstRegionPosInBlocks.x + dstRegionPosInBlocks.y * dstSizeInBlocks.x;
		int sourceStride = 16 * srcSizeInBlocks.x;
		int destinationStride = 16 * dstSizeInBlocks.x;
		int y = srcRegionSizeInBlocks.y;
		int elementSize = 16 * srcRegionSizeInBlocks.x;
		fixed (Dxt5Block* ptr2 = src)
		{
			fixed (Dxt5Block* ptr = dst)
			{
				UnsafeUtility.MemCpyStride(ptr + num2, destinationStride, ptr2 + num, sourceStride, elementSize, y);
			}
		}
	}

	public unsafe static void FillBlocks(Dxt5Block src, Span<Dxt5Block> dst, int2 dstSizeInBlocks, int2 dstRegionPosInBlocks, int2 dstRegionSizeInBlocks)
	{
		int num = dstRegionPosInBlocks.x + dstRegionPosInBlocks.y * dstSizeInBlocks.x;
		int x = dstSizeInBlocks.x;
		int y = dstRegionSizeInBlocks.y;
		fixed (Dxt5Block* ptr = dst)
		{
			for (int i = 0; i < y; i++)
			{
				UnsafeUtility.MemCpyReplicate(ptr + num + x * i, &src, sizeof(Dxt5Block), dstRegionSizeInBlocks.x);
			}
		}
	}
}
