namespace Owlcat.Runtime.Core.Math;

public static class Alignment
{
	public static int AlignUp(int value, int alignment)
	{
		if (alignment == 0)
		{
			return value;
		}
		return (value + alignment - 1) & -alignment;
	}
}
