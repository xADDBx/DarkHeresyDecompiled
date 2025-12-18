using System.Runtime.CompilerServices;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.LOD;

internal static class GPUDrivenInstanceLODInfo
{
	public const uint kInvalid = uint.MaxValue;

	private const int kMaskBits = 8;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsValid(uint lodInfo)
	{
		return lodInfo != uint.MaxValue;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint Pack(int lodGroupIndex, byte lodMask)
	{
		return (uint)((lodGroupIndex << 8) | lodMask);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Unpack(uint lodInfo, out int lodGroupIndex, out uint lodMask)
	{
		lodGroupIndex = (int)(lodInfo >> 8);
		lodMask = lodInfo & 0xFFu;
	}
}
