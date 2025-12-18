using System;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Events;
using Kingmaker.UI.Pointer;
using Kingmaker.UnitLogic.Levelup.Obsolete;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ServiceWindowsVM : ViewModel, INewServiceWindowUIHandler, ISubscriber, IGameModeHandler, ILevelUpCompleteUIHandler, ISubscriber<IBaseUnitEntity>, ILevelUpInitiateUIHandler, IAreaHandler, IAdditiveAreaSwitchHandler, ITurnBasedModeHandler, ITurnBasedModeStartHandler, ILootInteractionHandler, ITradeStateChanged, ISubscriber<IMechanicEntity>, IFormationWindowUIHandler, ICharInfoAbilitiesChooseModeHandler
{
	private readonly ReactiveProperty<ServiceWindowsMenuVM> m_ServiceWindowsMenuVM = new ReactiveProperty<ServiceWindowsMenuVM>();

	private readonly ReactiveCommand<ServiceWindowsType> m_OnWindowShown = new ReactiveCommand<ServiceWindowsType>();

	private readonly ReactiveCommand<Unit> m_OnWindowHidden = new ReactiveCommand<Unit>();

	public ServiceWindowsType CurrentWindow;

	private readonly ReactiveProperty<bool> m_ForceHideBackground = new ReactiveProperty<bool>();

	private CharInfoPageType m_CharInfoPageType;

	private bool m_CharInfoAbilitiesChooseMode;

	private readonly ReactiveCommand<ServiceWindowsType> m_OnOpen = new ReactiveCommand<ServiceWindowsType>();

	private readonly CompositeDisposable m_SelectUnit = new CompositeDisposable();

	private INode m_OpenEncyclopediaPage;

	public ReadOnlyReactiveProperty<ServiceWindowsMenuVM> ServiceWindowsMenuVM => m_ServiceWindowsMenuVM;

	public ReadOnlyReactiveProperty<bool> ForceHideBackground => m_ForceHideBackground;

	public bool CharInfoAbilitiesChooseMode => m_CharInfoAbilitiesChooseMode;

	public CharInfoPageType CharInfoPageType => m_CharInfoPageType;

	private bool IsInSpace
	{
		get
		{
			if (!(Game.Instance.CurrentModeType == GameModeType.SpaceCombat) && !(Game.Instance.CurrentModeType == GameModeType.StarSystem))
			{
				return Game.Instance.CurrentModeType == GameModeType.GlobalMap;
			}
			return true;
		}
	}

	public Observable<ServiceWindowsType> OnOpen => m_OnOpen;

	private bool FormationIsOpened => FormationVM.Instance != null;

	private bool ServiceWindowNowIsOpening
	{
		get
		{
			return RootUIContext.Instance.ServiceWindowNowIsOpening;
		}
		set
		{
			RootUIContext.Instance.ServiceWindowNowIsOpening = value;
		}
	}

	public ServiceWindowsVM()
	{
		EventBus.Subscribe(this).AddTo(this);
		BindKeys();
		ServiceWindowNowIsOpening = false;
		GameUIState.Instance.ActiveDialogController.Where((DialogController dialogController) => dialogController?.Dialog != null).Subscribe(delegate
		{
			HandleCloseAll();
		}).AddTo(this);
	}

	protected override void OnDispose()
	{
		ServiceWindowNowIsOpening = false;
		HideMenu();
		HideWindow(CurrentWindow);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, GetFullScreenUIType(CurrentWindow));
		});
	}

	private void BindKeys()
	{
		if (IsInSpace)
		{
			return;
		}
		Game.Instance.Keyboard.Bind("OpenFormation", delegate
		{
			if (!RootUIContext.Instance.IsBlockedFullScreenUIType() && !IsInSpace && !RootUIContext.Instance.ServiceWindowNowIsOpening)
			{
				if (FormationIsOpened)
				{
					EventBus.RaiseEvent(delegate(IFormationWindowUIHandler h)
					{
						h.HandleCloseFormation();
					});
				}
				else
				{
					EventBus.RaiseEvent(delegate(IFormationWindowUIHandler h)
					{
						h.HandleOpenFormation();
					});
				}
			}
		}).AddTo(this);
	}

	public void HandleCloseAll()
	{
		HideWindow(CurrentWindow);
		ServiceWindowsMenuVM.CurrentValue?.SelectWindow(ServiceWindowsType.None);
		HideMenu();
	}

	public void HandleOpenWindowOfType(ServiceWindowsType type)
	{
		HandleOpenWindow(type);
	}

	public void HandleOpenEncyclopedia(INode page = null)
	{
		m_OpenEncyclopediaPage = page;
		HandleOpenWindow(ServiceWindowsType.Encyclopedia);
	}

	private void InternalSelectUnit(BaseUnitEntity unitEntity)
	{
		if (unitEntity != null)
		{
			Game.Instance.Controllers.SelectionCharacter.SetSelected(unitEntity, force: true, forceFullScreenState: true);
		}
	}

	public void HandleOpenLocalMap()
	{
		HandleOpenWindow(ServiceWindowsType.LocalMap);
	}

	public void HandleOpenDetectiveJournal()
	{
		HandleOpenWindow(ServiceWindowsType.DetectiveJournal);
	}

	private void HandleOpenWindow(ServiceWindowsType type)
	{
		UIVisibilityState.ShowAllUI();
		EventBus.RaiseEvent(delegate(IFormationWindowUIHandler h)
		{
			h.HandleCloseFormation();
		});
		if (!ServiceWindowNowIsOpening && !RootUIContext.Instance.IsVendorShow && (!RootUIContext.Instance.IsBlockedFullScreenUIType() || (CanShowEncyclopedia() && RootUIContext.Instance.FullScreenUIType != FullScreenUIType.Chargen)))
		{
			ServiceWindowNowIsOpening = true;
			HandleOpenWindowDelayed(type);
		}
		bool CanShowEncyclopedia()
		{
			if (type == ServiceWindowsType.Encyclopedia && RootUIContext.Instance.RootVM.EscMenuContext.IsEscMenuActive)
			{
				if (!(Game.Instance.CurrentModeType == GameModeType.Dialog) && !(Game.Instance.CurrentModeType == GameModeType.Default))
				{
					return Game.Instance.CurrentModeType == GameModeType.Pause;
				}
				return true;
			}
			return false;
		}
	}

	private void HandleOpenWindowDelayed(ServiceWindowsType type)
	{
		if (ServiceWindowsMenuVM.CurrentValue == null)
		{
			if (type == CurrentWindow || type == ServiceWindowsType.None)
			{
				ServiceWindowNowIsOpening = false;
				return;
			}
			ShowMenu();
		}
		m_OnOpen?.Execute(type);
		ServiceWindowsMenuVM.CurrentValue?.SelectWindow(type);
	}

	private void OnSelectWindow(ServiceWindowsType type)
	{
		DoSelectWindow(type);
	}

	private void DoSelectWindow(ServiceWindowsType type)
	{
		HideWindow(CurrentWindow);
		if (type == CurrentWindow || type == ServiceWindowsType.None)
		{
			HideMenu();
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: false, GetFullScreenUIType(CurrentWindow));
			});
			CurrentWindow = ServiceWindowsType.None;
			ServiceWindowNowIsOpening = false;
		}
		else
		{
			CurrentWindow = type;
			ShowWindow(type);
		}
	}

	private void ShowWindow(ServiceWindowsType type)
	{
		if ((BuildModeUtility.Data?.Loading?.WidgetStashCleanup).GetValueOrDefault())
		{
			WidgetFactoryStash.ResetStash();
		}
		ServiceWindowNowIsOpening = false;
		if (Game.Instance.Controllers.ClickEventsController != null && Game.Instance.Controllers.ClickEventsController.Mode != 0 && !IsInSpace)
		{
			Game.Instance.Controllers.ClickEventsController.ClearPointerMode();
		}
		m_OnWindowShown.Execute(type);
	}

	private void HideWindow(ServiceWindowsType type)
	{
		m_OnWindowHidden.Execute();
	}

	private void ShowMenu()
	{
		m_ServiceWindowsMenuVM.Value = new ServiceWindowsMenuVM(OnSelectWindow).AddTo(this);
	}

	private void HideMenu()
	{
		ServiceWindowsMenuVM.CurrentValue?.Dispose();
		m_ServiceWindowsMenuVM.Value = null;
		if ((BuildModeUtility.Data?.Loading?.WidgetStashCleanup).GetValueOrDefault())
		{
			Game.Instance.Controllers.CoroutinesController.InvokeInTicks(WidgetFactoryStash.ResetStash, 1);
		}
	}

	public void HandleLevelUpComplete()
	{
		HandleCloseAll();
	}

	public void HandleChargenStart(Action enterNewGameAction)
	{
		HandleCloseAll();
	}

	private static FullScreenUIType GetFullScreenUIType(ServiceWindowsType type)
	{
		return type switch
		{
			ServiceWindowsType.None => FullScreenUIType.Unknown, 
			ServiceWindowsType.Inventory => FullScreenUIType.Inventory, 
			ServiceWindowsType.CharacterInfo => FullScreenUIType.CharacterScreen, 
			ServiceWindowsType.Journal => FullScreenUIType.Journal, 
			ServiceWindowsType.Encyclopedia => FullScreenUIType.Encyclopedia, 
			ServiceWindowsType.DetectiveJournal => FullScreenUIType.DetectiveJournal, 
			_ => FullScreenUIType.Unknown, 
		};
	}

	public void HandleLevelUpStart(BaseUnitEntity unit, Action onCommit = null, Action onStop = null, LevelUpState.CharBuildMode mode = LevelUpState.CharBuildMode.LevelUp)
	{
	}

	public void HandleForceCloseAllComponentsMenu()
	{
		HandleCloseAll();
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene || gameMode == GameModeType.GameOver || gameMode == GameModeType.CutsceneGlobalMap)
		{
			HandleCloseAll();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void OnAreaBeginUnloading()
	{
		HandleCloseAll();
	}

	public void OnAreaDidLoad()
	{
	}

	public void OnAdditiveAreaBeginDeactivated()
	{
		HandleCloseAll();
	}

	public void OnAdditiveAreaDidActivated()
	{
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			HandleCloseAll();
		}
	}

	void ITurnBasedModeStartHandler.HandleTurnBasedModeStarted()
	{
		HandleCloseAll();
	}

	public void HandleLootInteraction(EntityViewBase[] objects, LootContainerType containerType, Action closeCallback)
	{
	}

	public void HandleSpaceLootInteraction(ILootable[] objects, LootContainerType containerType, Action closeCallback, SkillCheckResult skillCheckResult = null)
	{
	}

	public void HandleZoneLootInteraction(AreaTransitionPart areaTransition)
	{
		HandleCloseAll();
	}

	public void HandleBeginTrading()
	{
		HandleCloseAll();
	}

	public void HandleEndTrading()
	{
	}

	public void HandleVendorAboutToTrading()
	{
	}

	public void HandleMultiEntrance(BlueprintMultiEntrance multiEntrance)
	{
		HandleCloseAll();
	}

	public void HandleOpenFormation()
	{
		DoSelectWindow(ServiceWindowsType.None);
	}

	public void HandleCloseFormation()
	{
	}

	public void HandleChooseMode(bool active)
	{
		m_CharInfoAbilitiesChooseMode = active;
	}
}
