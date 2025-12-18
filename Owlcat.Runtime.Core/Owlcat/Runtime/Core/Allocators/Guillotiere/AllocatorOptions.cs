using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Allocators.Guillotiere;

public struct AllocatorOptions
{
	public static readonly AllocatorOptions DefaultOptions = new AllocatorOptions
	{
		Alignment = new int2(1, 1),
		LargeSizeThreshold = 256,
		SmallSizeThreshold = 32
	};

	public int2 Alignment;

	public int SmallSizeThreshold;

	public int LargeSizeThreshold;
}
