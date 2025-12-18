using System;
using System.IO;
using Owlcat.Runtime.Core.Collections;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTexture.TiledTexture;

public static class TiledTextureFile
{
	public static readonly string FileExtension = ".tiledtexture";

	public static readonly FixedArray4<byte> kFormatDesc = new FixedArray4<byte>
	{
		item0 = 79,
		item1 = 67,
		item2 = 84,
		item3 = 84
	};

	public const ushort kLatestVersion = 2;

	public static TiledTextureFileHeader CreateHeader(Texture2D sourceTexture, int mipCount, float mipBias, bool isNormalMap, bool isSrgb)
	{
		return CreateHeader(sourceTexture, default(Hash128), mipCount, mipBias, isNormalMap, isSrgb);
	}

	public static TiledTextureFileHeader CreateHeader(Texture2D sourceTexture, Hash128 hash, int mipCount, float mipBias, bool isNormalMap, bool isSrgb)
	{
		return CreateHeader(sourceTexture.width, sourceTexture.height, mipCount, mipBias, (sourceTexture.wrapMode != 0) ? VTWrapMode.Clamp : VTWrapMode.Repeat, hash, isNormalMap, isSrgb);
	}

	public static TiledTextureFileHeader CreateHeader(int width, int height, int mipCount, float mipBias, VTWrapMode wrapMode, Hash128 contentHash, bool isNormalMap, bool isSrgb)
	{
		TiledTextureFlags tiledTextureFlags = TiledTextureFlags.None;
		if (isSrgb)
		{
			tiledTextureFlags |= TiledTextureFlags.sRGB;
		}
		if (isNormalMap)
		{
			tiledTextureFlags |= TiledTextureFlags.NormalMap;
		}
		TiledTextureFileHeader result = default(TiledTextureFileHeader);
		result.FormatDesc = kFormatDesc;
		result.Version = 2;
		result.WrapMode = wrapMode;
		result.Width = width;
		result.Height = height;
		result.Flags = tiledTextureFlags;
		result.Hash128 = contentHash;
		result.MipCount = mipCount;
		result.MipBias = mipBias;
		return result;
	}

	internal static TiledTextureFileHeader ReadHeader(string filePath)
	{
		TiledTextureFileHeader header = default(TiledTextureFileHeader);
		using (FileStream fileStream = File.OpenRead(filePath))
		{
			BinaryReader br = new BinaryReader(fileStream);
			for (int i = 0; i < 4; i++)
			{
				header.FormatDesc[i] = br.ReadByte();
			}
			if (header.FormatDesc != kFormatDesc)
			{
				fileStream.Seek(0L, SeekOrigin.Begin);
				ReadVersion0(ref header, ref br);
			}
			else
			{
				header.Version = br.ReadUInt16();
				switch (header.Version)
				{
				case 1:
					ReadVersion1(ref header, ref br);
					break;
				case 2:
					ReadVersion2(ref header, ref br);
					break;
				default:
					throw new NotImplementedException();
				}
			}
		}
		return header;
	}

	private static void ReadVersion0(ref TiledTextureFileHeader header, ref BinaryReader br)
	{
		header.Version = 0;
		header.Width = br.ReadInt32();
		header.Height = br.ReadInt32();
		header.Flags = (TiledTextureFlags)br.ReadInt32();
		header.MipCount = br.ReadInt32();
		ulong u64_ = br.ReadUInt64();
		ulong u64_2 = br.ReadUInt64();
		header.Hash128 = new Hash128(u64_, u64_2);
		header.WrapMode = VTWrapMode.Clamp;
	}

	private static void ReadVersion1(ref TiledTextureFileHeader header, ref BinaryReader br)
	{
		header.Width = br.ReadInt32();
		header.Height = br.ReadInt32();
		header.Flags = (TiledTextureFlags)br.ReadInt32();
		header.MipCount = br.ReadInt32();
		ulong u64_ = br.ReadUInt64();
		ulong u64_2 = br.ReadUInt64();
		header.Hash128 = new Hash128(u64_, u64_2);
		header.WrapMode = (VTWrapMode)br.ReadByte();
	}

	private static void ReadVersion2(ref TiledTextureFileHeader header, ref BinaryReader br)
	{
		ReadVersion1(ref header, ref br);
		header.MipBias = br.ReadSingle();
	}
}
