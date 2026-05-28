namespace OwlPack.Runtime;

public readonly ref struct ReadOnlyArraySpan
{
	private readonly byte[] m_Array;

	private readonly int m_UsedLength;

	public byte[] Array => m_Array;

	public int UsedLength => m_UsedLength;

	public ReadOnlyArraySpan(byte[] array, int length)
	{
		m_Array = array;
		m_UsedLength = length;
	}
}
