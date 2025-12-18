using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Allocators;

public struct BuddyAllocation
{
	public int Level;

	public int Index;

	public uint2 index2D => SpaceFillingCurves.DecodeMorton2D((uint)Index);

	public BuddyAllocation(int level, int index)
	{
		Level = level;
		Index = index;
	}
}
