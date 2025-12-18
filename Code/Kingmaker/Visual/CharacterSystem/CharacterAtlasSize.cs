using System;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

[Serializable]
public struct CharacterAtlasSize
{
	public enum AtlasResolution
	{
		x64 = 0x40,
		x128 = 0x80,
		x256 = 0x100,
		x512 = 0x200,
		x1024 = 0x400,
		x2048 = 0x800,
		x4096 = 0x1000,
		x8192 = 0x2000
	}

	public AtlasResolution X;

	public AtlasResolution Y;

	public static CharacterAtlasSize Default
	{
		get
		{
			CharacterAtlasSize result = default(CharacterAtlasSize);
			result.X = AtlasResolution.x4096;
			result.Y = AtlasResolution.x2048;
			return result;
		}
	}

	public static CharacterAtlasSize DefaultBake
	{
		get
		{
			CharacterAtlasSize result = default(CharacterAtlasSize);
			result.X = AtlasResolution.x512;
			result.Y = AtlasResolution.x512;
			return result;
		}
	}

	public static CharacterAtlasSize AtlasSizeForCurrentGraphicsSettings()
	{
		switch (QualitySettings.globalTextureMipmapLimit)
		{
		case 0:
			return Default;
		case 1:
		{
			CharacterAtlasSize result = default(CharacterAtlasSize);
			result.X = AtlasResolution.x2048;
			result.Y = AtlasResolution.x1024;
			return result;
		}
		default:
		{
			CharacterAtlasSize result = default(CharacterAtlasSize);
			result.X = AtlasResolution.x1024;
			result.Y = AtlasResolution.x512;
			return result;
		}
		}
	}
}
