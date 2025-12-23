using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Code.View.UI.MVVM;
using Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Cheats;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Interfaces;
using Kingmaker.Code.View.Bridge.Interfaces.Canvas;
using Kingmaker.Code.View.UI.MVVM.DetectiveJournal;
using Kingmaker.Code.View.UI.MVVM.HUD.PreciseAttack;
using Kingmaker.Code.View.UI.MVVM.ServiceWindows;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA.Clockwork;
using Kingmaker.Settings;
using Kingmaker.UI.Canvases;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.DebugInformation;
using Kingmaker.UI.DragNDrop;
using Kingmaker.UI.Events;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Pointer;
using Kingmaker.UI.Selection;
using Kingmaker.UI.Sound;
using Kingmaker.UI.UIKitDependencies;
using Kingmaker.Utility.DisposableExtension;
using Owlcat.Runtime.Core.Logging;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.Code.UI.MVVM;

public class RootUIContext : IRootUIContext, IBugReportContext, IFullScreenUIHandler, ISubscriber, IModalWindowUIHandler, IPartyGainExperienceHandler, IGameModeHandler
{
	private readonly List<Type> m_BugReportVMs = new List<Type>
	{
		typeof(ActionBarVM),
		typeof(BookEventVM),
		typeof(BuffVM),
		typeof(ObsoleteCharacterInfoVM),
		typeof(CharGenVM),
		typeof(CombatLogVM),
		typeof(PCCursor),
		typeof(GameOverVM),
		typeof(DialogVM),
		typeof(EscMenuVM),
		typeof(FormationVM),
		typeof(Glossary),
		typeof(InspectVM),
		typeof(InteractionSlotPartVM),
		typeof(InterchapterVM),
		typeof(EpilogVM),
		typeof(InventoryVM),
		typeof(JournalVM),
		typeof(LoadingScreenVM),
		typeof(LocalMapVM),
		typeof(MainMenuVM),
		typeof(NewGameVM),
		typeof(SettingsVM),
		typeof(MapObjectOvertipsVM),
		typeof(UnitOvertipsCollectionVM),
		typeof(PartyVM),
		typeof(SaveLoadVM),
		typeof(SplashScreenController),
		typeof(InitiativeTrackerVM),
		typeof(TutorialVM),
		typeof(VendorVM),
		typeof(InfoWindowVM),
		typeof(TermsOfUseVM),
		typeof(FirstLaunchSettingsVM),
		typeof(GroupChangerVM)
	};

	private readonly Dictionary<Type, string> m_ContextVMNames = new Dictionary<Type, string>
	{
		{
			typeof(InfoWindowVM),
			"Tooltips"
		},
		{
			typeof(TermsOfUseVM),
			"License"
		},
		{
			typeof(SettingsVM),
			"Options"
		},
		{
			typeof(UnitOvertipsCollectionVM),
			"Overtips"
		},
		{
			typeof(PartyVM),
			"PartyManager"
		},
		{
			typeof(MapObjectOvertipsVM),
			"Interactions"
		},
		{
			typeof(CharGenVM),
			"CharacterProgression"
		}
	};

	private MonoBehaviour m_RootView;

	private MonoBehaviour m_CommonView;

	private MonoBehaviour m_UILoadingScreenView;

	private static bool s_UIKitDependenciesInitialized;

	private static readonly DisposableBooleanFlag UIKitDependenciesInitializing = new DisposableBooleanFlag();

	private static GameObject s_SoundGameObject;

	private CompositeDisposable m_Disposables;

	private FullScreenUIType m_FullScreenUIType;

	private ModalWindowUIType m_ModalWindowUIType;

	public static RootUIContext Instance => (RootUIContext)Game.Instance.RootUIContext;

	public GameUIState GameUIState { get; private set; } = new GameUIState();


	public RootVM RootVM { get; private set; }

	public CommonVM CommonVM { get; private set; }

	public LoadingScreenRootVM LoadingScreenRootVM { get; private set; }

	public BaseUnitEntity PreviousLoadingScreenCompanion { get; set; }

	public bool ServiceWindowNowIsOpening { get; set; }

	public bool IsDebugBlueprintsInformationShow { get; set; }

	public bool IsCharGenShown => RootVM?.CharGenContextVM.CharGenVM?.CurrentValue != null;

	public bool IsInMainMenu => RootVM?.MainMenuVM.CurrentValue != null;

	public ISelectionManager SelectionManager
	{
		get
		{
			if (!UtilityGame.IsGlobalMap())
			{
				return SelectionManagerBase.Instance;
			}
			return null;
		}
	}

	private static WidgetPoolSettings WidgetPoolSettings
	{
		get
		{
			WidgetPoolSettings result = default(WidgetPoolSettings);
			result.Scene = SceneManager.GetSceneByName("UI_Common_Scene");
			result.DontDestroyOnLoad = true;
			result.HideFlags = HideFlags.NotEditable;
			return result;
		}
	}

	bool IRootUIContext.CanChangeInput => CanChangeInput();

	public bool HasMainMenuContext => MainMenuContext.Instance != null;

	public GameObject SoundGameObject => s_SoundGameObject;

	public IMainCanvas MainCanvas => Kingmaker.UI.Canvases.MainCanvas.Instance;

	private bool GroupChangerIsShown => RootVM.Instance?.GroupChangerVM?.CurrentValue != null;

	private bool IsLootShow => (RootVM.Instance?.LootContext?.IsShown).GetValueOrDefault();

	public bool IsLoadingScreen => LoadingScreenRootVM?.LoadingScreenVM.CurrentValue != null;

	public bool TooltipIsShown
	{
		get
		{
			if (RootVM.Instance?.TooltipContext.TooltipVM.CurrentValue == null)
			{
				return RootVM.Instance?.TooltipContext.ComparativeTooltipVM.CurrentValue != null;
			}
			return true;
		}
	}

	public bool SaveLoadIsShown => RootVM?.SaveLoadVM.CurrentValue != null;

	public bool IsBugReportOpen => RootVM.Instance?.BugReportVM.CurrentValue != null;

	public bool IsVendorShow => CommonVM?.VendorVM?.CurrentValue != null;

	public bool IsVendorSelectingWindowShow => CommonVM?.VendorSelectingWindowVM?.CurrentValue != null;

	public bool IsInventoryShow => RootVM.WindowsPanelVM?.Value.InventoryVM.CurrentValue != null;

	public bool IsChargenShown => RootVM?.CharGenContextVM.CharGenVM?.CurrentValue != null;

	public FullScreenUIType FullScreenUIType => m_FullScreenUIType;

	public ServiceWindowsType CurrentServiceWindow => ServiceWindowsType.None;

	public bool IsInitiativeTrackerActive => (RootVM.Instance.HUDContext?.InitiativeTrackerVM?.CurrentValue?.ConsoleActive?.CurrentValue).GetValueOrDefault();

	public bool CreditsAreShown => false;

	public bool HasIngameMenu => RootVM?.EscMenuVM?.Value != null;

	public string GetInterfaceName()
	{
		if (Instance.CommonVM?.VendorVM?.CurrentValue != null)
		{
			if (Instance.CommonVM.VendorVM.CurrentValue.ActiveTab.CurrentValue == VendorWindowsTab.Trade)
			{
				return "Vendor";
			}
			return Instance.CommonVM.VendorVM.CurrentValue.ActiveTab.CurrentValue.ToString();
		}
		if (RootVM.Instance == null)
		{
			return string.Empty;
		}
		ReactiveProperty<ServiceWindowsPanelVM> windowsPanelVM = RootVM.Instance.WindowsPanelVM;
		if (windowsPanelVM != null && !windowsPanelVM.IsDisposed)
		{
			ServiceWindowsPanelVM currentValue = windowsPanelVM.CurrentValue;
			if (currentValue != null)
			{
				FullScreenUIType currentValue2 = currentValue.CurrentUIType.CurrentValue;
				if (currentValue2 != 0)
				{
					return currentValue2.ToString();
				}
			}
		}
		ReactiveProperty<DialogVM> dialogVM = RootVM.Instance.DialogVM;
		if (dialogVM != null && !dialogVM.IsDisposed)
		{
			DialogVM currentValue3 = dialogVM.CurrentValue;
			if (currentValue3 != null)
			{
				ReadOnlyReactiveProperty<bool> isVisible = currentValue3.IsVisible;
				if (isVisible != null && isVisible.CurrentValue)
				{
					return "Dialog";
				}
			}
		}
		LootContext lootContext = RootVM.Instance.LootContext;
		if (lootContext != null && lootContext.IsShown)
		{
			return "Loot";
		}
		ReactiveProperty<PreciseAttackVM> preciseAttackVM = RootVM.Instance.PreciseAttackVM;
		if (preciseAttackVM != null && !preciseAttackVM.IsDisposed && preciseAttackVM.CurrentValue != null)
		{
			return "PreciseShot";
		}
		return string.Empty;
	}

	public string GetTooltipData(TooltipData tooltipData)
	{
		TooltipBaseTemplate mainTemplate = tooltipData.MainTemplate;
		if (!(mainTemplate is TooltipTemplateBuff tooltipTemplateBuff))
		{
			if (!(mainTemplate is TooltipTemplateFeature tooltipTemplateFeature))
			{
				if (!(mainTemplate is TooltipTemplateUIFeature tooltipTemplateUIFeature))
				{
					if (!(mainTemplate is TooltipTemplateAbility tooltipTemplateAbility))
					{
						if (!(mainTemplate is TooltipTemplateToggleAbility tooltipTemplateToggleAbility))
						{
							if (!(mainTemplate is TooltipTemplateAbilityTag tooltipTemplateAbilityTag))
							{
								if (!(mainTemplate is TooltipTemplateItem tooltipTemplateItem))
								{
									if (!(mainTemplate is TooltipTemplateTalent tooltipTemplateTalent))
									{
										if (!(mainTemplate is TooltipTemplateLevelUpModifier tooltipTemplateLevelUpModifier))
										{
											if (!(mainTemplate is TooltipTemplateLevelUpAbility tooltipTemplateLevelUpAbility))
											{
												if (mainTemplate is TooltipTemplateLevelUpToggleAbility tooltipTemplateLevelUpToggleAbility)
												{
													return GetBlueprintName(tooltipTemplateLevelUpToggleAbility.BlueprintAbility);
												}
												return string.Empty;
											}
											return GetBlueprintName(tooltipTemplateLevelUpAbility.BlueprintAbility);
										}
										return GetBlueprintName(tooltipTemplateLevelUpModifier.BlueprintModifier);
									}
									return GetBlueprintName(tooltipTemplateTalent.BlueprintFeature);
								}
								return GetBlueprintName(tooltipTemplateItem.Item?.Blueprint);
							}
							return GetBlueprintName(tooltipTemplateAbilityTag.AbilityTag);
						}
						return GetBlueprintName(tooltipTemplateToggleAbility.BlueprintAbility);
					}
					return GetBlueprintName(tooltipTemplateAbility.BlueprintAbility);
				}
				return GetBlueprintName(tooltipTemplateUIFeature.UIFeature.Feature);
			}
			return GetBlueprintName(tooltipTemplateFeature.BlueprintFeatureBase);
		}
		return GetBlueprintName(tooltipTemplateBuff.BlueprintBuff);
		static string GetBlueprintName(BlueprintScriptableObject blueprint)
		{
			return Utilities.GetBlueprintName(blueprint);
		}
	}

	public string GetUIContext(GameObject parent)
	{
		MonoBehaviour[] components = parent.GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour monoBehaviour in components)
		{
			Type t = monoBehaviour.GetType();
			Type type = m_BugReportVMs.FirstOrDefault((Type vm) => IsType(t, vm));
			if (type != null)
			{
				return GetVMContext(type);
			}
		}
		return null;
	}

	public string GetUIBlueprintName(MonoBehaviour parent)
	{
		List<Func<MonoBehaviour, string>> list = new List<Func<MonoBehaviour, string>>
		{
			MakeExtractor((ItemSlotVM vm) => vm.Item.CurrentValue.Blueprint.NameSafe()),
			MakeExtractor((BuffVM vm) => vm.Buff.Blueprint.NameSafe()),
			MakeExtractor((PartyCharacterVM vm) => vm.UnitEntityData.Blueprint.NameSafe()),
			MakeExtractor((EquipSlotVM vm) => vm.Item.CurrentValue.Blueprint.NameSafe()),
			MakeExtractor<CharInfoFeatureVM>(GetNameFromCharInfoFeatureVM),
			MakeExtractor((CharInfoTalentItemVM vm) => vm.Name),
			MakeExtractor((JournalNavigationGroupVM vm) => vm.Title),
			MakeExtractor((JournalQuestVM vm) => vm.Quest.Blueprint.NameSafe()),
			MakeExtractor((JournalQuestObjectiveVM vm) => vm.Objective.Blueprint.NameSafe())
		};
		if (RootVM.Instance.ServiceWindowsContext.FullScreenUIType == FullScreenUIType.DetectiveJournal)
		{
			List<Func<MonoBehaviour, string>> collection = new List<Func<MonoBehaviour, string>>
			{
				MakeExtractor((CaseCardVM vm) => vm.BlueprintCase.NameSafe()),
				MakeExtractor((DetectiveJournalClueVM vm) => vm.Clue.NameSafe()),
				MakeExtractor((ClueConclusionEntityVM vm) => vm.BlueprintCaseItem.NameSafe()),
				MakeExtractor((AddendumInfoVM vm) => (vm.Info as AddendumInfo)?.BlueprintAddendum.NameSafe()),
				MakeExtractor((DeductionOnScreenVM vm) => vm.Conclusion.NameSafe()),
				MakeExtractor((ConclusionSelectionEntityVM vm) => vm.Conclusion.NameSafe()),
				MakeExtractor((ReportAnswerVM vm) => vm.Answer.NameSafe()),
				MakeExtractor((SimpleAnswerVM vm) => vm.Answer.NameSafe()),
				MakeExtractor((NewStudyVM vm) => string.Join("\n", vm.CurrentGroup.CurrentValue.Studies.Select((BlueprintClueStudy s) => s.NameSafe()))),
				MakeExtractor<BlueprintClue>(),
				MakeExtractor<BlueprintClueAddendum>(),
				MakeExtractor<BlueprintConclusion>(),
				MakeExtractor<BlueprintCaseAnswer>()
			};
			list.AddRange(collection);
		}
		foreach (Func<MonoBehaviour, string> item in list)
		{
			string text = item(parent);
			if (text != null)
			{
				return text;
			}
		}
		return string.Empty;
	}

	public BlueprintScriptableObject GetBlueprint(MonoBehaviour parent)
	{
		if (parent is InventoryEquipSlotView inventoryEquipSlotView)
		{
			return inventoryEquipSlotView.Item?.Blueprint;
		}
		if (parent is View<ItemSlotVM> view)
		{
			object obj = view.GetType().GetProperty("ViewModel", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(view);
			if (obj != null)
			{
				return ((ItemSlotVM)obj).Item.CurrentValue.Blueprint;
			}
		}
		if (parent is ItemSlotView<ItemSlotVM> itemSlotView)
		{
			return itemSlotView.Item?.Blueprint;
		}
		if (parent is IHasBlueprintInfo hasBlueprintInfo)
		{
			return hasBlueprintInfo.Blueprint;
		}
		return null;
	}

	public Dictionary<string, string> GetVMNameToContext()
	{
		return m_BugReportVMs.ToDictionary((Type vm) => vm.Name, GetVMContext);
	}

	private string GetVMContext(Type t)
	{
		if (!m_ContextVMNames.TryGetValue(t, out var value))
		{
			return t.Name.Replace("VM", "");
		}
		return value;
	}

	private bool IsType(Type tMono, Type t)
	{
		if (tMono == t)
		{
			return true;
		}
		Type baseType = tMono.BaseType;
		if (baseType == typeof(MonoBehaviour))
		{
			return false;
		}
		if (baseType == t)
		{
			return true;
		}
		if (baseType != null && baseType.IsGenericType && baseType.GetGenericArguments().Length != 0)
		{
			Type[] genericArguments = baseType.GetGenericArguments();
			for (int i = 0; i < genericArguments.Length; i++)
			{
				if (genericArguments[i] == t)
				{
					return true;
				}
			}
		}
		return IsType(baseType, t);
	}

	private Func<MonoBehaviour, string> MakeExtractor<TVM>(Func<TVM, string> selector) where TVM : ViewModel
	{
		return (MonoBehaviour monoBehaviour) => GetNameFromViewModel(monoBehaviour, selector);
	}

	private Func<MonoBehaviour, string> MakeExtractor<TB>() where TB : SimpleBlueprint
	{
		return delegate(MonoBehaviour monoBehaviour)
		{
			try
			{
				return (!(monoBehaviour is View<TB> view)) ? null : view.ViewModel.NameSafe();
			}
			catch
			{
				return (string)null;
			}
		};
	}

	private string GetNameFromViewModel<T>(MonoBehaviour mono, Func<T, string> extractor) where T : ViewModel
	{
		try
		{
			if (!(mono is View<T> view))
			{
				return null;
			}
			return extractor(view.ViewModel);
		}
		catch
		{
			return null;
		}
	}

	private string GetNameFromCharInfoFeatureVM(CharInfoFeatureVM vm)
	{
		try
		{
			TooltipBaseTemplate currentValue = vm.Tooltip.CurrentValue;
			if (currentValue is TooltipTemplateAbility tooltipTemplateAbility)
			{
				return tooltipTemplateAbility.BlueprintAbility?.NameSafe();
			}
			if (currentValue is TooltipTemplateToggleAbility tooltipTemplateToggleAbility)
			{
				return tooltipTemplateToggleAbility.BlueprintAbility?.NameSafe();
			}
			if (currentValue is TooltipTemplateFeature tooltipTemplateFeature)
			{
				return tooltipTemplateFeature.BlueprintFeatureBase?.NameSafe();
			}
			if (currentValue is TooltipTemplateBuff tooltipTemplateBuff)
			{
				return tooltipTemplateBuff.BlueprintBuff.NameSafe();
			}
			if (currentValue is TooltipTemplateAbilityTag tooltipTemplateAbilityTag)
			{
				return tooltipTemplateAbilityTag.AbilityTag?.NameSafe();
			}
			if (currentValue is TooltipTemplateUIFeature tooltipTemplateUIFeature)
			{
				try
				{
					return ((UIFeature)(tooltipTemplateUIFeature.GetType().GetField("m_UIFeature", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField)?.GetValue(tooltipTemplateUIFeature)))?.Feature.NameSafe();
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
		return vm.Acronym;
	}

	private static void InitializeUISystems()
	{
		PFLog.UI.Log("Initializing UI Systems");
		LocalizationManager.Instance.Init(SettingsRoot.Game.Main.Localization, SettingsController.Instance, !SettingsRoot.Game.Main.LocalizationWasTouched.GetValue());
		Game.Instance.UISettingsManager.Initialize();
		EscHotkeyManager.Instance.Initialize();
		WidgetFactoryStash.ResetStash();
		WidgetPool.Initialize(WidgetPoolSettings);
	}

	public static void InitializeUIKitDependencies()
	{
		if (!s_UIKitDependenciesInitialized && !UIKitDependenciesInitializing)
		{
			PFLog.UI.Log("Initializing UI Kit Dependencies");
			using (UIKitDependenciesInitializing.Retain())
			{
				UIKitLogger.SetLogger(new LogChannelLoggerWrapper(PFLog.UI, "UI"));
				UIKitSoundManager.SetSoundManager(UISounds.Instance);
				UIKitRewiredCursorController.SetRewiredCursorController(ConsoleCursor.Instance);
				InputLayer.SetCanReceiveInputPredicate(LoadingProcess.Instance.CanReceiveInput);
				GamePadIcons.SetInstance(ConsoleRoot.Instance.Icons);
			}
			s_UIKitDependenciesInitialized = true;
		}
	}

	public void EnterGame(Action action)
	{
		if (MainMenuContext.Instance != null)
		{
			MainMenuContext.Instance.EnterGame(action);
		}
	}

	public void ClearForLoadMainMenu()
	{
		Instance.RootVM?.SaveLoadContext.DisposeSaveLoad();
		Instance.RootVM?.EscMenuContext?.DisposeEscMenu();
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleCloseAll();
		});
	}

	public void CreateLoadingScreen()
	{
		RootUIConfig config = GetConfig("LoadingScreen");
		LoadingScreenRootVM = new LoadingScreenRootVM();
		m_UILoadingScreenView = config.TryCreateView(LoadingScreenRootVM);
	}

	void IRootUIContext.InitializeUIKitDependencies()
	{
		InitializeUIKitDependencies();
	}

	bool IRootUIContext.CanChangeLanguage()
	{
		return CanChangeLanguage();
	}

	public ILogger CreateLogChannelLoggerWrapper(LogChannel channel, string tag)
	{
		return new LogChannelLoggerWrapper(channel, tag);
	}

	public void Clear()
	{
		PFLog.UI.Log("Disposing Root UI Context");
		ResetUIViewAndVM();
	}

	public void StartUI(bool showLoadingScreen = false)
	{
		PFLog.UI.Log("Start UI");
		ResetUIViewAndVM();
		WidgetFactory.DestroyAll();
		m_Disposables = new CompositeDisposable();
		BugReportControls.AddBugReportControls().AddTo(m_Disposables);
		EventBus.Subscribe(this).AddTo(m_Disposables);
		InitializeUIKitDependencies();
		InitializeUISystems();
		CreateCommonViewAndVM();
		if (showLoadingScreen)
		{
			PFLog.UI.Log("Restart Loading Screen");
			CommonVM.CloseTutorialOnLoad();
			LoadingScreenRootVM.ShowLoadingScreen();
		}
	}

	void IRootUIContext.ResetUI(Action onComplete)
	{
		ResetUI(onComplete);
	}

	public void CloseUI(bool taskIsInteract)
	{
		if (!taskIsInteract && IsLootShow)
		{
			MaybeCollectLoot();
		}
		TutorialVM tutorialVM = RootVM.Instance?.TutorialVM.CurrentValue;
		if (tutorialVM?.BigWindowVM.CurrentValue != null)
		{
			tutorialVM.BigWindowVM.CurrentValue.Hide();
			PFLog.Clockwork.Log("Closed big tutorial window");
		}
		if (tutorialVM?.SmallWindowVM.CurrentValue != null)
		{
			tutorialVM.SmallWindowVM.CurrentValue.Hide();
			PFLog.Clockwork.Log("Closed small tutorial window");
		}
		MessageBoxVM messageBoxVM = RootVM.Instance?.MessageBoxVM.CurrentValue;
		if (messageBoxVM != null)
		{
			PFLog.Clockwork.Log("Accept messagebox with text: " + messageBoxVM.MessageText);
			if (!Clockwork.Instance.Runner.TestScenario.AutoUseRest && messageBoxVM.MessageText.EndsWith("begin resting?"))
			{
				messageBoxVM.OnDeclinePressed();
			}
			else
			{
				messageBoxVM.OnAcceptPressed();
			}
		}
		GroupChangerVM groupChangerVM = Instance.RootVM?.GroupChangerVM.CurrentValue;
		if (groupChangerVM != null)
		{
			Game.Instance.GameCommandQueue.AcceptChangeGroup(groupChangerVM.PartyCharacterRef.ToList(), groupChangerVM.RemoteCharacterRef.ToList(), groupChangerVM.RequiredCharactersRef.ToList(), groupChangerVM is GroupChangerCommonVM);
		}
	}

	public void SetLoadingArea(object area)
	{
		LoadingScreenRootVM?.SetLoadingArea((BlueprintArea)area);
	}

	public void SaveLoadContextLoad(object saveInfo, Action callback)
	{
		Instance.RootVM?.SaveLoadContext.Load((SaveInfo)saveInfo, callback);
	}

	public object GetLoadingScreenVM()
	{
		return LoadingScreenRootVM;
	}

	public object GetFadeVM()
	{
		return FadeVM.Instance;
	}

	public void SetLoadingScreenVM(object vm)
	{
		DoSetLoadingScreenVM((LoadingScreenRootVM)vm);
	}

	public void SwitchDebugBlueprintsInformationShow()
	{
		Instance.IsDebugBlueprintsInformationShow = !Instance.IsDebugBlueprintsInformationShow;
	}

	public bool GetDebugBlueprintsInformationShow()
	{
		return Instance.IsDebugBlueprintsInformationShow;
	}

	void IRootUIContext.ChangeUIPlatform(bool nextPlatform)
	{
		ChangeUIPlatform(nextPlatform);
	}

	public void DoSetLoadingScreenVM(LoadingScreenRootVM vm)
	{
		LoadingScreenRootVM = vm;
	}

	private void ResetUIViewAndVM()
	{
		PFLog.UI.Log("Disposing UI VM and View");
		CommonVM?.Dispose();
		CommonVM = null;
		DestroyView(m_CommonView);
		m_CommonView = null;
		RootVM?.Dispose();
		RootVM = null;
		DestroyView(m_RootView);
		m_RootView = null;
		m_Disposables?.Clear();
		m_Disposables = null;
	}

	private void CreateCommonViewAndVM()
	{
		PFLog.UI.Log("Creating Common View and VM");
		RootUIConfig config = GetConfig("UI_Common_Scene");
		CommonVM = new CommonVM();
		m_CommonView = config.TryCreateView(CommonVM);
		RootVM = new RootVM();
		m_RootView = config.TryCreateView(RootVM);
		s_SoundGameObject = config.gameObject;
	}

	private static void DestroyView(MonoBehaviour view)
	{
		if (!(view == null))
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(view.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(view.gameObject);
			}
		}
	}

	public static void ResetUI(Action onComplete = null)
	{
		PFLog.UI.Log("Reseting UI");
		Instance.StartUI();
		onComplete?.Invoke();
	}

	public static void ChangeUIPlatform(bool nextPlatform)
	{
		PFLog.UI.Log("Changing UI Platform");
		GamePad gamePad = GamePad.Instance;
		int length = Enum.GetValues(typeof(ConsoleType)).Length;
		gamePad.ConsoleTypeProperty.Value = (ConsoleType)((int)(gamePad.ConsoleTypeProperty.Value + length + (nextPlatform ? 1 : (-1))) % length);
		Game.Instance.ControllerMode = ((gamePad.ConsoleTypeProperty.Value != 0) ? Game.ControllerModeType.Gamepad : Game.ControllerModeType.Mouse);
		ResetUI(delegate
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning($"Input controller: {gamePad.ConsoleTypeProperty.Value}");
			});
		});
	}

	public static bool CanChangeInput()
	{
		if (CanChangeInGameMode(Game.Instance.CurrentModeType) && !Instance.GroupChangerIsShown && !Instance.SaveLoadIsShown && !LoadingProcess.Instance.IsLoadingInProcess && CanChangeInSaveState(Game.Instance.SaveManager.CurrentState) && !IsLoadingScreenShown() && Game.Instance.SaveManager.AreSavesUpToDate)
		{
			return Instance.FullScreenUIType != FullScreenUIType.Chargen;
		}
		return false;
		static bool CanChangeInGameMode(GameModeType gameMode)
		{
			if (gameMode != GameModeType.Dialog && gameMode != GameModeType.GameOver)
			{
				return gameMode != GameModeType.BugReport;
			}
			return false;
		}
		static bool CanChangeInSaveState(SaveManager.State saveState)
		{
			if (saveState != SaveManager.State.Saving)
			{
				return saveState != SaveManager.State.Loading;
			}
			return false;
		}
		static bool IsLoadingScreenShown()
		{
			if (Instance.LoadingScreenRootVM != null)
			{
				return Instance.LoadingScreenRootVM.GetLoadingScreenState() == LoadingScreenState.Shown;
			}
			return false;
		}
	}

	public static bool CanChangeLanguage()
	{
		if (Game.Instance.CurrentModeType != GameModeType.Dialog && Game.Instance.CurrentModeType != GameModeType.GameOver && !Instance.GroupChangerIsShown)
		{
			return !Instance.IsChargenShown;
		}
		return false;
	}

	public bool IsBlockedFullScreenUIType()
	{
		if (!(Game.Instance.CurrentModeType == GameModeType.Cutscene) && !(Game.Instance.CurrentModeType == GameModeType.Dialog) && !LoadingProcess.Instance.IsLoadingScreenActive)
		{
			FullScreenUIType fullScreenUIType = m_FullScreenUIType;
			if (fullScreenUIType != FullScreenUIType.EscapeMenu && fullScreenUIType != FullScreenUIType.TransitionMap && fullScreenUIType != FullScreenUIType.Vendor && fullScreenUIType != FullScreenUIType.Credits && fullScreenUIType != FullScreenUIType.Chargen && fullScreenUIType != FullScreenUIType.NewGame)
			{
				return m_ModalWindowUIType == ModalWindowUIType.GameEndingTitles;
			}
		}
		return true;
	}

	private RootUIConfig GetConfig(string loadedUIScene)
	{
		return GetSceneRootObject<RootUIConfig>(loadedUIScene);
	}

	private TSceneCtxConfig GetSceneRootObject<TSceneCtxConfig>(string loadedUIScene) where TSceneCtxConfig : MonoBehaviour
	{
		return (from root in SceneManager.GetSceneByName(loadedUIScene).GetRootGameObjects()
			select root.GetComponent<TSceneCtxConfig>()).FirstOrDefault((TSceneCtxConfig config) => config != null);
	}

	public void MaybeCollectLoot()
	{
		LootContext lootContext = RootVM.Instance?.LootContext;
		if (lootContext == null || !lootContext.IsShown)
		{
			return;
		}
		lootContext.LootVM.CurrentValue.LootCollectorExitLocation.CollectAll();
		foreach (ItemEntity item in Game.Instance.Player.MainCharacterEntity.Inventory)
		{
			item.OnOpenDescriptionFirstTime();
		}
		foreach (ItemEntity item2 in Game.Instance.Player.MainCharacterEntity.Inventory.ToList())
		{
			if (!item2.Blueprint.IsNotable && !(item2.Blueprint is BlueprintItemKey) && !(item2.Blueprint is BlueprintItemNote) && Clockwork.Instance.Runner.TestScenario.CanSellItem(item2.Blueprint) && item2.Wielder == null)
			{
				PFLog.Clockwork.Log($"Moved to cargo: {item2}");
			}
		}
		if (lootContext.LootVM?.CurrentValue != null && lootContext.LootVM.CurrentValue.Mode == LootWindowMode.ZoneExit)
		{
			lootContext.LootVM.CurrentValue.LeaveZone();
		}
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		m_FullScreenUIType = (state ? fullScreenUIType : FullScreenUIType.Unknown);
	}

	public void HandleModalWindowUiChanged(bool state, ModalWindowUIType modalWindowUIType)
	{
		m_ModalWindowUIType = (state ? modalWindowUIType : ModalWindowUIType.Unknown);
	}

	public void HandlePartyGainExperience(int gained, bool isExperienceForEncounter)
	{
		if ((!RootVM.Instance.DialogContext.HasDialog || SettingsRoot.Game.Dialogs.ShowXPGainedNotification.GetValue()) && !Game.Instance.Controllers.TurnController.InCombat)
		{
			string format = "<link=\"Encyclopedia:ExperiencePoints\">" + UINotificationTexts.Instance.XPGainedFormat.Text + "</link>";
			string text = string.Format(format, gained);
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(text, addToLog: false);
			});
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		DragNDropManager.Instance?.CancelDrag();
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		DragNDropManager.Instance?.CancelDrag();
	}
}
