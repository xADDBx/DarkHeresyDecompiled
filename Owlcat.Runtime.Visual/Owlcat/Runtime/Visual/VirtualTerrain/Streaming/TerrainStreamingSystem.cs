using System;
using System.Collections.Generic;
using System.IO;
using Owlcat.Runtime.Visual.OcclusionGeometryClip;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.VirtualTerrain.Streaming;

internal static class TerrainStreamingSystem
{
	private static readonly List<IStreamingListener> s_Listeners = new List<IStreamingListener>();

	private static TerrainStreamingImplementation s_TerrainStreamingImplementation;

	public static bool Active => s_TerrainStreamingImplementation != null;

	public static bool ManualUpdateEnabled { get; set; }

	public static void Initialize()
	{
		if (!(GraphicsSettings.currentRenderPipeline is WaaaghPipelineAsset) || !GraphicsSettings.TryGetRenderPipelineSettings<VirtualTerrainGlobalSettings>(out var settings) || !settings.Enabled)
		{
			return;
		}
		VirtualTerrainSettings settings2 = new VirtualTerrainSettings
		{
			AtlasCapacityLod0 = settings.AtlasCapacityLod0,
			AtlasCapacityLod1 = settings.AtlasCapacityLod1,
			AtlasCapacityLod2 = settings.AtlasCapacityLod2
		};
		if (s_TerrainStreamingImplementation == null)
		{
			LogChannels.VirtualTerrain.Log("Initialize streaming system");
			string defaultDatabaseFilePath = Config.GetDefaultDatabaseFilePath();
			if (!File.Exists(defaultDatabaseFilePath))
			{
				LogChannels.VirtualTerrain.Log("Database file not found at path " + defaultDatabaseFilePath);
				return;
			}
			s_TerrainStreamingImplementation = new TerrainStreamingImplementation(settings2, s_Listeners, defaultDatabaseFilePath);
			PlayerLoopUtility.RegisterUpdateDelegate(typeof(PreLateUpdate), typeof(TerrainStreamingSystem), OnPreLateUpdate);
		}
	}

	public static void Terminate()
	{
		if (s_TerrainStreamingImplementation == null)
		{
			return;
		}
		try
		{
			LogChannels.VirtualTerrain.Log("Terminate streaming system");
			PlayerLoopUtility.UnregisterUpdateDelegate(typeof(PreLateUpdate), typeof(TerrainStreamingSystem));
			s_TerrainStreamingImplementation.Dispose();
		}
		finally
		{
			s_TerrainStreamingImplementation = null;
		}
	}

	private static void OnPreLateUpdate()
	{
		try
		{
			s_TerrainStreamingImplementation.Update();
		}
		catch (Exception ex)
		{
			LogChannels.VirtualTerrain.Exception(ex);
			Terminate();
		}
	}

	public static void Update()
	{
		if (!ManualUpdateEnabled)
		{
			return;
		}
		try
		{
			s_TerrainStreamingImplementation.Update();
		}
		catch (Exception ex)
		{
			LogChannels.VirtualTerrain.Exception(ex);
			Terminate();
		}
	}

	public static void RegisterListener(IStreamingListener listener)
	{
		s_Listeners.Add(listener);
	}

	public static void UnregisterListener(IStreamingListener listener)
	{
		s_Listeners.Remove(listener);
	}

	public static void PopulateRedirectionBuffer(List<int> layerIds, Span<Vector4> buffer, int bufferLodStride)
	{
		s_TerrainStreamingImplementation?.PopulateRedirectionBuffer(layerIds, buffer, bufferLodStride);
	}

	public static void Dump(Dump results)
	{
		results.Clear();
		s_TerrainStreamingImplementation?.Dump(results);
	}
}
