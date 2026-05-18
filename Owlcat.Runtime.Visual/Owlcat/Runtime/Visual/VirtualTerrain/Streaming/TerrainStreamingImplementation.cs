using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IO.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTerrain.Streaming;

internal sealed class TerrainStreamingImplementation : IDisposable
{
	private ref struct RewritableAtlasSlotEnumerator
	{
		private readonly int m_Lod;

		private readonly Span<int> m_AtlasSlotUploadedLayerIds;

		private readonly Span<int> m_LayerLods;

		private int m_AtlasSlotId;

		private bool m_UntouchedOnly;

		public int Current => m_AtlasSlotId;

		public RewritableAtlasSlotEnumerator(int lod, Span<int> atlasSlotUploadedLayerIds, Span<int> layerLods)
		{
			m_Lod = lod;
			m_AtlasSlotUploadedLayerIds = atlasSlotUploadedLayerIds;
			m_LayerLods = layerLods;
			m_AtlasSlotId = -1;
			m_UntouchedOnly = true;
		}

		public bool MoveNext()
		{
			if (m_UntouchedOnly)
			{
				m_AtlasSlotId++;
				while (m_AtlasSlotId < m_AtlasSlotUploadedLayerIds.Length)
				{
					if (m_AtlasSlotUploadedLayerIds[m_AtlasSlotId] == -1)
					{
						return true;
					}
					m_AtlasSlotId++;
				}
				m_UntouchedOnly = false;
				m_AtlasSlotId = -1;
			}
			if (!m_UntouchedOnly)
			{
				m_AtlasSlotId++;
				while (m_AtlasSlotId < m_AtlasSlotUploadedLayerIds.Length)
				{
					int num = m_AtlasSlotUploadedLayerIds[m_AtlasSlotId];
					if (num != -1 && m_LayerLods[num] > m_Lod)
					{
						return true;
					}
					m_AtlasSlotId++;
				}
			}
			return false;
		}
	}

	private ref struct UploadableLayerEnumerator
	{
		private readonly int m_Lod;

		private readonly Span<int> m_LayerLods;

		private readonly Span<int> m_LayerAtlasSlotIds;

		private int m_LayerId;

		public int Current => m_LayerId;

		public UploadableLayerEnumerator(int lod, Span<int> layerLods, Span<int> layerAtlasSlotIds)
		{
			m_Lod = lod;
			m_LayerLods = layerLods;
			m_LayerAtlasSlotIds = layerAtlasSlotIds;
			m_LayerId = -1;
		}

		public bool MoveNext()
		{
			m_LayerId++;
			while (m_LayerId < m_LayerLods.Length)
			{
				if (m_LayerAtlasSlotIds[m_LayerId] == -1 && m_LayerLods[m_LayerId] <= m_Lod)
				{
					return true;
				}
				m_LayerId++;
			}
			return false;
		}
	}

	private const int kInvalidIndex = -1;

	private const int kInvalidLod = int.MaxValue;

	private readonly List<IStreamingListener> m_Listeners;

	private DatabaseLayout m_DatabaseLayout;

	private DatabaseLayerMapping m_DatabaseLayerMapping;

	private FileHandle m_DatabaseFileHandle;

	private UploadTextureLayout m_UploadTextureLayout;

	private AtlasTextureLayout m_AtlasTextureLayout;

	private NativeList<int> m_LayerLods;

	private NativeList<int>[] m_LayerAtlasSlotIds;

	private NativeList<float4>[] m_LayerRedirections;

	private NativeArray<int>[] m_AtlasSlotUploadedLayerIds;

	private bool m_Reading;

	private ReadHandle m_ReadHandle;

	private NativeList<ReadCommand> m_ReadCommands;

	private NativeList<int> m_ReadLayerIds;

	private int m_ReadLayerLod = int.MaxValue;

	private UploadTexture m_UploadTexture;

	private AtlasTextures m_AtlasTextures;

	private Uploader m_Uploader;

	public TerrainStreamingImplementation(VirtualTerrainSettings settings, List<IStreamingListener> listeners, string databasePath)
	{
		m_Listeners = listeners;
		m_DatabaseLayout = DatabaseUtility.ReadIndexFromFile(databasePath);
		m_DatabaseLayerMapping = new DatabaseLayerMapping(m_DatabaseLayout.TerrainLayerGuids);
		m_DatabaseFileHandle = AsyncReadManager.OpenFileAsync(databasePath);
		m_DatabaseFileHandle.JobHandle.Complete();
		m_UploadTextureLayout = new UploadTextureLayout(m_DatabaseLayout.TextureSize);
		m_AtlasTextureLayout = new AtlasTextureLayout(m_DatabaseLayout.TextureSize, settings.AtlasCapacityLod0, settings.AtlasCapacityLod1, settings.AtlasCapacityLod2);
		m_LayerLods = new NativeList<int>(Allocator.Persistent);
		m_LayerAtlasSlotIds = new NativeList<int>[3];
		m_AtlasSlotUploadedLayerIds = new NativeArray<int>[3];
		for (int i = 0; i < 3; i++)
		{
			m_LayerAtlasSlotIds[i] = new NativeList<int>(64, Allocator.Persistent);
			m_AtlasSlotUploadedLayerIds[i] = new NativeArray<int>(m_AtlasTextureLayout.GetSlotCount(i), Allocator.Persistent);
			m_AtlasSlotUploadedLayerIds[i].AsSpan().Fill(-1);
		}
		m_ReadCommands = new NativeList<ReadCommand>(Allocator.Persistent);
		m_ReadLayerIds = new NativeList<int>(Allocator.Persistent);
		m_UploadTexture = new UploadTexture(m_UploadTextureLayout);
		m_AtlasTextures = new AtlasTextures(m_AtlasTextureLayout);
		m_Uploader = new Uploader(m_UploadTextureLayout, m_AtlasTextureLayout, m_UploadTexture, m_AtlasTextures);
		int value = 0;
		int value2 = 0;
		int num = 2;
		m_LayerAtlasSlotIds[num].Add(in value2);
		m_AtlasSlotUploadedLayerIds[num][value2] = value;
	}

	public void Dispose()
	{
		if (m_Reading)
		{
			m_ReadHandle.JobHandle.Complete();
		}
		m_DatabaseFileHandle.Close().Complete();
		m_LayerLods.Dispose();
		for (int i = 0; i < 3; i++)
		{
			m_LayerAtlasSlotIds[i].Dispose();
			m_AtlasSlotUploadedLayerIds[i].Dispose();
		}
		m_ReadCommands.Dispose();
		m_ReadLayerIds.Dispose();
		m_UploadTexture.Dispose();
		m_AtlasTextures.Dispose();
		m_Uploader.Dispose();
	}

	public void Update()
	{
		if (m_Reading)
		{
			if (m_ReadHandle.Status != ReadStatus.InProgress)
			{
				EnsureCapacity();
				UpdateLayerLods();
				FinishReading();
				TryStartReading();
				NotifyAtlasChanged();
			}
		}
		else
		{
			EnsureCapacity();
			UpdateLayerLods();
			TryStartReading();
		}
	}

	private void EnsureCapacity()
	{
		int terrainLayerCount = TerrainLayerId.GetTerrainLayerCount();
		int length = m_LayerLods.Length;
		NativeArray<int> nativeArray;
		Span<int> span;
		if (m_LayerLods.Length < terrainLayerCount)
		{
			m_LayerLods.Resize(terrainLayerCount, NativeArrayOptions.UninitializedMemory);
			nativeArray = m_LayerLods.AsArray();
			span = nativeArray.AsSpan();
			span = span.Slice(length, terrainLayerCount - length);
			span.Fill(int.MaxValue);
		}
		for (int i = 0; i < 3; i++)
		{
			int length2 = m_LayerAtlasSlotIds[i].Length;
			m_LayerAtlasSlotIds[i].Resize(terrainLayerCount, NativeArrayOptions.UninitializedMemory);
			nativeArray = m_LayerAtlasSlotIds[i].AsArray();
			span = nativeArray.AsSpan();
			span = span.Slice(length2, terrainLayerCount - length2);
			span.Fill(-1);
		}
	}

	private void UpdateLayerLods()
	{
		Span<int> layerLods = m_LayerLods.AsArray().AsSpan();
		layerLods.Fill(int.MaxValue);
		TerrainStreamingFeedback.GetFeedback(layerLods);
		layerLods[0] = 2;
	}

	private void TryStartReading()
	{
		int num = 2;
		while (num >= 0 && !TryStartReading(num))
		{
			num--;
		}
	}

	private unsafe bool TryStartReading(int lod)
	{
		NativeArray<int> nativeArray = m_LayerLods.AsArray();
		Span<int> layerLods = nativeArray.AsSpan();
		nativeArray = m_LayerAtlasSlotIds[lod].AsArray();
		Span<int> layerAtlasSlotIds = nativeArray.AsSpan();
		Span<int> atlasSlotUploadedLayerIds = m_AtlasSlotUploadedLayerIds[lod].AsSpan();
		int layerCapacity = m_UploadTextureLayout.GetLayerCapacity(lod);
		RewritableAtlasSlotEnumerator rewritableAtlasSlotEnumerator = new RewritableAtlasSlotEnumerator(lod, atlasSlotUploadedLayerIds, layerLods);
		UploadableLayerEnumerator uploadableLayerEnumerator = new UploadableLayerEnumerator(lod, layerLods, layerAtlasSlotIds);
		while (rewritableAtlasSlotEnumerator.MoveNext() && uploadableLayerEnumerator.MoveNext() && m_ReadLayerIds.Length < layerCapacity)
		{
			int value = uploadableLayerEnumerator.Current;
			if (m_DatabaseLayerMapping.TryGetDatabaseLayerIndex(value, out var _))
			{
				m_ReadLayerIds.Add(in value);
			}
		}
		if (m_ReadLayerIds.Length == 0)
		{
			return false;
		}
		m_UploadTexture.Setup(lod);
		long slotSizeInBytes = m_UploadTextureLayout.GetSlotSizeInBytes(lod);
		NativeArray<ulong> buffer = m_UploadTexture.GetBuffer();
		byte* unsafePtr = (byte*)buffer.GetUnsafePtr();
		_ = buffer.Length;
		for (int i = 0; i < 3; i++)
		{
			byte* ptr = unsafePtr + m_UploadTexture.GetBufferOffsetForMipLevel(i);
			long num = slotSizeInBytes >> 2 * i;
			foreach (int readLayerId in m_ReadLayerIds)
			{
				m_DatabaseLayerMapping.TryGetDatabaseLayerIndex(readLayerId, out var result2);
				for (int j = 0; j < 3; j++)
				{
					long address = m_DatabaseLayout.GetAddress(result2, lod, i, j);
					ref NativeList<ReadCommand> readCommands = ref m_ReadCommands;
					ReadCommand value2 = new ReadCommand
					{
						Buffer = ptr,
						Offset = address,
						Size = num
					};
					readCommands.Add(in value2);
					ptr += num;
				}
			}
		}
		foreach (ReadCommand readCommand in m_ReadCommands)
		{
			_ = readCommand;
		}
		m_Reading = true;
		m_ReadLayerLod = lod;
		m_ReadHandle = AsyncReadManager.Read(in m_DatabaseFileHandle, new ReadCommandArray
		{
			ReadCommands = m_ReadCommands.GetUnsafePtr(),
			CommandCount = m_ReadCommands.Length
		});
		return true;
	}

	private unsafe void FinishReading()
	{
		int readLayerLod = m_ReadLayerLod;
		NativeArray<int> nativeArray = m_LayerLods.AsArray();
		Span<int> layerLods = nativeArray.AsSpan();
		nativeArray = m_LayerAtlasSlotIds[readLayerLod].AsArray();
		Span<int> span = nativeArray.AsSpan();
		Span<int> atlasSlotUploadedLayerIds = m_AtlasSlotUploadedLayerIds[readLayerLod].AsSpan();
		RewritableAtlasSlotEnumerator rewritableAtlasSlotEnumerator = new RewritableAtlasSlotEnumerator(readLayerLod, atlasSlotUploadedLayerIds, layerLods);
		ulong* bytesReadArray = m_ReadHandle.GetBytesReadArray();
		int num = 9;
		for (int i = 0; i < m_ReadLayerIds.Length; i++)
		{
			int num2 = m_ReadLayerIds[i];
			bool flag = false;
			for (int j = 0; j < num; j++)
			{
				int num3 = i * num + j;
				if (bytesReadArray[num3] != (ulong)m_ReadCommands[num3].Size)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (!rewritableAtlasSlotEnumerator.MoveNext())
				{
					break;
				}
				int current = rewritableAtlasSlotEnumerator.Current;
				if (atlasSlotUploadedLayerIds[current] >= 0)
				{
					span[atlasSlotUploadedLayerIds[current]] = -1;
				}
				atlasSlotUploadedLayerIds[current] = num2;
				span[num2] = current;
				m_Uploader.EnqueueUpload(i * 3, current, m_ReadLayerLod);
			}
		}
		m_Uploader.FlushUpload();
		m_Reading = false;
		m_ReadHandle.Dispose();
		m_ReadCommands.Clear();
		m_ReadLayerIds.Clear();
		m_ReadLayerLod = int.MaxValue;
	}

	private void NotifyAtlasChanged()
	{
		foreach (IStreamingListener listener in m_Listeners)
		{
			listener.OnAtlasChanged();
		}
	}

	public void PopulateRedirectionBuffer(List<int> layerIds, Span<Vector4> buffer, int bufferLodStride)
	{
		int num = Mathf.Min(layerIds.Count, bufferLodStride);
		for (int i = 0; i < num; i++)
		{
			int num2 = layerIds[i];
			if (num2 >= m_LayerAtlasSlotIds[0].Length)
			{
				for (int j = 0; j < 3; j++)
				{
					buffer[j * bufferLodStride + i] = default(Vector4);
				}
				continue;
			}
			Vector4 vector = default(Vector4);
			int k = 0;
			do
			{
				int num3 = m_LayerAtlasSlotIds[k][num2];
				if (num3 != -1)
				{
					vector = m_AtlasTextureLayout.LodLayouts[k].Redirections[num3];
					k++;
					break;
				}
				k++;
			}
			while (k < 3);
			for (int l = 0; l < k; l++)
			{
				buffer[l * bufferLodStride + i] = vector;
			}
			for (; k < 3; k++)
			{
				int num4 = m_LayerAtlasSlotIds[k][num2];
				if (num4 != -1)
				{
					vector = m_AtlasTextureLayout.LodLayouts[k].Redirections[num4];
				}
				buffer[k * bufferLodStride + i] = vector;
			}
		}
	}

	public void Dump(Dump results)
	{
		foreach (int layerLod in m_LayerLods)
		{
			results.LayerLods.Add(layerLod);
		}
		for (int i = 0; i < 3; i++)
		{
			foreach (int item in m_LayerAtlasSlotIds[i])
			{
				results.LayerAtlasSlotIds[i].Add(item);
			}
			foreach (int item2 in m_AtlasSlotUploadedLayerIds[i])
			{
				results.AtlasSlotUploadedLayerIds[i].Add(item2);
			}
		}
		results.AtlasTextures.Add(m_AtlasTextures.DiffuseTexture);
		results.AtlasTextures.Add(m_AtlasTextures.NormalTexture);
		results.AtlasTextures.Add(m_AtlasTextures.MaskTexture);
		results.UploadTexture = m_UploadTexture.Texture;
	}
}
