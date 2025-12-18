namespace Owlcat.Runtime.Visual.XPBD.SoA;

public abstract class StructureOfArrays<T> : StructureOfArraysBase where T : struct
{
	public abstract T this[int index] { get; set; }

	public StructureOfArrays(int size)
		: base(size)
	{
	}
}
