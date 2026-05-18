using System.Collections.Generic;
using System.Threading.Tasks;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Stores;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class MainMenuButtonsVM : ViewModel, ISavesUpdatedHandler, ISubscriber, ILocalizationHandler
{
	public ContextMenuEntityVM ContinueVm;

	public ContextMenuEntityVM NewGameVm;

	public ContextMenuEntityVM LoadVm;

	public ContextMenuEntityVM DlcManagerVm;

	public ContextMenuEntityVM NetVm;

	public ContextMenuEntityVM LicenseVm;

	public ContextMenuEntityVM CreditVm;

	public ContextMenuEntityVM OptionsVm;

	public ContextMenuEntityVM FeedbackVm;

	public ContextMenuEntityVM ExitVm;

	private ReactiveProperty<SaveLoadVM> m_SaveLoadVM;

	private readonly List<ContextMenuEntityVM> m_Entities = new List<ContextMenuEntityVM>();

	private SaveStreamer m_SaveStreamer;

	private readonly MainMenuVM m_MainMenu;

	private readonly ReactiveCommand<Unit> m_LanguageChanged = new ReactiveCommand<Unit>();

	public ReadOnlyReactiveProperty<SaveLoadVM> SaveLoadVM => m_SaveLoadVM;

	public bool ExitEnabled => true;

	public Observable<Unit> LanguageChanged => m_LanguageChanged;

	public MainMenuButtonsVM(MainMenuVM mainMenu)
	{
		EventBus.Subscribe(this).AddTo(this);
		UIStrings userInterfacesText = ConfigRoot.Instance.LocalizedTexts.UserInterfacesText;
		m_MainMenu = mainMenu;
		bool isConsole = false;
		CreateButtonsActions(mainMenu, userInterfacesText, isConsole);
		StoreManager.OnRefreshDLC += OnSaveListUpdated;
	}

	private void CreateButtonsActions(MainMenuVM mainMenu, UIStrings t, bool isConsole)
	{
		m_Entities.Add(ContinueVm = new ContextMenuEntityVM(new ContextMenuCollectionEntity(t.MainMenu.Continue, mainMenu.LoadLastGame, () => Game.Instance.SaveManager.AreSavesUpToDate && Game.Instance.SaveManager.GetLatestSave() != null, ButtonSoundsEnum.AnalogSound, ButtonSoundsEnum.AnalogSound)));
		m_Entities.Add(LoadVm = new ContextMenuEntityVM(new ContextMenuCollectionEntity(t.MainMenu.LoadGame, delegate
		{
			EventBus.RaiseEvent(delegate(ISaveLoadUIHandler h)
			{
				h.HandleOpenSaveLoad(SaveLoadMode.Load, singleMode: true);
			});
		}, () => Game.Instance.SaveManager.AreSavesUpToDate && Game.Instance.SaveManager.HasAnySaves(includingCorrupted: true), ButtonSoundsEnum.AnalogSound, ButtonSoundsEnum.AnalogSound)));
		m_Entities.Add(NewGameVm = new ContextMenuEntityVM(new ContextMenuCollectionEntity(t.MainMenu.NewGame, mainMenu.ShowNewGameSetup, null, ButtonSoundsEnum.AnalogSound, ButtonSoundsEnum.AnalogSound)));
		LocalizedString title = (isConsole ? UIStrings.Instance.DlcManager.DlcManagerLabel : t.MainMenu.Addons);
		m_Entities.Add(DlcManagerVm = new ContextMenuEntityVM(new ContextMenuCollectionEntity(title, mainMenu.ShowDlcManager, () => false, ButtonSoundsEnum.AnalogSound, ButtonSoundsEnum.AnalogSound)));
		m_Entities.Add(NetVm = new ContextMenuEntityVM(new ContextMenuCollectionEntity(t.NetLobbyTexts.NetHeader, mainMenu.ShowNetLobby, () => false, ButtonSoundsEnum.AnalogSound, ButtonSoundsEnum.AnalogSound)));
		m_Entities.Add(OptionsVm = new ContextMenuEntityVM(new ContextMenuCollectionEntity(t.MainMenu.Settings, mainMenu.OpenSettings, null, ButtonSoundsEnum.AnalogSound, ButtonSoundsEnum.AnalogSound)));
		m_Entities.Add(CreditVm = new ContextMenuEntityVM(new ContextMenuCollectionEntity(t.MainMenu.Credits, mainMenu.ShowCredits, () => false, ButtonSoundsEnum.AnalogSound, ButtonSoundsEnum.AnalogSound)));
		m_Entities.Add(LicenseVm = new ContextMenuEntityVM(new ContextMenuCollectionEntity(t.MainMenu.License, mainMenu.ShowLicense)));
		m_Entities.Add(FeedbackVm = new ContextMenuEntityVM(new ContextMenuCollectionEntity(t.MainMenu.Feedback, mainMenu.ShowFeedback)));
		m_Entities.Add(ExitVm = new ContextMenuEntityVM(new ContextMenuCollectionEntity(t.MainMenu.Exit, mainMenu.Exit, () => ExitEnabled, ButtonSoundsEnum.AnalogSound, ButtonSoundsEnum.AnalogSound)));
		m_Entities.ForEach(delegate(ContextMenuEntityVM e)
		{
			e.AddTo(this);
		});
	}

	protected override void OnDispose()
	{
		StoreManager.OnRefreshDLC -= OnSaveListUpdated;
		m_Entities.Clear();
	}

	public void OnSaveListUpdated()
	{
		foreach (ContextMenuEntityVM entity in m_Entities)
		{
			entity.RefreshEnabling();
		}
	}

	public async Task OnStreamSaves()
	{
		if (m_SaveStreamer == null)
		{
			m_SaveStreamer = new SaveStreamer();
		}
		await m_SaveStreamer.StreamSaves();
	}

	public void HandleLanguageChanged()
	{
		m_Entities.ForEach(delegate(ContextMenuEntityVM e)
		{
			e.UpdateTitle();
		});
		m_LanguageChanged.Execute(Unit.Default);
	}
}
