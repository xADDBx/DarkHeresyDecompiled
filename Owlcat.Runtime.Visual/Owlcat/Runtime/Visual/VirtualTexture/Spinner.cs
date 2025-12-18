using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Burst;

namespace Owlcat.Runtime.Visual.VirtualTexture;

[BurstCompile]
public struct Spinner
{
	private int m_Lock;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Acquire()
	{
		while (Interlocked.CompareExchange(ref m_Lock, 1, 0) != 0)
		{
			while (Volatile.Read(ref m_Lock) == 1)
			{
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool TryAcquire()
	{
		if (Volatile.Read(ref m_Lock) == 0)
		{
			return Interlocked.CompareExchange(ref m_Lock, 1, 0) == 0;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool TryAcquire(bool spin)
	{
		if (spin)
		{
			Acquire();
			return true;
		}
		return TryAcquire();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Release()
	{
		Volatile.Write(ref m_Lock, 0);
	}
}
