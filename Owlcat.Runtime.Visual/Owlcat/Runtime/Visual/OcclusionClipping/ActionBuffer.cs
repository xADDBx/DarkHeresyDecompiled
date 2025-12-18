using System;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.Experimental.Geometry;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

[BurstCompile]
internal struct ActionBuffer : INativeDisposable, IDisposable
{
	private enum CommandId
	{
		CreateTrigger,
		DestroyTrigger,
		CreateVolume,
		DestroyVolume,
		CreateRenderer,
		DestroyRenderer,
		CreateTriggerVolumeLink,
		DestroyTriggerVolumeLink
	}

	private readonly Allocator m_Allocator;

	private NativeArray<byte> m_Buffer;

	private int m_Length;

	public bool IsCreated => m_Buffer.IsCreated;

	public ActionBuffer(Allocator allocator)
	{
		this = default(ActionBuffer);
		m_Buffer = default(NativeArray<byte>);
		m_Length = 0;
		m_Allocator = allocator;
	}

	public void Dispose()
	{
		if (m_Buffer.IsCreated)
		{
			m_Buffer.Dispose();
		}
		m_Buffer = default(NativeArray<byte>);
		m_Length = 0;
	}

	public JobHandle Dispose(JobHandle dependency)
	{
		return m_Buffer.Dispose(dependency);
	}

	public void CreateTrigger(int id, Obb box)
	{
		(CommandId, int, Obb) data = (CommandId.CreateTrigger, id, box);
		Write<(CommandId, int, Obb)>(in data);
	}

	public void DestroyTrigger(int id)
	{
		(CommandId, int) data = (CommandId.DestroyTrigger, id);
		Write<(CommandId, int)>(in data);
	}

	public void CreateVolume(int id, Obb box)
	{
		(CommandId, int, Obb) data = (CommandId.CreateVolume, id, box);
		Write<(CommandId, int, Obb)>(in data);
	}

	public void DestroyVolume(int id)
	{
		(CommandId, int) data = (CommandId.DestroyVolume, id);
		Write<(CommandId, int)>(in data);
	}

	public void CreateRenderer(int id, Obb box)
	{
		(CommandId, int, Obb) data = (CommandId.CreateRenderer, id, box);
		Write<(CommandId, int, Obb)>(in data);
	}

	public void DestroyRenderer(int id)
	{
		(CommandId, int) data = (CommandId.DestroyRenderer, id);
		Write<(CommandId, int)>(in data);
	}

	public void CreateTriggerVolumeLink(int triggerId, int volumeId)
	{
		(CommandId, int, int) data = (CommandId.CreateTriggerVolumeLink, triggerId, volumeId);
		Write<(CommandId, int, int)>(in data);
	}

	public void DestroyTriggerVolumeLink(int triggerId, int volumeId)
	{
		(CommandId, int, int) data = (CommandId.DestroyTriggerVolumeLink, triggerId, volumeId);
		Write<(CommandId, int, int)>(in data);
	}

	public unsafe void Replay(ref State state)
	{
		byte* readPtr = (byte*)m_Buffer.GetUnsafeReadOnlyPtr();
		byte* endPtr = readPtr + m_Length;
		CommandId result2;
		while (Read<CommandId>(out result2))
		{
			switch (result2)
			{
			case CommandId.CreateTrigger:
			{
				if (!Read<(int, Obb)>(out var result10))
				{
					return;
				}
				state.CreateTrigger(result10.Item1, result10.Item2);
				break;
			}
			case CommandId.DestroyTrigger:
			{
				if (!Read<int>(out var result6))
				{
					return;
				}
				state.DestroyTrigger(result6);
				break;
			}
			case CommandId.CreateVolume:
			{
				if (!Read<(int, Obb)>(out var result8))
				{
					return;
				}
				state.CreateVolume(result8.Item1, result8.Item2);
				break;
			}
			case CommandId.DestroyVolume:
			{
				if (!Read<int>(out var result4))
				{
					return;
				}
				state.DestroyVolume(result4);
				break;
			}
			case CommandId.CreateRenderer:
			{
				if (!Read<(int, Obb)>(out var result9))
				{
					return;
				}
				state.CreateRenderer(result9.Item1, result9.Item2);
				break;
			}
			case CommandId.DestroyRenderer:
			{
				if (!Read<int>(out var result7))
				{
					return;
				}
				state.DestroyRenderer(result7);
				break;
			}
			case CommandId.CreateTriggerVolumeLink:
			{
				if (!Read<(int, int)>(out var result5))
				{
					return;
				}
				state.CreateTriggerVolumeLink(result5.Item1, result5.Item2);
				break;
			}
			case CommandId.DestroyTriggerVolumeLink:
			{
				if (!Read<(int, int)>(out var result3))
				{
					return;
				}
				state.DestroyTriggerVolumeLink(result3.Item1, result3.Item2);
				break;
			}
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		unsafe bool Read<T>(out T result) where T : unmanaged
		{
			result = *(T*)readPtr;
			readPtr += sizeof(T);
			return readPtr <= endPtr;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe void Write<T>(in T data) where T : unmanaged
	{
		int num = sizeof(T);
		if (!m_Buffer.IsCreated || m_Length + num > m_Buffer.Length)
		{
			int capacity = math.ceilpow2(m_Length + num);
			UnsafeCollectionExtensions.ResizeArray(ref m_Buffer, capacity, m_Allocator);
		}
		*(T*)((byte*)m_Buffer.GetUnsafePtr() + m_Length) = data;
		m_Length += num;
	}
}
