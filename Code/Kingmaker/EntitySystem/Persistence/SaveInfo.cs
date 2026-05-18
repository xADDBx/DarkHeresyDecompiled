using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Root;
using Kingmaker.DLC;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Localization;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence;

public class SaveInfo : IDisposable
{
	public enum SaveFormat
	{
		JSON,
		OwlPack
	}

	public enum SaveType
	{
		Manual,
		Quick,
		Auto,
		Remote,
		Bugreport,
		IronMan,
		ForImport,
		Coop
	}

	public enum StateType
	{
		None,
		Serializing,
		Saving
	}

	private class ReadLockScope : IDisposable
	{
		private SaveInfo m_SaveInfo;

		private readonly bool m_Upgradeable;

		public ReadLockScope(SaveInfo saveInfo, bool upgradeable)
		{
			m_Upgradeable = upgradeable;
			if (!(upgradeable ? saveInfo.m_FileAccessLock.TryEnterUpgradeableReadLock(20.Seconds()) : saveInfo.m_FileAccessLock.TryEnterReadLock(20.Seconds())))
			{
				throw new Exception("Cannot get access to file: possible deadlock");
			}
			saveInfo.UpdateSaverMode();
			m_SaveInfo = saveInfo;
		}

		public void Dispose()
		{
			if (m_Upgradeable)
			{
				m_SaveInfo?.m_FileAccessLock.ExitUpgradeableReadLock();
			}
			else
			{
				m_SaveInfo?.m_FileAccessLock.ExitReadLock();
			}
			m_SaveInfo?.UpdateSaverMode();
			m_SaveInfo = null;
		}
	}

	private class WriteLockScope : IDisposable
	{
		private SaveInfo m_SaveInfo;

		public WriteLockScope(SaveInfo saveInfo)
		{
			if (!saveInfo.m_FileAccessLock.TryEnterWriteLock(20.Seconds()))
			{
				throw new Exception("Cannot get access to file: possible deadlock");
			}
			saveInfo.UpdateSaverMode();
			m_SaveInfo = saveInfo;
		}

		public void Dispose()
		{
			m_SaveInfo?.m_FileAccessLock.ExitWriteLock();
			m_SaveInfo?.UpdateSaverMode();
			m_SaveInfo = null;
		}
	}

	public const int CurrentCompatibilityVersion = 2;

	private readonly ReaderWriterLockSlim m_FileAccessLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

	[JsonProperty]
	private BlueprintCampaignReference m_CampaignReference;

	[JsonProperty]
	private List<BlueprintDlcRewardReference> m_DlcRewards;

	[JsonProperty]
	public List<SerializableRandState> StatefulRandomStates;

	public volatile StateType m_OperationState;

	[JsonProperty]
	public SaveFormat Format { get; set; }

	[JsonProperty]
	public string Name { get; set; }

	[JsonProperty]
	public string Description { get; set; }

	[JsonProperty]
	public string SaveId { get; set; }

	[JsonProperty]
	public BlueprintQuest QuestWithSaveDescription { private get; set; }

	[JsonProperty]
	public string PlayerCharacterName { get; set; }

	[JsonProperty]
	public int PlayerCharacterRank { get; set; }

	[JsonProperty]
	public string GameId { get; set; }

	public IEnumerable<BlueprintDlcReward> DlcRewards
	{
		get
		{
			return m_DlcRewards?.Dereference();
		}
		set
		{
			m_DlcRewards = value.Select((BlueprintDlcReward dlc) => dlc.ToReference<BlueprintDlcRewardReference>()).ToList();
		}
	}

	[JsonProperty]
	public SaveType Type { get; set; }

	[JsonProperty]
	public bool IsAutoLevelupSave { get; set; }

	[JsonProperty]
	public int QuickSaveNumber { get; set; }

	[JsonProperty]
	public int LoadedTimes { get; set; }

	[JsonProperty]
	public BlueprintArea Area { get; set; }

	[JsonProperty]
	public BlueprintAreaPart AreaPart { get; set; }

	[JsonProperty]
	public string AreaNameOverride { get; set; }

	[JsonProperty]
	public List<PortraitForSave> PartyPortraits { get; set; }

	[JsonProperty]
	public DateTime? GameStartSystemTime { get; set; }

	[JsonProperty]
	public DateTime SystemSaveTime { get; set; }

	[JsonProperty]
	public TimeSpan GameSaveTime { get; set; }

	[JsonProperty]
	public string GameSaveTimeText { get; set; }

	[JsonProperty]
	public TimeSpan GameTotalTime { get; set; }

	[JsonProperty]
	public List<int> Versions { get; set; } = new List<int>();


	[JsonProperty]
	public int CompatibilityVersion { get; set; } = 2;


	public BlueprintCampaign Campaign
	{
		get
		{
			if (m_CampaignReference != null && !m_CampaignReference.IsEmpty())
			{
				return m_CampaignReference.Get();
			}
			return null;
		}
		set
		{
			m_CampaignReference = value?.ToReference<BlueprintCampaignReference>();
		}
	}

	public string FolderName { get; set; }

	public Texture2D Screenshot { get; set; }

	public Texture2D ScreenshotHighRes { get; set; }

	public ISaver Saver { get; set; }

	public string LocalizedDescription
	{
		get
		{
			LocalizedString localizedString = QuestWithSaveDescription?.Description;
			if (localizedString == null)
			{
				return Description;
			}
			return localizedString;
		}
	}

	public string FileName
	{
		get
		{
			if (!IsActuallySaved)
			{
				return null;
			}
			return Path.GetFileName(FolderName);
		}
	}

	public bool IsActuallySaved => !string.IsNullOrEmpty(FolderName);

	public StateType OperationState
	{
		get
		{
			return m_OperationState;
		}
		set
		{
			m_OperationState = value;
		}
	}

	public bool CheckDlcAvailable()
	{
		using (ContextData<DlcExtension.LoadSaveDlcCheck>.Request())
		{
			return DlcRewards?.All((BlueprintDlcReward dlcReward) => dlcReward.IsAvailable) ?? true;
		}
	}

	public List<List<IBlueprintDlc>> GetRequiredDLCMap()
	{
		using (ContextData<DlcExtension.LoadSaveDlcCheck>.Request())
		{
			List<List<IBlueprintDlc>> list = new List<List<IBlueprintDlc>>();
			foreach (BlueprintDlcReward dlcReward in DlcRewards)
			{
				List<IBlueprintDlc> list2 = new List<IBlueprintDlc>();
				foreach (IBlueprintDlc dlc in dlcReward.Dlcs)
				{
					if (!dlc.IsActive && !list.Any((List<IBlueprintDlc> r) => r.Contains(dlc)))
					{
						list2.Add(dlc);
					}
				}
				if (list2.Any())
				{
					list.Add(list2);
				}
			}
			return list;
		}
	}

	public void Dispose()
	{
		SaveScreenshotManager.Instance.DisposeScreenshotTexture(ScreenshotHighRes);
		SaveScreenshotManager.Instance.DisposeScreenshotTexture(Screenshot);
		ScreenshotHighRes = null;
		Screenshot = null;
	}

	public IDisposable GetReadScope(bool upgradeable = false)
	{
		return new ReadLockScope(this, upgradeable);
	}

	public IDisposable GetWriteScope()
	{
		return new WriteLockScope(this);
	}

	private void UpdateSaverMode()
	{
		ISaver.Mode mode = (m_FileAccessLock.IsWriteLockHeld ? ISaver.Mode.Write : ((m_FileAccessLock.IsReadLockHeld || m_FileAccessLock.IsUpgradeableReadLockHeld) ? ISaver.Mode.Read : ISaver.Mode.None));
		Saver?.SetMode(mode);
	}

	public override string ToString()
	{
		return $"Save {Name}(Area:{Area},Part:{AreaPart})";
	}
}
