using System;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Core.Collections;
using Unity.Burst;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTexture.TiledTexture;

[Serializable]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
[BurstCompile]
public struct TiledTextureFileHeader
{
	private static readonly int s_HeaderSize;

	public FixedArray4<byte> FormatDesc;

	public ushort Version;

	public int Width;

	public int Height;

	public TiledTextureFlags Flags;

	public int MipCount;

	public Hash128 Hash128;

	public VTWrapMode WrapMode;

	public float MipBias;

	static TiledTextureFileHeader()
	{
		s_HeaderSize = Marshal.SizeOf<TiledTextureFileHeader>();
	}

	public int GetSizeInBytes()
	{
		int num = s_HeaderSize;
		if (Version < 2)
		{
			num -= 4;
		}
		return num;
	}
}
