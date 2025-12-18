using System;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Utilities;

public struct IndexSegment : IComparable<IndexSegment>
{
	public int From;

	public int ToExclusive;

	public bool IsValid()
	{
		return From < ToExclusive;
	}

	public int CompareTo(IndexSegment other)
	{
		return From.CompareTo(other.From);
	}
}
