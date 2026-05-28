using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Text.Json;

namespace OwlPack.Runtime;

public class JsonOutputArchive<TBufferWriter> : IOutputArchive where TBufferWriter : IMemoryBufferWriter, new()
{
	private TBufferWriter m_TypesBufferWriter;

	private TBufferWriter m_DataBufferWriter;

	private JsonOutputArchiveOptions m_Options;

	public JsonOutputArchive()
	{
	}

	public JsonOutputArchive(JsonOutputArchiveOptions options)
	{
		m_Options = options;
	}

	public void Serialize<T>(ref T obj) where T : IOwlPackable, IOwlPackable<T>
	{
		m_TypesBufferWriter = new TBufferWriter();
		m_DataBufferWriter = new TBufferWriter();
		SerializerState serializerState = new SerializerState();
		using Utf8JsonWriter writer2 = new Utf8JsonWriter(m_TypesBufferWriter, new JsonWriterOptions
		{
			Indented = m_Options.PrettyPrint
		});
		using Utf8JsonWriter writer = new Utf8JsonWriter(m_DataBufferWriter, new JsonWriterOptions
		{
			Indented = m_Options.PrettyPrint
		});
		JsonOutputFormatter formatter = new JsonOutputFormatter(writer);
		JsonOutputFormatter formatter2 = new JsonOutputFormatter(writer2);
		obj.Serialize(formatter, serializerState);
		serializerState.TypeLibrary.Serialize(formatter2, new SerializerState());
	}

	public void SerializeAny<T>(ref T obj)
	{
		m_TypesBufferWriter = new TBufferWriter();
		m_DataBufferWriter = new TBufferWriter();
		SerializerState serializerState = new SerializerState();
		using Utf8JsonWriter writer2 = new Utf8JsonWriter(m_TypesBufferWriter, new JsonWriterOptions
		{
			Indented = m_Options.PrettyPrint
		});
		using Utf8JsonWriter writer = new Utf8JsonWriter(m_DataBufferWriter, new JsonWriterOptions
		{
			Indented = m_Options.PrettyPrint
		});
		JsonOutputFormatter formatter = new JsonOutputFormatter(writer);
		JsonOutputFormatter formatter2 = new JsonOutputFormatter(writer2);
		if (obj is IOwlPackable owlPackable)
		{
			owlPackable.Serialize(formatter, serializerState);
		}
		else
		{
			serializerState.TypeLibrary.GetExternalTypeSerializer(obj.GetType()).Serialize(formatter, ref obj, serializerState);
		}
		serializerState.TypeLibrary.Serialize(formatter2, new SerializerState());
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
		if (m_TypesBufferWriter == null)
		{
			throw new Exception("Types were not written into JsonOutputArchive");
		}
		if (m_DataBufferWriter == null)
		{
			throw new Exception("Types were not written into JsonOutputArchive");
		}
		using FileStream output = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write);
		using BinaryWriter binaryWriter = new BinaryWriter(output);
		byte[] bytes = Encoding.UTF8.GetBytes(((byte)3).ToString());
		binaryWriter.Write('[');
		binaryWriter.Write(bytes);
		binaryWriter.Write(',');
		Write(binaryWriter, m_TypesBufferWriter.WrittenMemory);
		binaryWriter.Write(',');
		Write(binaryWriter, m_DataBufferWriter.WrittenMemory);
		binaryWriter.Write(']');
	}

	public void Write(Stream stream)
	{
		if (m_TypesBufferWriter == null)
		{
			throw new Exception("Types were not written into JsonOutputArchive");
		}
		if (m_DataBufferWriter == null)
		{
			throw new Exception("Types were not written into JsonOutputArchive");
		}
		byte[] bytes = Encoding.UTF8.GetBytes(((byte)3).ToString());
		stream.WriteByte(91);
		stream.Write(bytes, 0, bytes.Length);
		stream.WriteByte(44);
		Write(stream, m_TypesBufferWriter.WrittenMemory);
		stream.WriteByte(44);
		Write(stream, m_DataBufferWriter.WrittenMemory);
		stream.WriteByte(93);
	}

	public void Write(IBufferWriter<byte> writer)
	{
		if (m_TypesBufferWriter == null)
		{
			throw new Exception("Types were not written into JsonOutputArchive");
		}
		if (m_DataBufferWriter == null)
		{
			throw new Exception("Types were not written into JsonOutputArchive");
		}
		byte[] bytes = Encoding.UTF8.GetBytes(((byte)3).ToString());
		writer.Write('[');
		BuffersExtensions.Write(writer, bytes);
		writer.Write(',');
		Write(writer, m_TypesBufferWriter.WrittenMemory);
		writer.Write(',');
		Write(writer, m_DataBufferWriter.WrittenMemory);
		writer.Write(']');
	}

	public void Write(out string s)
	{
		if (m_TypesBufferWriter == null)
		{
			throw new Exception("Types were not written into JsonOutputArchive");
		}
		if (m_DataBufferWriter == null)
		{
			throw new Exception("Types were not written into JsonOutputArchive");
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('[');
		stringBuilder.Append((byte)3);
		stringBuilder.Append(',');
		foreach (ReadOnlyArraySequenceSegment item in m_TypesBufferWriter.WrittenMemory)
		{
			stringBuilder.Append(Encoding.UTF8.GetChars(item.Span.ToArray()));
		}
		stringBuilder.Append(',');
		foreach (ReadOnlyArraySequenceSegment item2 in m_DataBufferWriter.WrittenMemory)
		{
			stringBuilder.Append(Encoding.UTF8.GetChars(item2.Span.ToArray()));
		}
		stringBuilder.Append(']');
		s = stringBuilder.ToString();
	}

	public void Write(out byte[] bytes)
	{
		if (m_TypesBufferWriter == null)
		{
			throw new Exception("Types were not written into JsonOutputArchive");
		}
		if (m_DataBufferWriter == null)
		{
			throw new Exception("Types were not written into JsonOutputArchive");
		}
		bytes = new byte[m_TypesBufferWriter.WrittenMemory.TotalLength + m_DataBufferWriter.WrittenMemory.TotalLength + 128];
		MemoryStream stream = new MemoryStream(bytes, writable: true);
		Write(stream);
	}

	public TInputArchive CreateInputArchive<TInputArchive>() where TInputArchive : class, IInputArchive
	{
		MemoryStream memoryStream = new MemoryStream(1024);
		Write(memoryStream);
		memoryStream.Position = 0L;
		return new JsonInputArchive(memoryStream) as TInputArchive;
	}
}
public class JsonOutputArchive : JsonOutputArchive<ArrayMemoryBufferWriter>
{
	public JsonOutputArchive()
	{
	}

	public JsonOutputArchive(JsonOutputArchiveOptions options)
		: base(options)
	{
	}
}
