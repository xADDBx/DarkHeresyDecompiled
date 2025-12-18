using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.DLC;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Localization;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Stores.DlcInterfaces;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SaveSlotVM : SelectionGroupEntityVM
{
	public readonly ReadOnlyReactiveProperty<SaveLoadMode> Mode;

	private readonly ReactiveProperty<bool> m_IsEmpty = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<string> m_CharacterName = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_GameName = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_GameId = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_SaveName = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_Description = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_LocationName = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_TimeInGame = new ReactiveProperty<string>();

	private readonly ReactiveProperty<DateTime> m_SystemSaveTime = new ReactiveProperty<DateTime>();

	private readonly ReactiveProperty<bool> m_ShowDlcRequiredLabel = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsCurrentIronManSave = new ReactiveProperty<bool>();

	public List<List<string>> DlcRequiredMap = new List<List<string>>();

	private readonly ReactiveProperty<bool> m_ShowAutoSaveMark = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ShowQuickSaveMark = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<List<SaveLoadPortraitVM>> m_PartyPortraits = new ReactiveProperty<List<SaveLoadPortraitVM>>();

	private readonly ReactiveProperty<Texture2D> m_ScreenShot = new ReactiveProperty<Texture2D>();

	private readonly ReactiveProperty<Texture2D> m_ScreenShotHighRes = new ReactiveProperty<Texture2D>();

	private readonly SaveLoadActions m_SaveLoadActions;

	public SaveInfo Reference { get; private set; }

	public bool IsActuallySaved { get; private set; }

	public ReadOnlyReactiveProperty<bool> IsEmpty => m_IsEmpty;

	public ReadOnlyReactiveProperty<string> CharacterName => m_CharacterName;

	public ReadOnlyReactiveProperty<string> GameName => m_GameName;

	public ReadOnlyReactiveProperty<string> GameId => m_GameId;

	public ReadOnlyReactiveProperty<string> SaveName => m_SaveName;

	public ReadOnlyReactiveProperty<string> Description => m_Description;

	public ReadOnlyReactiveProperty<string> LocationName => m_LocationName;

	public ReadOnlyReactiveProperty<string> TimeInGame => m_TimeInGame;

	public ReadOnlyReactiveProperty<DateTime> SystemSaveTime => m_SystemSaveTime;

	public ReadOnlyReactiveProperty<bool> ShowDlcRequiredLabel => m_ShowDlcRequiredLabel;

	public ReadOnlyReactiveProperty<bool> IsCurrentIronManSave => m_IsCurrentIronManSave;

	public ReadOnlyReactiveProperty<bool> ShowAutoSaveMark => m_ShowAutoSaveMark;

	public ReadOnlyReactiveProperty<bool> ShowQuickSaveMark => m_ShowQuickSaveMark;

	public ReadOnlyReactiveProperty<List<SaveLoadPortraitVM>> PartyPortraits => m_PartyPortraits;

	public ReadOnlyReactiveProperty<Texture2D> ScreenShot => m_ScreenShot;

	public ReadOnlyReactiveProperty<Texture2D> ScreenShotHighRes => m_ScreenShotHighRes;

	private static UISaveLoadTexts SaveLoadTexts => ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.SaveLoadTexts;

	public bool ShowSaveLoadButton
	{
		get
		{
			if (Mode.CurrentValue != SaveLoadMode.Load)
			{
				return Reference.Type == SaveInfo.SaveType.Manual;
			}
			return true;
		}
	}

	public SaveSlotVM(SaveInfo saveInfo, ReadOnlyReactiveProperty<SaveLoadMode> mode, SaveLoadActions actions = default(SaveLoadActions), bool allowSwitchOff = false)
		: base(allowSwitchOff)
	{
		Mode = mode;
		AddDisposable(mode.Subscribe(SetMode));
		m_IsEmpty.Value = false;
		m_SaveLoadActions = actions;
		SetSaveInfo(saveInfo);
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		Clear();
	}

	public void SaveOrLoad()
	{
		if (IsCurrentIronManSave.CurrentValue)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.SaveLoadTexts.CannotLoadCurrentIronManSave, addToLog: false, WarningNotificationFormat.Attention);
			});
		}
		else if (PhotonManager.Lobby.IsActive && !PhotonManager.DLC.IsDLCsInLobbyReady)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.SaveLoadTexts.DlcListIsNotLoading, addToLog: false, WarningNotificationFormat.Attention);
			});
		}
		else if (ShowDlcRequiredLabel.CurrentValue)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.SaveLoadTexts.DlcRequired, addToLog: false, WarningNotificationFormat.Attention);
			});
		}
		else
		{
			m_SaveLoadActions.SaveOrLoad?.Invoke(Reference);
		}
	}

	public void Delete()
	{
		m_SaveLoadActions.Delete?.Invoke(Reference);
	}

	public void DeleteWithoutBox()
	{
		m_SaveLoadActions.DeleteWithoutBox?.Invoke(Reference);
	}

	public void ShowScreenshot()
	{
		m_SaveLoadActions.ShowScreenshot?.Invoke(this);
	}

	public void HideScreenshot()
	{
		m_SaveLoadActions.ShowScreenshot?.Invoke(null);
	}

	public void SetSaveInfo(SaveInfo saveInfo)
	{
		Reference = saveInfo;
		m_IsEmpty.Value = Reference == null;
		if (Reference == null)
		{
			Clear();
			return;
		}
		IsActuallySaved = saveInfo.IsActuallySaved;
		m_CharacterName.Value = Reference.PlayerCharacterName;
		m_GameName.Value = Reference.Name;
		m_GameId.Value = Reference.GameId;
		m_SaveName.Value = Reference.Name ?? string.Empty;
		m_Description.Value = Reference.Description;
		ReactiveProperty<string> locationName = m_LocationName;
		object obj = Reference.AreaNameOverride;
		if (obj == null)
		{
			LocalizedString localizedString = SimpleBlueprintExtendAsObject.Or(Reference.Area, null)?.AreaName;
			obj = ((localizedString != null) ? ((string)localizedString) : "");
		}
		locationName.Value = (string)obj;
		m_SystemSaveTime.Value = Reference.SystemSaveTime;
		m_TimeInGame.Value = UtilityTime.TimeSpanToInGameTime(Reference.GameTotalTime);
		ReactiveProperty<bool> showAutoSaveMark = m_ShowAutoSaveMark;
		SaveInfo.SaveType type = Reference.Type;
		showAutoSaveMark.Value = type == SaveInfo.SaveType.Auto || type == SaveInfo.SaveType.IronMan;
		m_ShowQuickSaveMark.Value = Reference.Type == SaveInfo.SaveType.Quick;
		if (Reference.PartyPortraits != null)
		{
			m_PartyPortraits.Value = Reference.PartyPortraits.Where((PortraitForSave portrait) => portrait != null).Take(6).Select(delegate(PortraitForSave portrait)
			{
				string rank = (portrait.IsMainCharacter ? Reference.PlayerCharacterRank.ToString() : string.Empty);
				return new SaveLoadPortraitVM(portrait.Data.SmallPortrait, rank);
			})
				.ToList();
		}
		m_IsCurrentIronManSave.Value = saveInfo.Type == SaveInfo.SaveType.IronMan && !GameUIState.Instance.IsInMainMenu && saveInfo.GameId == Game.Instance.Player.GameId;
		CheckDLC();
	}

	public void TrySetSaveName(string newName)
	{
		if (!IsActuallySaved && Reference != null && !string.IsNullOrEmpty(newName))
		{
			m_SaveName.Value = newName.Trim();
			Reference.Name = newName.Trim();
		}
	}

	private void SetScreenShot()
	{
		m_ScreenShot.Value = Reference.Screenshot;
	}

	private void SetScreenShotHighRes()
	{
		m_ScreenShotHighRes.Value = Reference.ScreenshotHighRes;
	}

	private void SetMode(SaveLoadMode mode)
	{
		CheckDLC();
	}

	public void CheckDLC()
	{
		m_ShowDlcRequiredLabel.Value = Reference != null && !Reference.CheckDlcAvailable();
		if (Reference == null || Reference.CheckDlcAvailable())
		{
			return;
		}
		DlcRequiredMap = new List<List<string>>();
		foreach (List<IBlueprintDlc> item in Reference.GetRequiredDLCMap())
		{
			DlcRequiredMap.Add((from t in item.OfType<BlueprintDlc>()
				select t.GetDlcName()).ToList());
		}
	}

	private void Clear()
	{
		m_CharacterName.Value = string.Empty;
		m_GameName.Value = string.Empty;
		m_GameId.Value = string.Empty;
		m_SaveName.Value = string.Empty;
		m_Description.Value = string.Empty;
		m_LocationName.Value = string.Empty;
		m_TimeInGame.Value = string.Empty;
		m_ShowDlcRequiredLabel.Value = false;
		m_IsCurrentIronManSave.Value = false;
		m_ShowAutoSaveMark.Value = false;
		m_ShowQuickSaveMark.Value = false;
		ReadOnlyReactiveProperty<List<SaveLoadPortraitVM>> partyPortraits = PartyPortraits;
		if (partyPortraits != null && partyPortraits.CurrentValue != null)
		{
			foreach (SaveLoadPortraitVM item in PartyPortraits.CurrentValue)
			{
				item.Dispose();
			}
			m_PartyPortraits.Value = null;
		}
		m_ScreenShot.Value = null;
		m_ScreenShotHighRes.Value = null;
	}

	public bool ReferenceSaveEquals(SaveInfo saveInfo)
	{
		if (saveInfo == null || Reference == null)
		{
			return false;
		}
		return Reference.FolderName == saveInfo.FolderName;
	}

	protected override void DoSelectMe()
	{
		m_SaveLoadActions.Select?.Invoke(Reference);
	}

	public void UpdateScreenshot()
	{
		Game.Instance.SaveManager.LoadScreenshot(Reference, highRes: false, SetScreenShot);
	}

	public void UpdateHighResScreenshot()
	{
		Game.Instance.SaveManager.LoadScreenshot(Reference, highRes: true, SetScreenShotHighRes);
	}

	public void SetAvailable(bool state)
	{
		SetAvailableState(state);
	}

	public void SetAvailableAndActive(bool state)
	{
		SetAvailableState(state);
		Active.Value = state;
	}

	public void DisposeHighResScreenshot()
	{
		SaveScreenshotManager.Instance.DisposeScreenshotTexture(Reference.ScreenshotHighRes);
		Reference.ScreenshotHighRes = null;
		SetScreenShotHighRes();
	}
}
