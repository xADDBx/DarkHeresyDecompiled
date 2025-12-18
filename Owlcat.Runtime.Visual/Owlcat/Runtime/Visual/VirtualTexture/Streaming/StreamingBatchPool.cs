using System;
using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTexture.Streaming;

public class StreamingBatchPool : IDisposable
{
	private Dictionary<TilesInBatchLimit, Stack<StreamingBatch>> m_Pool = new Dictionary<TilesInBatchLimit, Stack<StreamingBatch>>();

	public void Dispose()
	{
		foreach (KeyValuePair<TilesInBatchLimit, Stack<StreamingBatch>> item in m_Pool)
		{
			foreach (StreamingBatch item2 in item.Value)
			{
				item2.Dispose();
			}
		}
	}

	public StreamingBatch Get(TilesInBatchLimit limit)
	{
		if (!m_Pool.TryGetValue(limit, out var value))
		{
			value = new Stack<StreamingBatch>();
			m_Pool.Add(limit, value);
		}
		if (value.Count == 0)
		{
			value.Push(new StreamingBatch(limit));
		}
		return value.Pop();
	}

	public void Release(StreamingBatch batch)
	{
		if (m_Pool.TryGetValue(batch.Limit, out var value))
		{
			batch.Tasks.Clear();
			value.Push(batch);
		}
		else
		{
			Debug.LogError(string.Format("You are trying to release {0}, but there is no batches of {1} size.", "StreamingBatch", batch.SizeInBytes));
		}
	}
}
