using System;
using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.Stores;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class SaveLoadVM : ViewModel, ISavesUpdatedHandler, ISubscriber
{
	private readonly ReactiveProperty<SaveLoadMode> m_Mode = new ReactiveProperty<SaveLoadMode>();

	public readonly SaveLoadMenuVM SaveLoadMenuVM;

	public readonly SaveSlotCollectionVM SaveSlotCollectionVm;

	public NewSaveSlotVM NewSaveSlotVM;

	private readonly ReactiveProperty<SaveSlotVM> m_SaveFullScreenshot = new ReactiveProperty<SaveSlotVM>();

	private readonly ReactiveProperty<SaveSlotVM> m_SelectedSaveSlot = new ReactiveProperty<SaveSlotVM>();

	private SelectionGroupRadioVM<SaveSlotVM> m_SelectionGroup;

	private readonly List<SaveSlotVM> m_SaveSlotVMs = new List<SaveSlotVM>();

	private readonly Action m_OnClose;

	private readonly IUILoadService m_LoadService;

	private readonly ReactiveProperty<bool> m_SaveListUpdating = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsCurrentIronManSave = new ReactiveProperty<bool>();

	private bool m_JustOpened;

	public ReadOnlyReactiveProperty<SaveLoadMode> Mode => m_Mode;

	public ReadOnlyReactiveProperty<SaveSlotVM> SaveFullScreenshot => m_SaveFullScreenshot;

	public ReadOnlyReactiveProperty<SaveSlotVM> SelectedSaveSlot => m_SelectedSaveSlot;

	private bool ShowCorruptionDialog { get; set; }

	public ReadOnlyReactiveProperty<bool> SaveListUpdating => m_SaveListUpdating;

	public ReadOnlyReactiveProperty<bool> IsCurrentIronManSave => m_IsCurrentIronManSave;

	private static UISaveLoadTexts SaveLoadTexts => UIStrings.Instance.SaveLoadTexts;

	private static string DefaultSaveName
	{
		get
		{
			TimeSpan gameTime = Game.Instance.Player.GameTime;
			return string.Concat(SimpleBlueprintExtendAsObject.Or(Game.Instance.CurrentlyLoadedArea, null)?.AreaDisplayName, " -", UIStrings.Instance.SaveLoadTexts.SaveDefaultName, " -", $"{gameTime.Hours:D2}:{gameTime.Minutes:D2}:{gameTime.Seconds:D2}");
		}
	}

	public SaveLoadVM(SaveLoadMode mode, bool singleMode, Action onClose, IUILoadService loadService)
	{
		m_OnClose = onClose;
		m_LoadService = loadService;
		m_Mode.Value = mode;
		SaveLoadMenuVM = new SaveLoadMenuVM(m_Mode, singleMode ? new List<SaveLoadMode> { mode } : new List<SaveLoadMode>
		{
			SaveLoadMode.Save,
			SaveLoadMode.Load
		}).AddTo(this);
		MainThreadDispatcher.StartCoroutine(UIUtilitySaves.WaitForSaveUpdated(delegate
		{
			SaveInfo saveInfo = Game.Instance.SaveManager.CreateNewSave(DefaultSaveName, extended: true).AddTo(this);
			NewSaveSlotVM = new NewSaveSlotVM(saveInfo, Mode, new SaveLoadActions
			{
				SaveOrLoad = RequestSaveNew,
				ShowScreenshot = RequestShowScreenshot
			}).AddTo(this);
		}));
		SaveSlotCollectionVm = new SaveSlotCollectionVM(m_Mode, SelectedSaveSlot).AddTo(this);
		Mode.Subscribe(delegate(SaveLoadMode value)
		{
			HideScreenshot();
			NewSaveSlotVM?.SetAvailable(value == SaveLoadMode.Save);
			m_SelectionGroup?.TrySelectFirstValidEntity();
		}).AddTo(this);
		UpdateSavesCollection();
		StoreManager.OnRefreshDLC += OnRefreshDLC;
		EventBus.Subscribe(this).AddTo(this);
		Disposable.Create(DisposeImplementation).AddTo(this);
	}

	private void DisposeImplementation()
	{
		StoreManager.OnRefreshDLC -= OnRefreshDLC;
		if (!GameUIState.Instance.IsInMainMenu)
		{
			SaveScreenshotManager.Instance.Cleanup();
		}
		HideScreenshot();
	}

	private void OnRefreshDLC()
	{
		SaveSlotCollectionVm?.RefreshDLC();
	}

	private void UpdateSavesCollection()
	{
		m_SaveListUpdating.Value = true;
		Game.Instance.SaveManager.UpdateSaveListAsync();
	}

	private void HandleSaveListUpdate()
	{
		List<SaveInfo> referenceCollection = new List<SaveInfo>(Game.Instance.SaveManager);
		referenceCollection.RemoveAll((SaveInfo s) => Game.Instance.SaveManager.SavesQueuedForDeletion.Contains(s));
		referenceCollection.Sort((SaveInfo s1, SaveInfo s2) => -s1.SystemSaveTime.CompareTo(s2.SystemSaveTime));
		bool allowSwitchOff = Game.Instance.ControllerMode == Game.ControllerModeType.Gamepad;
		ShowCorruptionDialog = false;
		foreach (SaveInfo saveInfo in referenceCollection)
		{
			if (!SaveManager.IsCoopSave(saveInfo) && !SaveManager.IsImportSave(saveInfo) && !m_SaveSlotVMs.Any((SaveSlotVM vm) => vm.ReferenceSaveEquals(saveInfo)))
			{
				SaveSlotVM saveSlotVM = new SaveSlotVM(saveInfo, Mode, new SaveLoadActions
				{
					SaveOrLoad = RequestSaveOrLoad,
					Delete = RequestDeleteSaveInfo,
					ShowScreenshot = RequestShowScreenshot,
					DeleteWithoutBox = DeleteSaveWithoutBox
				}, allowSwitchOff).AddTo(this);
				SaveSlotCollectionVm.HandleNewSave(saveSlotVM);
				m_SaveSlotVMs.Add(saveSlotVM);
			}
		}
		foreach (SaveSlotVM item in m_SaveSlotVMs.Where((SaveSlotVM saveSlotVm) => !referenceCollection.Any(saveSlotVm.ReferenceSaveEquals)).ToList())
		{
			SaveSlotCollectionVm.HandleDeleteSave(item);
			m_SaveSlotVMs.Remove(item);
		}
		m_SaveListUpdating.Value = false;
		m_SelectionGroup = new SelectionGroupRadioVM<SaveSlotVM>(m_SaveSlotVMs, m_SelectedSaveSlot).AddTo(this);
		m_SelectionGroup.InsertEntityAtIndex(0, NewSaveSlotVM);
		if (!m_JustOpened)
		{
			m_SelectionGroup.TrySelectFirstValidEntity();
			m_JustOpened = true;
		}
	}

	private void UpdateSaveSlot(SaveInfo saveInfo)
	{
		m_SaveSlotVMs.FirstOrDefault((SaveSlotVM slot) => slot.ReferenceSaveEquals(saveInfo))?.SetSaveInfo(saveInfo);
	}

	private void RequestSaveNew(SaveInfo saveInfo)
	{
		MainThreadDispatcher.StartCoroutine(UIUtilitySaves.WaitForSaveUpdated(delegate
		{
			SaveManager saveManager = Game.Instance.SaveManager;
			if (saveManager.FirstOrDefault((SaveInfo s) => s.Name.Trim().Equals(saveInfo.Name.Trim(), StringComparison.OrdinalIgnoreCase)) != null)
			{
				int num = 2;
				string name = saveInfo.Name;
				while (saveManager.Any((SaveInfo s) => s.Name.Trim().Equals(saveInfo.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
				{
					saveInfo.Name = $"{name} {num}";
					num++;
				}
			}
			Game.Instance.RequestSaveGame(saveInfo, null, UpdateSavesCollection);
			OnClose();
		}));
	}

	private void RequestSaveOrLoad(SaveInfo saveInfo = null)
	{
		if (saveInfo == null)
		{
			saveInfo = SelectedSaveSlot.CurrentValue?.Reference;
		}
		if (saveInfo != null)
		{
			if (m_Mode.Value == SaveLoadMode.Load)
			{
				RequestLoad(saveInfo);
			}
			else
			{
				RequestOverrideSave(saveInfo);
			}
		}
	}

	private void RequestOverrideSave(SaveInfo saveInfo = null)
	{
		if (saveInfo == null)
		{
			saveInfo = SelectedSaveSlot.CurrentValue?.Reference;
		}
		if (saveInfo == null || saveInfo.Type != 0)
		{
			return;
		}
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler h)
		{
			h.HandleOpen(SaveLoadTexts.OverwriteWarning, DialogMessageBoxType.TextField, null, null, inputText: saveInfo.Name, yesLabel: UIStrings.Instance.SettingsUI.DialogSave, noLabel: null, onTextResult: delegate(string text)
			{
				if (!string.IsNullOrEmpty(text))
				{
					ExecuteOverrideSave(saveInfo, text);
					OnClose();
				}
			});
		});
	}

	private void RequestLoad(SaveInfo saveInfo = null)
	{
		SaveInfo si = saveInfo ?? SelectedSaveSlot.CurrentValue?.Reference;
		SaveInfo saveInfo2 = si;
		bool flag = saveInfo2 != null && saveInfo2.Type == SaveInfo.SaveType.IronMan;
		bool flag2 = GameUIState.Instance.IsInMainMenu || !SettingsRoot.Difficulty.OnlyOneSave;
		if (PhotonManager.Lobby.IsActive && flag)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.SaveLoadTexts.CannotLoadIronManSaveInCoop, addToLog: false, WarningNotificationFormat.Attention);
			});
			return;
		}
		m_IsCurrentIronManSave.Value = flag && !GameUIState.Instance.IsInMainMenu && si.GameId == Game.Instance.Player.GameId;
		if (IsCurrentIronManSave.CurrentValue)
		{
			UtilityMessageBox.ShowMessageBox(UIStrings.Instance.SaveLoadTexts.CannotLoadCurrentIronManSave, DialogMessageBoxType.Message, delegate
			{
			});
			return;
		}
		string text = ((flag && flag2) ? ((string)UIStrings.Instance.SaveLoadTexts.YouLoadIronManSave) : ((!flag && !flag2) ? ((string)UIStrings.Instance.SaveLoadTexts.YouLoadNotIronManSave) : string.Empty));
		if (!string.IsNullOrWhiteSpace(text))
		{
			UtilityMessageBox.ShowMessageBox(text, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
			{
				if (button == DialogMessageBoxButton.Yes)
				{
					if ((bool)SettingsRoot.Difficulty.OnlyOneSave)
					{
						MainThreadDispatcher.StartCoroutine(UIUtilitySaves.WaitForSaveUpdated(delegate
						{
							LoadingProcess.Instance.StartLoadingProcess("SaveRoutine (from save list)", Game.Instance.SaveManager.SaveRoutine(Game.Instance.SaveManager.GetNextAutoslot(), forceAuto: true), delegate
							{
								m_LoadService.Load(si);
							}, LoadingProcessTag.Save);
						}));
					}
					else
					{
						m_LoadService.Load(si);
					}
				}
			});
		}
		else
		{
			m_LoadService.Load(si);
		}
	}

	private void DeleteSaveWithoutBox(SaveInfo saveInfo = null)
	{
		if (saveInfo == null)
		{
			saveInfo = SelectedSaveSlot.CurrentValue?.Reference;
		}
		RequestDeleteSave(saveInfo);
	}

	private void RequestDeleteSaveInfo(SaveInfo saveInfo = null)
	{
		if (saveInfo == null)
		{
			saveInfo = SelectedSaveSlot.CurrentValue?.Reference;
		}
		if (saveInfo == null)
		{
			return;
		}
		string deleteWarning = string.Format(SaveLoadTexts.DeleteWarning, saveInfo.Name);
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler h)
		{
			h.HandleOpen(deleteWarning, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton respond)
			{
				if (respond == DialogMessageBoxButton.Yes)
				{
					RequestDeleteSave(saveInfo);
					SaveSlotVM saveSlotVM = m_SaveSlotVMs.FirstOrDefault((SaveSlotVM s) => s.ReferenceSaveEquals(saveInfo));
					if (saveSlotVM != null)
					{
						SaveSlotCollectionVm.HandleDeleteSave(saveSlotVM);
						m_SaveSlotVMs.Remove(saveSlotVM);
					}
				}
			});
		});
	}

	private void RequestShowScreenshot(SaveSlotVM saveSlotVM)
	{
		saveSlotVM?.UpdateHighResScreenshot();
		m_SaveFullScreenshot.Value = saveSlotVM;
	}

	private void HideScreenshot()
	{
		m_SaveFullScreenshot.Value = null;
	}

	private void ExecuteOverrideSave(SaveInfo saveInfo, string newName)
	{
		Game.Instance.RequestSaveGame(saveInfo, newName, delegate
		{
			UpdateSaveSlot(saveInfo);
		});
	}

	private void RequestDeleteSave(SaveInfo saveInfo)
	{
		Game.Instance.SaveManager.RequestDeleteSave(saveInfo);
	}

	public void OnClose()
	{
		m_SaveListUpdating.Value = false;
		m_IsCurrentIronManSave.Value = false;
		m_JustOpened = false;
		m_OnClose?.Invoke();
	}

	public void OnSaveListUpdated()
	{
		MainThreadDispatcher.StartCoroutine(UIUtilitySaves.WaitForSaveUpdated(HandleSaveListUpdate));
	}
}
