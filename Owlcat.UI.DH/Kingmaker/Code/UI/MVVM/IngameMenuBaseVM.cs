using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class IngameMenuBaseVM : ViewModel, IHideUIWhileActionCameraHandler, ISubscriber, IGameModeHandler, IUIEventHandler
{
	private readonly ReactiveProperty<bool> m_IsInventoryActive = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsJournalActive = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsLocalMapActive = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsCharScreenActive = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsEncyclopediaActive = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsFormationActive = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsDetectiveJournalActive = new ReactiveProperty<bool>();

	private bool m_IsExplorationOpened;

	private bool m_IsWarpTravelInProgress;

	protected readonly ReactiveProperty<bool> m_ShouldShow = new ReactiveProperty<bool>(value: false);

	protected bool IsAppropriateGameMode
	{
		get
		{
			if (Game.Instance.CurrentModeType != GameModeType.Dialog && Game.Instance.CurrentModeType != GameModeType.Cutscene && Game.Instance.CurrentModeType != GameModeType.GameOver && Game.Instance.CurrentModeType != GameModeType.CutsceneGlobalMap)
			{
				return Game.Instance.CurrentModeType != GameModeType.Pause;
			}
			return false;
		}
	}

	public ReadOnlyReactiveProperty<bool> ShouldShow => m_ShouldShow;

	public ReadOnlyReactiveProperty<bool> IsInventoryActive => m_IsInventoryActive;

	public ReadOnlyReactiveProperty<bool> IsJournalActive => m_IsJournalActive;

	public ReadOnlyReactiveProperty<bool> IsLocalMapActive => m_IsLocalMapActive;

	public ReadOnlyReactiveProperty<bool> IsCharScreenActive => m_IsCharScreenActive;

	public ReadOnlyReactiveProperty<bool> IsEncyclopediaActive => m_IsEncyclopediaActive;

	public ReadOnlyReactiveProperty<bool> IsFormationActive => m_IsFormationActive;

	public ReadOnlyReactiveProperty<bool> IsDetectiveJournalActive => m_IsDetectiveJournalActive;

	protected IngameMenuBaseVM()
	{
		EventBus.Subscribe(this).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			UpdateHandler();
		}).AddTo(this);
		m_ShouldShow.Value = IsAppropriateGameMode;
	}

	protected override void OnDispose()
	{
	}

	protected virtual void UpdateHandler()
	{
		if (Game.Instance.CurrentlyLoadedArea != null)
		{
			ServiceWindowsType currentServiceWindow = RootUIContext.Instance.CurrentServiceWindow;
			m_IsInventoryActive.Value = currentServiceWindow == ServiceWindowsType.Inventory;
			m_IsJournalActive.Value = currentServiceWindow == ServiceWindowsType.Journal;
			m_IsLocalMapActive.Value = currentServiceWindow == ServiceWindowsType.LocalMap;
			m_IsCharScreenActive.Value = currentServiceWindow == ServiceWindowsType.CharacterInfo;
			m_IsEncyclopediaActive.Value = false;
			m_IsDetectiveJournalActive.Value = currentServiceWindow == ServiceWindowsType.DetectiveJournal;
		}
	}

	public void HandleHideUI()
	{
		m_ShouldShow.Value = false;
	}

	public void HandleShowUI()
	{
		DelayedInvoker.InvokeInTime(delegate
		{
			m_ShouldShow.Value = true;
		}, 2.5f);
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		m_ShouldShow.Value = IsAppropriateGameMode && !m_IsExplorationOpened && !m_IsWarpTravelInProgress;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void OpenExplorationScreen(MapObjectView explorationObjectView)
	{
		m_IsExplorationOpened = true;
		m_ShouldShow.Value = false;
	}

	public void CloseExplorationScreen()
	{
		m_IsExplorationOpened = false;
		m_ShouldShow.Value = IsAppropriateGameMode;
	}

	public void HandleUIEvent(UIEventType type)
	{
		m_IsFormationActive.Value = type == UIEventType.FormationWindowOpen;
	}

	public void HandleWarpTravelBeforeStart()
	{
		m_ShouldShow.Value = false;
		m_IsWarpTravelInProgress = true;
	}

	public void HandleWarpTravelStarted(SectorMapPassageEntity passage)
	{
	}

	public void HandleWarpTravelStopped()
	{
		m_ShouldShow.Value = IsAppropriateGameMode && !m_IsExplorationOpened;
		m_IsWarpTravelInProgress = false;
	}

	public void HandleWarpTravelPaused()
	{
	}

	public void HandleWarpTravelResumed()
	{
	}
}
