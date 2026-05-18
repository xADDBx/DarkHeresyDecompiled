using System;
using System.IO;
using System.Threading.Tasks;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Utility.CodeTimer;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Visual.FogOfWar;
using OwlPack.Runtime;

namespace Kingmaker.EntitySystem.Persistence;

internal class AreaDataStash
{
	private static readonly AreaDataStashDirectoryManager Manager;

	public static GameHistoryFile GameHistoryFile => Manager.GameHistoryFile;

	public static string Folder => Manager.Folder;

	private static JsonSerializer Serializer => SaveSystemJsonSerializer.Serializer;

	public static Formatting Formatting => Formatting.None;

	static AreaDataStash()
	{
		Manager = new AreaDataStashDirectoryManager();
		Manager.Init();
	}

	public static AreaDataStashFileAccessor AccessFile(string filename)
	{
		return Manager.AccessFile(filename);
	}

	public static bool Exists(string filename)
	{
		return Manager.Exists(filename);
	}

	public static void ClearDirectory()
	{
		Manager.ClearDirectory();
	}

	public static void CloseAndDelete()
	{
		Manager.CloseAndDelete();
	}

	public static void PrepareFirstLaunch()
	{
	}

	public static void StashAreaState(AreaPersistentState state, bool dispose)
	{
		bool forceJsonSaves = CheatsSaves.ForceJsonSaves;
		if (state.MainState.SkipSerialize && dispose)
		{
			state.Dispose();
			foreach (SceneEntitiesState allSceneState in state.GetAllSceneStates())
			{
				ClearDataForArea(state.Blueprint.AssetGuidThreadSafe, (allSceneState == state.MainState) ? "" : allSceneState.SceneName, forceJsonSaves);
			}
			SavedFogMasks.Get(state.AreaGuid).Wipe();
			return;
		}
		state.ShouldLoad = true;
		if (forceJsonSaves)
		{
			StandardSerializer.SerializeToJson(ref state).Write(Path(state.Blueprint.AssetGuidThreadSafe, "", useJson: true));
		}
		else
		{
			StandardSerializer.SerializeToBinary(ref state).Write(Path(state.Blueprint.AssetGuidThreadSafe, "", useJson: false));
		}
		foreach (SceneEntitiesState additionalSceneState in state.GetAdditionalSceneStates())
		{
			SceneEntitiesState value = additionalSceneState;
			if (value.IsSceneLoadedThreadSafe)
			{
				if (value.SkipSerialize && dispose)
				{
					ClearDataForArea(state.Blueprint.AssetGuidThreadSafe, value.SceneName, forceJsonSaves);
				}
				else if (forceJsonSaves)
				{
					StandardSerializer.SerializeToJson(ref value).Write(Path(state.Blueprint.AssetGuidThreadSafe, value.SceneName, useJson: true));
				}
				else
				{
					StandardSerializer.SerializeToBinary(ref value).Write(Path(state.Blueprint.AssetGuidThreadSafe, value.SceneName, useJson: false));
				}
			}
		}
		if (dispose)
		{
			state.Dispose();
		}
	}

	private static T Deserialize<T>(SaveInfo.SaveFormat format, byte[] data)
	{
		return format switch
		{
			SaveInfo.SaveFormat.OwlPack => StandardSerializer.DeserializeFromBinary<T>(new BinaryInputArchive(new MemoryStream(data))), 
			SaveInfo.SaveFormat.JSON => StandardSerializer.DeserializeFromJson<T>(new JsonInputArchive(new MemoryStream(data))), 
			_ => throw new ArgumentException($"Unknown format {format}"), 
		};
	}

	public static AreaPersistentState UnstashAreaState(AreaPersistentState area)
	{
		try
		{
			(SaveInfo.SaveFormat, byte[])? dataForArea = GetDataForArea(area, area.MainState);
			AreaPersistentState areaPersistentState = Deserialize<AreaPersistentState>(dataForArea.Value.Item1, dataForArea.Value.Item2);
			foreach (SceneEntitiesState additionalSceneState in area.GetAdditionalSceneStates())
			{
				if (!additionalSceneState.IsSceneLoaded)
				{
					areaPersistentState.GetStateForScene(additionalSceneState.SceneName);
					continue;
				}
				dataForArea = GetDataForArea(area, additionalSceneState);
				if (dataForArea.HasValue)
				{
					using (ProfileScope.New("Deserialize Side State: " + additionalSceneState.SceneName))
					{
						SceneEntitiesState deserializedSceneState = Deserialize<SceneEntitiesState>(dataForArea.Value.Item1, dataForArea.Value.Item2);
						areaPersistentState.SetDeserializedSceneState(deserializedSceneState);
					}
				}
			}
			areaPersistentState.ShouldLoad = false;
			EntityService.Instance.GetProxy(areaPersistentState.Area.UniqueId).Entity?.Dispose();
			return areaPersistentState;
		}
		catch (Exception ex)
		{
			LogChannel.System.Exception(ex, "Exception unstash area state: {0}", area.Blueprint.AssetGuidThreadSafe);
			return area;
		}
		finally
		{
		}
	}

	public static (SaveInfo.SaveFormat format, byte[] data)? GetDataForArea(AreaPersistentState area, SceneEntitiesState state)
	{
		foreach (SaveInfo.SaveFormat value in Enum.GetValues(typeof(SaveInfo.SaveFormat)))
		{
			string path = Path(area, state, value);
			if (File.Exists(path))
			{
				try
				{
					return (value, File.ReadAllBytes(path));
				}
				catch (IOException ex)
				{
					LogChannel.System.Exception(ex, "Exception occured while loading binary area state: {0} {1}", area.Blueprint.AssetGuidThreadSafe, state.SceneName);
					return null;
				}
			}
		}
		LogChannel.System.Log("No state for " + area.Blueprint.AssetGuidThreadSafe + " " + state.SceneName);
		return null;
	}

	public static string Encode(string filename, bool doNotEncode = false)
	{
		return filename;
	}

	public static string FileName(string areaId, string sceneName, SaveInfo.SaveFormat format, bool doNotEncode = false)
	{
		return format switch
		{
			SaveInfo.SaveFormat.JSON => Encode(areaId + sceneName, doNotEncode) + ".json", 
			SaveInfo.SaveFormat.OwlPack => Encode(areaId + sceneName, doNotEncode) + ".owl", 
			_ => throw new ArgumentException($"Unsupported format {format}"), 
		};
	}

	public static string FileName(string areaId, string sceneName, bool useJson)
	{
		return FileName(areaId, sceneName, (!useJson) ? SaveInfo.SaveFormat.OwlPack : SaveInfo.SaveFormat.JSON);
	}

	private static string Path(AreaPersistentState area, SceneEntitiesState state, SaveInfo.SaveFormat format, bool doNotEncode = false)
	{
		return System.IO.Path.Combine(Folder, FileName(area.Blueprint.AssetGuidThreadSafe, (state == area.MainState) ? "" : state.SceneName, format, doNotEncode));
	}

	public static string Path(string areaId, string sceneName, bool useJson, bool doNotEncode = false)
	{
		return System.IO.Path.Combine(Folder, FileName(areaId, sceneName, (!useJson) ? SaveInfo.SaveFormat.OwlPack : SaveInfo.SaveFormat.JSON, doNotEncode));
	}

	public static bool HasData(string areaId, string sceneName, bool useJson)
	{
		return File.Exists(Path(areaId, sceneName, useJson));
	}

	public static void ClearDataForArea(string areaId, string sceneName, bool useJson, bool doNotEncode = false)
	{
		string path = Path(areaId, sceneName, useJson, doNotEncode);
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	public static async Task EncodeActiveAreaFog(AreaPersistentState state)
	{
		FogOfWarArea active = FogOfWarArea.Active;
		if ((bool)active)
		{
			string sceneName = active.gameObject.scene.name;
			byte[] data = await active.RequestData();
			await SavedFogMasks.Get(state.AreaGuid).Save(sceneName, data);
		}
	}

	public static void StashAreaSubState(AreaPersistentState area, SceneEntitiesState state)
	{
		if (CheatsSaves.ForceJsonSaves)
		{
			StandardSerializer.SerializeToJson(ref state).Write(Path(area.Blueprint.AssetGuidThreadSafe, state.SceneName, useJson: true));
		}
		else
		{
			StandardSerializer.SerializeToBinary(ref state).Write(Path(area.Blueprint.AssetGuidThreadSafe, state.SceneName, useJson: false));
		}
	}

	public static SceneEntitiesState UnstashAreaSubState(AreaPersistentState areaState, SceneEntitiesState subState)
	{
		if (!subState.IsSceneLoaded)
		{
			return subState;
		}
		(SaveInfo.SaveFormat, byte[])? dataForArea = GetDataForArea(areaState, subState);
		if (dataForArea.HasValue)
		{
			SceneEntitiesState sceneEntitiesState = Deserialize<SceneEntitiesState>(dataForArea.Value.Item1, dataForArea.Value.Item2);
			areaState.SetDeserializedSceneState(sceneEntitiesState);
			sceneEntitiesState.PostLoad();
			return sceneEntitiesState;
		}
		areaState.SetDeserializedSceneState(subState = new SceneEntitiesState(subState.SceneName));
		return subState;
	}
}
