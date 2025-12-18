using System.IO;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTerrain.Streaming;

internal static class Config
{
	public const string kDefaultDatabaseFileName = "terrain_layers";

	public const int kPadding = 16;

	public const int kPageLodCount = 3;

	public const int kPageMipCount = 3;

	public const int kPageTypeCount = 3;

	public const int kMinTextureSize = 256;

	public const int kMaxTextureSize = 4096;

	public static string GetDefaultDatabaseFilePath()
	{
		return Path.Combine(Application.streamingAssetsPath, "Terrain", "terrain_layers");
	}
}
