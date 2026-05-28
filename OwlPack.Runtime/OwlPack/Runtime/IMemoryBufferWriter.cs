using System.Buffers;

namespace OwlPack.Runtime;

public interface IMemoryBufferWriter : IBufferWriter<byte>
{
	ReadOnlyArraySequence WrittenMemory { get; }

	int Capacity { get; }

	void Reset();
}
