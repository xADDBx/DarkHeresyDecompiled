using System;
using System.IO;

namespace OwlPack.Runtime;

public class BinaryInputArchive : IInputArchive, ITestInputArchive
{
	protected BinaryReader m_Reader;

	private byte m_Version;

	public Action<TypeLibrary> OnTypeLibraryDeserialize { get; set; }

	public BinaryInputArchive(Stream data)
	{
		m_Reader = new BinaryReader(data);
		byte[] array = m_Reader.ReadBytes(BinaryOutputArchiveConst.Header.Length);
		for (int i = 0; i < 3; i++)
		{
			if (BinaryOutputArchiveConst.Header[i] != array[i])
			{
				throw new InvalidDataException("Expecting OWL header in binary archive.");
			}
		}
		m_Version = array[3];
		if (m_Version > 3)
		{
			throw new InvalidDataException($"Serialized version {m_Version} is newer than current version {(byte)3}");
		}
	}

	public BinaryInputArchive(byte[] data)
	{
		m_Reader = new BinaryReader(new MemoryStream(data));
		byte[] array = m_Reader.ReadBytes(BinaryOutputArchiveConst.Header.Length);
		for (int i = 0; i < 3; i++)
		{
			if (BinaryOutputArchiveConst.Header[i] != array[i])
			{
				throw new InvalidDataException("Expecting OWL header in binary archive.");
			}
		}
		m_Version = array[3];
		if (m_Version > 3)
		{
			throw new InvalidDataException($"Serialized version {m_Version} is newer than current version {(byte)3}");
		}
	}

	public T Deserialize<T>()
	{
		if (m_Reader.BaseStream.Position != BinaryOutputArchiveConst.Header.Length)
		{
			m_Reader.BaseStream.Seek(BinaryOutputArchiveConst.Header.Length, SeekOrigin.Begin);
		}
		BinaryInputFormatter formatter = new BinaryInputFormatter(m_Reader);
		TypeLibrary typeLibrary = Serializer.DeserializeObject<TypeLibrary>(formatter, new DeserializerState(m_Version, new TypeLibrary()));
		typeLibrary.UpdateWithOldNames(TypeLibrary.OldNames);
		if (OnTypeLibraryDeserialize != null)
		{
			OnTypeLibraryDeserialize(typeLibrary);
		}
		return Serializer.DeserializeObject<T>(formatter, new DeserializerState(m_Version, typeLibrary));
	}
}
