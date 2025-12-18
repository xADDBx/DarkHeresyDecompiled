using System;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.VirtualTexture.Streaming;

public class StreamingBatch : IDisposable
{
	private TilesInBatchLimit m_Limit;

	public int LastUsingFrameId;

	public NativeArray<byte> RawData;

	public NativeList<TileReadTask> Tasks;

	public int SizeInBytes => RawData.Length;

	public TilesInBatchLimit Limit => m_Limit;

	public StreamingBatch(TilesInBatchLimit limit)
	{
		m_Limit = limit;
		int length = 25920 * (int)m_Limit;
		RawData = new NativeArray<byte>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Tasks = new NativeList<TileReadTask>((int)m_Limit, Allocator.Persistent);
	}

	public void Dispose()
	{
		if (RawData.IsCreated)
		{
			RawData.Dispose();
		}
		if (Tasks.IsCreated)
		{
			Tasks.Dispose();
		}
	}
}
