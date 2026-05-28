using System;
using System.Buffers;
using System.IO;

namespace OwlPack.Runtime;

public class BinaryOutputArchive<TBufferWriter> : IOutputArchive where TBufferWriter : IMemoryBufferWriter, new()
{
	private BinaryOutputFormatter<TBufferWriter> m_TypesFormatter;

	private BinaryOutputFormatter<TBufferWriter> m_DataFormatter;

	private static readonly ReadOnlyArraySequence HeaderSequence = new ReadOnlyArraySequence(new ReadOnlyArraySequenceSegment(BinaryOutputArchiveConst.Header, 4), 4);

	public void Serialize<T>(ref T obj) where T : IOwlPackable, IOwlPackable<T>
	{
		SerializerState serializerState = new SerializerState();
		m_DataFormatter = new BinaryOutputFormatter<TBufferWriter>();
		m_TypesFormatter = new BinaryOutputFormatter<TBufferWriter>();
		obj.Serialize(m_DataFormatter, serializerState);
		serializerState.TypeLibrary.Serialize(m_TypesFormatter, new SerializerState());
	}

	public void SerializeAny<T>(ref T obj)
	{
		SerializerState serializerState = new SerializerState();
		m_DataFormatter = new BinaryOutputFormatter<TBufferWriter>();
		m_TypesFormatter = new BinaryOutputFormatter<TBufferWriter>();
		if (obj is IOwlPackable owlPackable)
		{
			owlPackable.Serialize(m_DataFormatter, serializerState);
		}
		else
		{
			serializerState.TypeLibrary.GetExternalTypeSerializer(obj.GetType()).Serialize(m_DataFormatter, ref obj, serializerState);
		}
		serializerState.TypeLibrary.Serialize(m_TypesFormatter, new SerializerState());
	}

	private void Write(BinaryWriter writer, ReadOnlyArraySequence data)
	{
		foreach (ReadOnlyArraySequenceSegment datum in data)
		{
			writer.Write(datum.ArraySpan.Array, 0, datum.ArraySpan.UsedLength);
		}
	}

	private void Write(IBufferWriter<byte> writer, ReadOnlyArraySequence data)
	{
		foreach (ReadOnlyArraySequenceSegment datum in data)
		{
			writer.Write(new ReadOnlySpan<byte>(datum.ArraySpan.Array, 0, datum.ArraySpan.UsedLength));
		}
	}

	private void Write(Stream stream, ReadOnlyArraySequence data)
	{
		foreach (ReadOnlyArraySequenceSegment datum in data)
		{
			stream.Write(datum.ArraySpan.Array, 0, datum.ArraySpan.UsedLength);
		}
	}

	public void Write(string filename)
	{
		if (m_TypesFormatter == null)
		{
			throw new Exception("Types were not written into BinaryOutputArchive");
		}
		if (m_DataFormatter == null)
		{
			throw new Exception("Types were not written into BinaryOutputArchive");
		}
		using FileStream output = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write);
		using BinaryWriter writer = new BinaryWriter(output);
		Write(writer, HeaderSequence);
		Write(writer, m_TypesFormatter.WrittenMemory);
		Write(writer, m_DataFormatter.WrittenMemory);
	}

	public void Write(Stream stream)
	{
		if (m_TypesFormatter == null)
		{
			throw new Exception("Types were not written into BinaryOutputArchive");
		}
		if (m_DataFormatter == null)
		{
			throw new Exception("Types were not written into BinaryOutputArchive");
		}
		Write(stream, HeaderSequence);
		Write(stream, m_TypesFormatter.WrittenMemory);
		Write(stream, m_DataFormatter.WrittenMemory);
	}

	public void Write(IBufferWriter<byte> writer)
	{
		if (m_TypesFormatter == null)
		{
			throw new Exception("Types were not written into BinaryOutputArchive");
		}
		if (m_DataFormatter == null)
		{
			throw new Exception("Types were not written into BinaryOutputArchive");
		}
		Write(writer, HeaderSequence);
		Write(writer, m_TypesFormatter.WrittenMemory);
		Write(writer, m_DataFormatter.WrittenMemory);
	}

	public void Write(out byte[] bytes)
	{
		bytes = new byte[m_TypesFormatter.WrittenMemory.TotalLength + m_DataFormatter.WrittenMemory.TotalLength + HeaderSequence.TotalLength];
		MemoryStream stream = new MemoryStream(bytes, writable: true);
		Write(stream);
	}

	public TInputArchive CreateInputArchive<TInputArchive>() where TInputArchive : class, IInputArchive
	{
		MemoryStream memoryStream = new MemoryStream(1024);
		Write(memoryStream);
		memoryStream.Position = 0L;
		return new BinaryInputArchive(memoryStream) as TInputArchive;
	}
}
public class BinaryOutputArchive : BinaryOutputArchive<ArrayMemoryBufferWriter>
{
}
