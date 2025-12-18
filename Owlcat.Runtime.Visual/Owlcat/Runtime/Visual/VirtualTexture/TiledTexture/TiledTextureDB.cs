using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTexture.TiledTexture;

public class TiledTextureDB : IDisposable
{
	private static readonly string kErrorTexPath;

	private const string kErrorTexGuidString = "02ca2c2bed87d5d4ba3e400d7f61e71c";

	public static readonly Guid kErrorGuid;

	private Dictionary<Guid, string> m_GuidToFilePathMap;

	private Dictionary<string, TiledTextureFileHeader> m_FileHeaderMap;

	private static TiledTextureDB s_Instance;

	private readonly string m_ErrorTexPathInProject;

	public static IReadOnlyDictionary<Guid, string> Assets => s_Instance.m_GuidToFilePathMap;

	static TiledTextureDB()
	{
		kErrorGuid = Guid.Parse("02ca2c2bed87d5d4ba3e400d7f61e71c");
		kErrorTexPath = "Packages/com.owlcat.visual/Runtime/VirtualTexture/TiledTexture/Textures/StreamingAssets/02ca2c2bed87d5d4ba3e400d7f61e71c" + TiledTextureFile.FileExtension;
	}

	private TiledTextureDB()
	{
		m_ErrorTexPathInProject = Path.Combine(VirtualTextureManager.TiledTexturesPath, "02ca2c2bed87d5d4ba3e400d7f61e71c" + TiledTextureFile.FileExtension);
		m_GuidToFilePathMap = new Dictionary<Guid, string>();
		m_FileHeaderMap = new Dictionary<string, TiledTextureFileHeader>();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void Init()
	{
		s_Instance = new TiledTextureDB();
		Refresh();
	}

	public static void Refresh()
	{
		s_Instance.m_GuidToFilePathMap.Clear();
		s_Instance.m_FileHeaderMap.Clear();
		s_Instance.InitInRuntime();
	}

	public static bool HasTiles(string guid)
	{
		if (Guid.TryParse(guid, out var result))
		{
			return HasTiles(in result);
		}
		return false;
	}

	public static bool HasTiles(in Guid guid)
	{
		return s_Instance.m_GuidToFilePathMap.ContainsKey(guid);
	}

	internal static bool TryGetAssetPath(Guid guid, out string assetPath)
	{
		return s_Instance.m_GuidToFilePathMap.TryGetValue(guid, out assetPath);
	}

	internal static bool TryGetAssetPath(string guid, out string assetPath)
	{
		if (Guid.TryParse(guid, out var result))
		{
			return TryGetAssetPath(result, out assetPath);
		}
		assetPath = null;
		return false;
	}

	internal static TiledTextureFileHeader GetHeader(string filepath)
	{
		if (!s_Instance.m_FileHeaderMap.TryGetValue(filepath, out var value))
		{
			value = (s_Instance.m_FileHeaderMap[filepath] = TiledTextureFile.ReadHeader(filepath));
		}
		return value;
	}

	private void EnsureDefaultResourcesPrivate()
	{
		if (!Directory.Exists(VirtualTextureManager.TiledTexturesPath))
		{
			Directory.CreateDirectory(VirtualTextureManager.TiledTexturesPath);
		}
		if (!File.Exists(m_ErrorTexPathInProject))
		{
			File.Copy(kErrorTexPath, m_ErrorTexPathInProject);
		}
	}

	public static void EnsureDefaultResources()
	{
		s_Instance.EnsureDefaultResourcesPrivate();
	}

	private void InitInRuntime()
	{
		if (!Directory.Exists(VirtualTextureManager.TiledTexturesPath))
		{
			return;
		}
		foreach (string item in Directory.EnumerateFiles(VirtualTextureManager.TiledTexturesPath))
		{
			if (Guid.TryParse(Path.GetFileNameWithoutExtension(item), out var result))
			{
				m_GuidToFilePathMap.Add(result, item);
			}
		}
	}

	private void OnAssemblyReload()
	{
		Dispose();
	}

	public void Dispose()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		m_GuidToFilePathMap.Clear();
		m_GuidToFilePathMap = null;
	}
}
