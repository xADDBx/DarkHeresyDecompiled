using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OwlPack.Runtime;

public static class IBufferWriterExtensions
{
	public unsafe static void Write<T>(this IBufferWriter<byte> writer, T value) where T : unmanaged
	{
		Span<byte> span = writer.GetSpan(sizeof(T));
		if (span.Length < sizeof(T))
		{
			throw new Exception("End of buffer");
		}
		Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), value);
		writer.Advance(sizeof(T));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Write<T>(this IBufferWriter<T> writer, T value)
	{
		Span<T> span = writer.GetSpan(1);
		if (span.Length < 1)
		{
			throw new Exception("End of buffer");
		}
		MemoryMarshal.GetReference(span) = value;
		writer.Advance(1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Write<T>(this IBufferWriter<byte> writer, ReadOnlySpan<T> span) where T : unmanaged
	{
		ReadOnlySpan<byte> readOnlySpan = MemoryMarshal.AsBytes(span);
		Span<byte> span2 = writer.GetSpan(readOnlySpan.Length);
		readOnlySpan.CopyTo(span2);
		writer.Advance(readOnlySpan.Length);
	}
}
