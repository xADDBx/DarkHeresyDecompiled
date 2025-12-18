using System;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.Experimental.Geometry;

public struct NativeBvhQueryContext<T> : IDisposable where T : unmanaged
{
	public interface IHandler<TContext> where TContext : unmanaged
	{
		void OnOverlap(ref TContext context, T data);
	}

	public UnsafeBvh<T> Tree;

	public NativeArray<uint> Stack;

	public NativeBvhQueryContext(UnsafeBvh<T> tree)
	{
		Tree = tree;
		Stack = ((Tree.NodesCount != 0) ? new NativeArray<uint>((int)Tree.NodesCount, Allocator.Temp) : default(NativeArray<uint>));
	}

	public void Dispose()
	{
		Stack.Dispose();
	}

	public void Overlap<TContext, THandler>(Aabb box, ref TContext context, ref THandler handler) where TContext : unmanaged where THandler : unmanaged, IHandler<TContext>
	{
		if (Tree.NodesCount == 0)
		{
			return;
		}
		Stack[0] = Tree.RootIndex;
		int num = 1;
		do
		{
			ref UnsafeBvh<T>.Node reference = ref Tree.Nodes[Stack[--num]];
			if (reference.Box.Overlaps(in box))
			{
				if (reference.IsLeaf)
				{
					handler.OnOverlap(ref context, reference.Data);
					continue;
				}
				Stack[num++] = System.Runtime.CompilerServices.Unsafe.As<UnsafeBvh<T>.Node._003CChildren_003Ee__FixedBuffer, uint>(ref reference.Children);
				Stack[num++] = System.Runtime.CompilerServices.Unsafe.As<UnsafeBvh<T>.Node._003CChildren_003Ee__FixedBuffer, uint>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref reference.Children, 4));
			}
		}
		while (num > 0);
	}
}
