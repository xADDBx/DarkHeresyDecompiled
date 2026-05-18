using System;
using ExitGames.Client.Photon;
using MemoryPack;

namespace Kingmaker.Networking;

public static class NetMessageSerializer
{
	private static CustomArrayBufferWriter<byte> MemoryPackBuffer => MessageNetManager.SendBytes;

	public static ByteArraySlice SerializeToSlice<T>(T value)
	{
		MemoryPackBuffer.Clear();
		MemoryPackSerializer.ResetWriterState();
		try
		{
			CustomArrayBufferWriter<byte> bufferWriter = MemoryPackBuffer;
			MemoryPackSerializer.Serialize(in bufferWriter, in value);
		}
		catch (Exception ex)
		{
			PFLog.Net.Exception(ex);
		}
		return PhotonManager.Instance.ByteArraySlicePool.Acquire(MemoryPackBuffer.GetArray(), 0, MemoryPackBuffer.WrittenCount);
	}

	public static T DeserializeFromSlice<T>(ByteArraySlice slice)
	{
		return DeserializeFromSpan<T>(new ReadOnlySpan<byte>(slice.Buffer, slice.Offset, slice.Count));
	}

	public static T DeserializeFromSpan<T>(ReadOnlySpan<byte> span)
	{
		T result = default(T);
		try
		{
			MemoryPackSerializer.ResetReaderState();
			result = MemoryPackSerializer.Deserialize<T>(span);
			return result;
		}
		catch (Exception ex)
		{
			PFLog.Net.Exception(ex);
		}
		return result;
	}
}
