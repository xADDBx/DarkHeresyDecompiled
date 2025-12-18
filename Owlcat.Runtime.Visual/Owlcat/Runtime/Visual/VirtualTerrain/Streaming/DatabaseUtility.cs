using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTerrain.Streaming;

internal static class DatabaseUtility
{
	private const byte kMagic0 = 84;

	private const byte kMagic1 = 69;

	private const byte kMagic2 = 82;

	private const byte kMagic3 = 76;

	private const uint kVersion = 1u;

	private const long kSizeOfHeader = 28L;

	public static DatabaseLayout ReadIndexFromFile(string path)
	{
		using FileStream input = File.OpenRead(path);
		using BinaryReader binaryReader = new BinaryReader(input, Encoding.UTF8);
		if (binaryReader.ReadByte() != 84)
		{
			throw new Exception("Invalid magic");
		}
		if (binaryReader.ReadByte() != 69)
		{
			throw new Exception("Invalid magic");
		}
		if (binaryReader.ReadByte() != 82)
		{
			throw new Exception("Invalid magic");
		}
		if (binaryReader.ReadByte() != 76)
		{
			throw new Exception("Invalid magic");
		}
		if (binaryReader.ReadUInt32() != 1)
		{
			throw new Exception("Invalid version");
		}
		int num = (int)binaryReader.ReadUInt32();
		int num2 = (int)binaryReader.ReadUInt32();
		int num3 = (int)binaryReader.ReadUInt32();
		int num4 = (int)binaryReader.ReadUInt32();
		int num5 = (int)binaryReader.ReadUInt32();
		if (!Mathf.IsPowerOfTwo(num) || num < 256 || num > 4096)
		{
			throw new Exception($"Invalid texture size ({num}). Must be power of two in range [{256}:{4096}]");
		}
		if (num2 != 16)
		{
			throw new Exception($"Invalid texture padding ({num2}). Must be equal to {16}");
		}
		if (num3 != 3)
		{
			throw new Exception($"Invalid page lod count ({num3}). Must be equal to {3}");
		}
		if (num4 != 3)
		{
			throw new Exception($"Invalid page mip ({num4}). Must be equal to {3}");
		}
		if (num5 != 3)
		{
			throw new Exception($"Invalid page type ({num5}). Must be equal to {3}");
		}
		uint num6 = binaryReader.ReadUInt32();
		TerrainLayerGuid[] array = new TerrainLayerGuid[num6];
		for (int i = 0; i < num6; i++)
		{
			array[i] = new TerrainLayerGuid(binaryReader.ReadUInt32(), binaryReader.ReadUInt32(), binaryReader.ReadUInt32(), binaryReader.ReadUInt32());
		}
		return new DatabaseLayout(num, array, (int)binaryReader.BaseStream.Position);
	}
}
