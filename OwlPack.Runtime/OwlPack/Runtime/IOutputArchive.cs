using System.Buffers;
using System.IO;

namespace OwlPack.Runtime;

public interface IOutputArchive
{
	void Serialize<T>(ref T obj) where T : IOwlPackable, IOwlPackable<T>;

	void SerializeAny<T>(ref T obj);

	void Write(string filename);

	void Write(Stream stream);

	void Write(IBufferWriter<byte> writer);

	TInputArchive CreateInputArchive<TInputArchive>() where TInputArchive : class, IInputArchive;
}
