using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Core.Async;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Networking.Settings;
using Kingmaker.Settings;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Serialization;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.EntitySystem.Persistence;

public static class ThreadedGameLoader
{
	public static async Task<string> Load(SaveInfo saveInfo, bool isSmokeTest)
	{
		await Awaiters.ThreadPoolYield;
		ISaver mainSaver = saveInfo.Saver;
		try
		{
			saveInfo.Saver = null;
			List<string> allFiles = mainSaver.GetAllFiles();
			ConcurrentQueue<string> allFiles2 = new ConcurrentQueue<string>(allFiles);
			mainSaver.Save();
			Task[] array = new Task[Math.Min(allFiles.Count / 5 + 1, 6)];
			using ISaver mainThreadLoader = mainSaver.Clone();
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = ReadFiles(saveInfo, mainSaver, allFiles2, isSmokeTest);
			}
			Task<string> statisticsStateTask = LoadStatisticsAsync(saveInfo, mainSaver, "statistic");
			string text = LoadJson(saveInfo, mainThreadLoader, "settings");
			string json = LoadJson(saveInfo, mainThreadLoader, "coop");
			byte[] array2 = LoadBin(saveInfo, mainThreadLoader, SaveConsts.PlayerFileName(saveInfo.Format));
			byte[] array3 = LoadBin(saveInfo, mainThreadLoader, SaveConsts.PartyFileName(saveInfo.Format));
			if (text == null)
			{
				throw new Exception("Save is broken: settings.json was not found");
			}
			if (array2 == null)
			{
				throw new Exception("Save is broken: " + SaveConsts.PlayerFileName(saveInfo.Format) + " was not found");
			}
			if (array3 == null)
			{
				throw new Exception("Save is broken: " + SaveConsts.PartyFileName(saveInfo.Format) + " was not found");
			}
			Task<InGameSettings> deserializeSettingsTask = DeserializeInGameSettings(text);
			Task<SceneEntitiesState> deserializePlayerTask = DeserializeSceneState(array2, saveInfo.Format);
			Task<SceneEntitiesState> deserializeCrossSceneStateTask = DeserializeSceneState(array3, saveInfo.Format);
			Task<CoopData> deserializeCoopTask = DeserializeCoopSettings(json);
			await Task.WhenAll(array);
			string statistics = await statisticsStateTask;
			CreateStateData(allFiles);
			await Task.WhenAll(deserializeSettingsTask, deserializeCrossSceneStateTask, deserializePlayerTask, deserializeCoopTask);
			Game.Instance.State.InGameSettings = deserializeSettingsTask.Result;
			Game.Instance.State.CrossSceneState = deserializeCrossSceneStateTask.Result;
			Game.Instance.State.PlayerState = deserializePlayerTask.Result;
			Game.Instance.State.CoopData = deserializeCoopTask.Result;
			return statistics;
		}
		finally
		{
			saveInfo.Saver = mainSaver;
		}
		static async Task<string> LoadStatisticsAsync(SaveInfo saveInfo, ISaver mainSaver, string fileName)
		{
			using ISaver saver = mainSaver.Clone();
			await Awaiters.ThreadPoolYield;
			return LoadJson(saveInfo, saver, fileName);
		}
	}

	private static Task<SceneEntitiesState> DeserializeSceneState(byte[] data, SaveInfo.SaveFormat format)
	{
		return Deserialize<SceneEntitiesState>("Restore scene state (BINARY)", data, format);
	}

	private static Task<InGameSettings> DeserializeInGameSettings(string json)
	{
		return Deserialize<InGameSettings>("Restore settings state", json, SettingsJsonSerializer.Serializer);
	}

	private static async Task<CoopData> DeserializeCoopSettings(string json)
	{
		return (await Deserialize<CoopData>("Restore coop state", json, CoopSettingsSaveDataSerializer.Serializer)) ?? new CoopData();
	}

	private static async Task<T> Deserialize<T>(string timer, string json, JsonSerializer serializer) where T : class
	{
		await EditorSafeThreading.Awaitable;
		using (CodeTimer.New(timer))
		{
			return (json == null) ? null : serializer.DeserializeObject<T>(json);
		}
	}

	private static async Task<T> Deserialize<T>(string timer, byte[] data, SaveInfo.SaveFormat format) where T : class
	{
		await EditorSafeThreading.Awaitable;
		using (CodeTimer.New(timer))
		{
			return format switch
			{
				SaveInfo.SaveFormat.JSON => (data == null) ? null : StandardSerializer.DeserializeFromJson<T>(new JsonInputArchive(new MemoryStream(data))), 
				SaveInfo.SaveFormat.OwlPack => (data == null) ? null : StandardSerializer.DeserializeFromBinary<T>(new BinaryInputArchive(new MemoryStream(data))), 
				_ => throw new ArgumentException($"Unknown save format {format}"), 
			};
		}
	}

	private static void CreateStateData(List<string> allFiles)
	{
		foreach (string allFile in allFiles)
		{
			if (!allFile.EndsWith(".json") && !allFile.EndsWith(".owl"))
			{
				continue;
			}
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(allFile);
			switch (fileNameWithoutExtension)
			{
			case "header":
			case "player":
			case "party":
			case "settings":
			case "statistic":
			case "coop":
				continue;
			}
			if (allFile.StartsWith("checksums"))
			{
				continue;
			}
			string sceneName = ((fileNameWithoutExtension.Length > 32) ? fileNameWithoutExtension.Substring(32, fileNameWithoutExtension.Length - 32) : null);
			AreaPersistentState state = GetState(allFile);
			if (state != null)
			{
				state.ShouldLoad = true;
				if (sceneName != null && state.GetAdditionalSceneStates().FirstOrDefault((SceneEntitiesState s) => s.SceneName == sceneName) == null)
				{
					state.GetAdditionalSceneStates().Add(new SceneEntitiesState(sceneName));
				}
			}
		}
	}

	private static async Task ReadFiles(SaveInfo saveInfo, ISaver mainSaver, ConcurrentQueue<string> allFiles, bool isSmokeTest)
	{
		using ISaver saver = mainSaver.Clone();
		await Awaiters.ThreadPoolYield;
		string result;
		while (allFiles.TryDequeue(out result))
		{
			switch (result)
			{
			case "header.json":
			case "settings.json":
			case "coop.json":
			case "history":
			case "statistic":
				continue;
			}
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(result);
			if (fileNameWithoutExtension == "player" || fileNameWithoutExtension == "party" || (isSmokeTest && !result.StartsWith(saveInfo.Area.AssetGuidThreadSafe)))
			{
				continue;
			}
			if (result.EndsWith(".json"))
			{
				string text = result;
				string text2 = text.Substring(0, text.Length - 5);
				if (JsonUpgradeSystem.ShouldUpgrade(saveInfo, text2))
				{
					string contents = LoadJson(saveInfo, saver, text2);
					using (AreaDataStashFileAccessor accessor = AreaDataStash.AccessFile(result))
					{
						await File.WriteAllTextAsync(accessor.Path, contents);
					}
					continue;
				}
			}
			saver.CopyToStash(result);
		}
	}

	[CanBeNull]
	private static string LoadJson(SaveInfo saveInfo, [NotNull] ISaver saver, [NotNull] string path)
	{
		string text = saver.ReadJson(path);
		if (text == null)
		{
			return null;
		}
		return JsonUpgradeSystem.Upgrade(saveInfo, path, text);
	}

	[CanBeNull]
	private static byte[] LoadBin(SaveInfo saveInfo, [NotNull] ISaver saver, [NotNull] string path)
	{
		return saver.ReadBytes(path);
	}

	private static AreaPersistentState GetState(string guid)
	{
		guid = guid.Substring(0, 32);
		AreaPersistentState areaPersistentState = Game.Instance.State.SavedAreaStates.FirstOrDefault((AreaPersistentState s) => s.AreaGuid == guid);
		if (areaPersistentState == null)
		{
			Game.Instance.State.SavedAreaStates.Add(areaPersistentState = new AreaPersistentState(guid));
		}
		return areaPersistentState;
	}
}
