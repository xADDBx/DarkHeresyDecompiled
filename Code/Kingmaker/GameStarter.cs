using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Code.GameCore.Modding;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase.ResourceReplacementProvider;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.BundlesLoading;
using Kingmaker.Cheats;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.Code.View.Bridge.GameStarter;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.GameInfo;
using Kingmaker.IngameConsole;
using Kingmaker.Localization;
using Kingmaker.Networking;
using Kingmaker.QA.Clockwork;
using Kingmaker.Settings;
using Kingmaker.Sound;
using Kingmaker.Stores;
using Kingmaker.TextTools;
using Kingmaker.TextTools.Core;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.CommandLineArgs;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility.Locator;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Pathfinding;
using TMPro;
using UnityEngine;

namespace Kingmaker;

public class GameStarter : MonoBehaviour
{
	public static GameStarter Instance;

	private static BlueprintAreaPreset s_MainPresetOverride;

	[CanBeNull]
	[AssetPicker("")]
	public LibraryScriptableObject Library;

	[SerializeField]
	private SettingsValues m_SettingValues;

	[SerializeField]
	private AbstractMainMenuLoadingScreen m_MainMenuLoadingScreen;

	private IResourceReplacementProvider m_ResourceReplacementProvider;

	[SerializeField]
	private GameObject m_LogoCanvas;

	[SerializeField]
	private GameObject m_SplashScreenCanvas;

	private float m_Progress;

	private float m_VirtualProgress;

	public static BlueprintAreaPreset MainPreset => s_MainPresetOverride ?? ConfigRoot.Instance.NewGameSettings.NewGamePreset;

	public static bool InitComplete { get; private set; }

	public static event Action OnInitComplete;

	public void OnEnable()
	{
		Instance = this;
	}

	public void OnDestroy()
	{
		Instance = null;
	}

	public void Awake()
	{
		InitProcess();
		SuperluminalPerf.Initialize();
	}

	private bool ConvertSave()
	{
		string text = "";
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i] == "convert-save")
			{
				if (i == commandLineArgs.Length - 1)
				{
					PFLog.Default.Error("convert-save requires path to save file");
					return true;
				}
				text = commandLineArgs[i + 1];
			}
		}
		if (text == "")
		{
			return false;
		}
		try
		{
			if (!SaveConverter.ConvertSave(text))
			{
				PFLog.Default.Error("Failed to convert " + text + ", check logs above");
			}
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex, "Failed to convert " + text);
		}
		return true;
	}

	private void InitProcess()
	{
		try
		{
			if (ConvertSave())
			{
				Environment.Exit(0);
				return;
			}
			LoggingConfiguration.Configure();
			PFLog.System.Log("LoggingConfiguration.Configure()");
			if (BuildModeUtility.CheckAndEnableLowMemoryOptimizations())
			{
				PFLog.System.Log("Low Memory Optimizations Enabled");
			}
			PathPool.EnablePoolingDebugging = BuildModeUtility.IsDevelopment;
			PFLog.System.Log($"Graphics API: {SystemInfo.graphicsDeviceType}");
			PFLog.System.Log("GameStarter initProcess");
			SystemUtil.WaitForPreviousProcessToFinish();
			PFLog.System.Log("SystemUtil.WaitForPreviousProcessToFinish();");
			AkAudioService.Init();
			PFLog.System.Log("AkAudioService.EnsureAudioInitialized();");
			SettingsRoot.Initialize(m_SettingValues);
			if ((SettingsRoot.Game.Main.AskedSendGameStatistic.GetValue() && SettingsRoot.Game.Main.SendGameStatistic.GetValue()) || Application.isEditor)
			{
				Metrics.StartDataCollection();
			}
			PFLog.System.Log("SettingsRoot.Initialize(m_SettingValues);");
			SettingsController.Instance.InitializeSoundController();
			PFLog.System.Log("SettingsController.Instance.InitializeSoundController();");
			LocalizationManager.Instance.Init(SettingsRoot.Game.Main.Localization, SettingsController.Instance, !SettingsRoot.Game.Main.LocalizationWasTouched.GetValue());
			PFLog.System.Log("LocalizationManager.Instance.Init");
			TextTemplateEngineProxy.Instance.Initialize(TextTemplateEngine.Instance);
			PFLog.System.Log("TextTemplateEngineProxy.Instance.Initialize");
			Game.EnsureGameLifetimeServices();
			PFLog.System.Log("Game.EnsureGameLifetimeServices();");
			PFLog.System.Log("GameStarter.Awake finished");
			PFLog.Default.Log($"Startup options: {GameVersion.Mode}");
			InitComplete = true;
			PFLog.System.Log("InitComplete = true;");
			if (SoundState.Instance != null)
			{
				SoundState.Instance.MusicStateHandler.SetDefaultState(setDefaultMusicState: false);
			}
			PFLog.System.Log("SoundState.Instance.MusicStateHandler.SetDefaultState();");
			ModInitializer.InitializeMods();
			m_ResourceReplacementProvider = ModInitializer.ResourceReplacementProvider;
			BundledSceneLoader.SetResourceReplacementProvider(m_ResourceReplacementProvider);
			ApplicationQuitter.Ensure();
			PFLog.System.Log("GameStarter Init end");
			GameStarter.OnInitComplete?.Invoke();
		}
		catch (Exception ex)
		{
			PFLog.System.Exception(ex, "Exception during init");
		}
	}

	private static void DisableSRPBatching()
	{
		FieldInfo field = typeof(WaaaghPipelineAsset).GetField("m_UseSRPBatcher", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
		if (field == null)
		{
			PFLog.System.Log("Failed to disable SRP batching");
			return;
		}
		PFLog.System.Log("Disable SRP batching");
		field.SetValue(WaaaghPipeline.Asset, false);
	}

	public void StartGame()
	{
		StartCoroutine(StartGameCoroutine());
	}

	private IEnumerator StartGameCoroutine()
	{
		PFLog.System.Log("GameStarter.StartGame started");
		float progressBase = 0.1f;
		SetProgress(progressBase);
		yield return null;
		SettingsController.Instance.StartSyncSettings();
		if (Services.GetInstance<BundlesLoadService>() == null)
		{
			Services.RegisterServiceInstance(new BundlesLoadService(m_ResourceReplacementProvider));
		}
		if (BuildModeUtility.Data?.DisableProfileScope ?? false)
		{
			ProfileScope.Disable = true;
		}
		PFLog.System.Log("GameStarter.StartGame: Loading common bundles");
		using (CodeTimer.New("All bundle loading"))
		{
			IEnumerator bl = BundlesLoadService.Instance.RequestCommonBundlesCoroutine();
			float p = 0f;
			while (bl.MoveNext())
			{
				GameStarter gameStarter = this;
				float num = progressBase;
				float num2;
				p = (num2 = Mathf.Lerp(p, 0.3f, Time.unscaledDeltaTime * 3f));
				gameStarter.SetProgress(num + num2);
				yield return null;
			}
		}
		PFLog.System.Log("GameStarter.StartGame: Init Library");
		GuidClassBinder.StartWarmingUp();
		yield return null;
		StartGameLoader.LoadDirectReferencesList();
		yield return null;
		ModInitializer.ApplyOwlcatModificationsContent();
		yield return null;
		StartGameLoader.LoadPackTOC(m_ResourceReplacementProvider);
		yield return null;
		StartGameLoader.LoadAllJson("Blueprints");
		yield return null;
		StartGameLoader.LoadAllJson(BundlesLoadService.BundlesPath("Blueprints"));
		yield return null;
		ResourcesLibrary.InitializeLibrary(m_ResourceReplacementProvider);
		progressBase += 0.3f;
		PFLog.System.Log("GameStarter.StartGame: Loading Library Finished");
		Game.Instance.RootUIContext.InitializeUIKitDependencies();
		StoreManager.InitializeStore();
		SoundState.Instance.ResetState(SoundStateType.MainMenu);
		FixTMPAssets();
		PFLog.System.Log("GameStarter.StartGame: Total bundles loaded: {0:0.0} MB", (float)BundlesLoadService.Instance.GetTotalBundleMemory() / 1024f / 1024f);
		Debug.developerConsoleVisible = false;
		if (!Application.isEditor)
		{
			using (CodeTimer.New("Wait for LocalizationManager to catch up"))
			{
				while (LocalizationManager.Instance.CurrentPack == null)
				{
					yield return null;
				}
			}
		}
		PFLog.System.Log("GameStarter.StartGame: Localization Initialized");
		yield return null;
		IngameConsoleInitializer.Init();
		GameHeapSnapshot.StartSnapshot();
		LoadPresetFromParameters();
		Game.NewGamePreset = MainPreset;
		PFLog.System.Log($"GameStarter.StartGame: Initialized new game preset {MainPreset}");
		Game.DoAutoDetectControllerMode();
		PFLog.System.Log("GameStarter.StartGame: starting loading UI_Common_Scene");
		Task sceneLoading = BundledSceneLoader.LoadSceneAsync("UI_Common_Scene");
		PFLog.System.Log("GameStarter.StartGame: starting loading LoadingScreen");
		Task loadingScreen = BundledSceneLoader.LoadSceneAsync("LoadingScreen");
		SetProgress(progressBase);
		while (!sceneLoading.IsCompleted)
		{
			yield return null;
		}
		sceneLoading.Wait();
		while (!loadingScreen.IsCompleted)
		{
			yield return null;
		}
		progressBase += 0.4f;
		SetProgress(progressBase);
		PFLog.System.Log("GameStarter.StartGame: finished loading UI_Common_Scene");
		Game.Instance.RootUIContext.StartUI();
		PFLog.System.Log("GameStarter.StartGame: finished loading LoadingScreen");
		Game.Instance.RootUIContext.CreateLoadingScreen();
		CommandLineArguments commandLineArguments = CommandLineArguments.Parse();
		if (commandLineArguments.Contains("copy-saves"))
		{
			SaveManager.CopySaves = commandLineArguments.Get("copy-saves", "");
		}
		Game.Instance.SaveManager.UpdateSaveListAsync();
		string startFrom = commandLineArguments.Get("-start_from");
		if (!string.IsNullOrEmpty(startFrom) || ((bool)ConfigRoot.Instance.NewGameSettings.StartArea && ConfigRoot.Instance.NewGameSettings.SkipMainMenu))
		{
			BlueprintAreaPreset preset = ConfigRoot.Instance.NewGameSettings.StartArea?.DefaultPreset;
			if (preset == null)
			{
				preset = ((BuildModeUtility.CheatsEnabled && !Guid.TryParse(startFrom, out var _)) ? (Utilities.GetScriptableObjects<BlueprintAreaPreset>().FirstOrDefault((BlueprintAreaPreset i) => i.name == startFrom) ?? Utilities.GetScriptableObjects<BlueprintArea>().FirstOrDefault((BlueprintArea i) => i.name == startFrom)?.DefaultPreset) : (BlueprintsDatabase.LoadById<BlueprintAreaPreset>(startFrom) ?? BlueprintsDatabase.LoadById<BlueprintArea>(startFrom)?.DefaultPreset));
			}
			if (preset != null)
			{
				PFLog.System.Log("GameStarter.StartGame: starting from preset " + preset.name);
				SetProgress(1f);
				yield return new WaitForSeconds(0.5f);
				BundledSceneLoader.UnloadSceneAsync("Start");
				SceneLoader.LoadObligatoryScenes();
				Game.Instance.LoadNewGame(preset);
				yield return WaitForSaveAndSettingsSync();
				yield break;
			}
			PFLog.System.Warning("GameStarter.StartGame: can't find suitable preset or area for -start_from=" + startFrom);
		}
		PFLog.System.Log("GameStarter.StartGame: starting loading " + GameScenes.MainMenu);
		sceneLoading = BundledSceneLoader.LoadSceneAsync(GameScenes.MainMenu);
		SetProgress(progressBase);
		while (!sceneLoading.IsCompleted)
		{
			yield return null;
		}
		sceneLoading.Wait();
		SetProgress(progressBase + 0.2f);
		PFLog.System.Log("GameStarter.StartGame: finished loading " + GameScenes.MainMenu);
		yield return WaitForSaveAndSettingsSync();
		SetProgress(1f);
		yield return new WaitForSecondsRealtime(0.5f);
		PFLog.System.Log("GameStarter.StartGame finished");
		Game.Instance.Keyboard.RegisterBuiltinBindings();
		if (IsClockworkMode())
		{
			Clockwork.Enabled = true;
		}
		m_MainMenuLoadingScreen.EndLoading(UnloadStarterScene);
		PhotonManager.NetGame.InitPlatform();
	}

	private IEnumerator WaitForSaveAndSettingsSync()
	{
		WaitForSeconds wait = null;
		while (Game.Instance.SaveManager.SaveListUpdateInProgress || SettingsController.Instance.SettingsSyncInProgress)
		{
			UpdateVirtualProgress();
			if (wait == null)
			{
				wait = new WaitForSeconds(0.1f);
			}
			yield return wait;
		}
	}

	private async void UnloadStarterScene()
	{
		try
		{
			await BundledSceneLoader.UnloadSceneAsync(base.gameObject.scene.name);
			ModInitializer.InitializeModsUI();
		}
		catch (Exception ex)
		{
			PFLog.System.Error(ex);
		}
	}

	private void FixTMPAssets()
	{
		if (!ResourcesLibrary.UseBundles)
		{
			return;
		}
		TMP_FontAsset defaultFont = ConfigRoot.Instance.UIConfig.DefaultTMPFontAsset;
		TMP_SpriteAsset defaultSpriteAsset = ConfigRoot.Instance.UIConfig.DefaultTMPSriteAsset;
		HashSet<UnityEngine.Object> assetsToDeleteSet = new HashSet<UnityEngine.Object> { TMP_Settings.defaultSpriteAsset };
		AddFontToDelete(TMP_Settings.defaultFontAsset);
		foreach (TMP_FontAsset fallbackFontAsset in TMP_Settings.fallbackFontAssets)
		{
			AddFontToDelete(fallbackFontAsset);
		}
		typeof(TMP_Settings).GetField("m_defaultFontAsset", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(TMP_Settings.instance, defaultFont);
		typeof(TMP_Settings).GetField("m_fallbackFontAssets", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(TMP_Settings.instance, new List<TMP_FontAsset> { defaultFont });
		typeof(TMP_Settings).GetField("m_defaultSpriteAsset", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(TMP_Settings.instance, defaultSpriteAsset);
		TMP_Text[] componentsInChildren = m_LogoCanvas.GetComponentsInChildren<TMP_Text>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Fix(componentsInChildren[i]);
		}
		componentsInChildren = m_SplashScreenCanvas.GetComponentsInChildren<TMP_Text>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Fix(componentsInChildren[i]);
		}
		if (Application.isEditor)
		{
			return;
		}
		foreach (UnityEngine.Object item in assetsToDeleteSet)
		{
			if (item != null)
			{
				UnityEngine.Object.DestroyImmediate(item, allowDestroyingAssets: true);
			}
		}
		void AddFontToDelete(TMP_FontAsset fontAsset)
		{
			if (!(fontAsset == null) && assetsToDeleteSet.Add(fontAsset))
			{
				assetsToDeleteSet.Add(fontAsset.material);
				assetsToDeleteSet.AddRange(fontAsset.atlasTextures);
				assetsToDeleteSet.Add(fontAsset.sourceFontFile);
				foreach (TMP_FontAsset item2 in fontAsset.fallbackFontAssetTable)
				{
					AddFontToDelete(item2);
				}
				TMP_FontWeightPair[] fontWeightTable = fontAsset.fontWeightTable;
				for (int j = 0; j < fontWeightTable.Length; j++)
				{
					TMP_FontWeightPair tMP_FontWeightPair = fontWeightTable[j];
					AddFontToDelete(tMP_FontWeightPair.regularTypeface);
					AddFontToDelete(tMP_FontWeightPair.italicTypeface);
				}
			}
		}
		void Fix(TMP_Text t)
		{
			if (t.fontSharedMaterial != null)
			{
				assetsToDeleteSet.Add(t.fontSharedMaterial);
				t.fontSharedMaterial = defaultFont.material;
			}
			if (t.spriteAsset != null)
			{
				assetsToDeleteSet.Add(t.spriteAsset);
				t.spriteAsset = defaultSpriteAsset;
			}
			if (t.font != null)
			{
				AddFontToDelete(t.font);
				t.font = defaultFont;
			}
		}
	}

	private void UpdateVirtualProgress()
	{
		SetProgress(m_Progress);
	}

	private void SetProgress(float progress)
	{
		m_Progress = Mathf.Max(progress, m_Progress);
		float num = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
		m_VirtualProgress = ((m_Progress > m_VirtualProgress) ? m_Progress : (m_VirtualProgress += num / 600f));
		if (m_MainMenuLoadingScreen != null)
		{
			m_MainMenuLoadingScreen.SetLoadingProgress(m_VirtualProgress);
		}
	}

	private void LoadPresetFromParameters()
	{
		string text = CommandLineArguments.Parse().Get("preset");
		if (!(text == ""))
		{
			BlueprintAreaPreset blueprintByName = Utilities.GetBlueprintByName<BlueprintAreaPreset>(text);
			if ((bool)blueprintByName)
			{
				s_MainPresetOverride = blueprintByName;
			}
		}
	}

	public static bool IsClockworkMode()
	{
		bool num = CommandLineArguments.Parse().Contains("-clockwork");
		bool flag = !string.IsNullOrEmpty(PlayerPrefs.GetString("ClockworkScenario", ""));
		return num || flag;
	}
}
