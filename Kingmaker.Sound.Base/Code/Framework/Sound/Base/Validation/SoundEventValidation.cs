using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using Kingmaker;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;

namespace Code.Framework.Sound.Base.Validation;

[Serializable]
public class SoundEventValidation
{
	[Serializable]
	public class Entry
	{
		[JsonPropertyName("id")]
		public string Id;

		[JsonPropertyName("void")]
		public string VoId;

		[JsonPropertyName("event")]
		public string Event;

		[JsonPropertyName("ForRerecording")]
		public bool ForRerecording;
	}

	public List<Entry> Entries = new List<Entry>();

	private Dictionary<string, Dictionary<string, Entry>> m_EntriesMap = new Dictionary<string, Dictionary<string, Entry>>();

	public void Init()
	{
		LoadValidationInfo();
		foreach (Entry entry in Entries)
		{
			if (!string.IsNullOrEmpty(entry.Id) && !string.IsNullOrEmpty(entry.VoId) && !string.IsNullOrEmpty(entry.Event))
			{
				m_EntriesMap.TryAdd(entry.Id, new Dictionary<string, Entry>());
				m_EntriesMap[entry.Id].TryAdd(entry.VoId, entry);
			}
		}
	}

	private void LoadValidationInfo()
	{
		string path = Path.Combine(ApplicationPaths.streamingAssetsPath, "SoundValidationInfo.json").Replace(Path.DirectorySeparatorChar, '/');
		if (!File.Exists(path))
		{
			return;
		}
		try
		{
			using StreamReader streamReader = new StreamReader(path);
			Entries = JsonConvert.DeserializeObject<List<Entry>>(streamReader.ReadToEnd());
		}
		catch (Exception)
		{
			PFLog.VO.Error("[VO] Cant load SoundValidationInfo.JSON");
		}
	}

	public bool TryGetVoiceOverEvent(string stringId, string voId, out string eventId)
	{
		eventId = string.Empty;
		if (!m_EntriesMap.TryGetValue(stringId, out var value))
		{
			return false;
		}
		if (!value.TryGetValue(voId, out var value2))
		{
			return false;
		}
		if (value2.ForRerecording && BuildModeUtility.IsRelease)
		{
			return false;
		}
		eventId = value2.Event;
		return true;
	}
}
