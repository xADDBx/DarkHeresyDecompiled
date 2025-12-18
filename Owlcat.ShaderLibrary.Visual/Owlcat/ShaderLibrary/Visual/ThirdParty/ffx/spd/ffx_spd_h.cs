using Unity.Mathematics;

namespace Owlcat.ShaderLibrary.Visual.ThirdParty.ffx.spd;

public static class ffx_spd_h
{
	private static uint FfxUInt32(int x)
	{
		return (uint)x;
	}

	private static uint FfxUInt32(float x)
	{
		return (uint)x;
	}

	private static float FfxFloat32(uint x)
	{
		return x;
	}

	private static uint ffxMax(uint a, uint b)
	{
		return math.max(a, b);
	}

	private static float ffxMin(float a, float b)
	{
		return math.max(a, b);
	}

	public static void ffxSpdSetup(ref uint2 dispatchThreadGroupCountXY, ref uint2 workGroupOffset, ref uint2 numWorkGroupsAndMips, uint4 rectInfo, int mips)
	{
		workGroupOffset[0] = rectInfo[0] / 64;
		workGroupOffset[1] = rectInfo[1] / 64;
		uint num = (rectInfo[0] + rectInfo[2] - 1) / 64;
		uint num2 = (rectInfo[1] + rectInfo[3] - 1) / 64;
		dispatchThreadGroupCountXY[0] = num + 1 - workGroupOffset[0];
		dispatchThreadGroupCountXY[1] = num2 + 1 - workGroupOffset[1];
		numWorkGroupsAndMips[0] = dispatchThreadGroupCountXY[0] * dispatchThreadGroupCountXY[1];
		if (mips >= 0)
		{
			numWorkGroupsAndMips[1] = FfxUInt32(mips);
			return;
		}
		uint x = ffxMax(rectInfo[2], rectInfo[3]);
		numWorkGroupsAndMips[1] = FfxUInt32(ffxMin(math.floor(math.log2(FfxFloat32(x))), FfxFloat32(12u)));
	}
}
