using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Code.Framework.Utility.UnityExtensions;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Base;
using Kingmaker.Cheats;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameCommands;
using Kingmaker.GameInfo;
using Kingmaker.GameModes;
using Kingmaker.Localization;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Settings;
using Kingmaker.Stores;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Reporting.Base;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Newtonsoft.Json;
using Owlcat.Bugreport;
using Owlcat.Bugreport.Attachments;
using Owlcat.Bugreport.ErrorHandling;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility.Locator;
using Owlcat.UI;
using OwlPack.Runtime;
using UnityEngine;
using UnityEngine.CrashReportHandler;

namespace Kingmaker.Utility;

public class ReportingUtils : IDisposable, IFullScreenUIHandler, ISubscriber, IPortraitHoverUIHandler, ISubscriber<IBaseUnitEntity>, IBugReportDescriptionUIHandler, IGlobalRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, IGlobalRulebookSubscriber, IBugReportUIHandler, IService
{
	public static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("ReportingUtils");

	private const string Project = "WH2";

	private const string SaveFileName = "save.zks";

	private const string ReportingUtilsLogFile = "reporting_util.txt";

	private const int ReportingUtilsLogFileMaxSize = 5248000;

	private const string HistoryFileName = "history";

	private const string TemporarySaveDefaultLabel = "SavedFromBug";

	private const string TemporarySaveFileName = "saveatthemoment.zks";

	private const string LastAutosaveFileName = "lastAutosave.zks";

	private const string MetadataFileName = "GameMetadata.json";

	private const string CombatLogFileName = "combatLog.txt";

	private const string PersistentDataGameLogFileName = "GameLog.txt";

	private const string PersistentDataGameLogPrevFileName = "GameLogFullPrev.txt";

	private const string PersistentDataGameLogFullFileName = "GameLogFull.txt";

	private const string PersistentDataEditorLogFullFileName = "EditorLogFull.txt";

	private const string PersistentDataEditorLogFileName = "EditorLog.txt";

	private const string PersistentDataGameHistoryLogFileName = "game-history.txt";

	private const string ConsoleLogFullFileName = "ConsoleLogFull.txt";

	private const string ConsoleLogFullPrevFileName = "ConsoleLogFullPrev.txt";

	private readonly CancellationTokenSource m_Cts = new CancellationTokenSource();

	private string m_TemporarySaveLabel = string.Empty;

	private BugContext m_Context;

	private readonly List<BugContext> m_ContextVariants = new List<BugContext>();

	private readonly string m_GameVersion;

	private string m_SelectedFixVersion = "None";

	private FullScreenUIType m_ActiveFullScreenUIType;

	private BlueprintUnit m_HoveredPortraitUnitBlueprint;

	private readonly List<PartyContext.ReportParameterHelper> m_SpellCastHistory = new List<PartyContext.ReportParameterHelper>();

	private readonly List<PartyContext.ReportParameterHelper> m_ItemUseHistory = new List<PartyContext.ReportParameterHelper>();

	private bool m_IsGlobalMapOpened;

	private readonly ReportCombatLogManager m_ReportCombatLogManager;

	private string m_SuggestedAssignee = string.Empty;

	private bool m_IsFeedback;

	private SaveInfo m_SelectedManualSave;

	private string m_InitialAspect;

	private Exception m_Exception;

	private string[] m_ErrorMessages;

	private readonly ReportFilesMd5Manager m_ReportFilesMd5Manager;

	private string m_UiFeatureName = string.Empty;

	private readonly ReportPrivacyManager m_ReportPrivacyManager;

	private readonly IBugreportService _bugreportService;

	private Bugreport<WHBugReportParameters> _currentReport;

	private Dictionary<string, bool> m_labelsDictionary = new Dictionary<string, bool>();

	private List<SaveInfo> m_ManualSaves = new List<SaveInfo>();

	private bool m_WaitForSave;

	public static ReportingUtils Instance => Services.GetInstance<ReportingUtils>();

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	public static string BugReportsPath => Path.Combine(ApplicationPaths.persistentDataPath, "Reports");

	public string CurrentReportFolder => _currentReport?.Directory.FullName ?? string.Empty;

	public BlueprintScriptableObject ExceptionSource
	{
		get
		{
			return ReportingUtilsSourceHolder.Instance.ExceptionSource;
		}
		set
		{
			ReportingUtilsSourceHolder.Instance.ExceptionSource = value;
		}
	}

	public BugreportOptions ReportOptions { get; private set; }

	public string CurrentContextName => m_Context.Type;

	private static string DefaultPath => ApplicationPaths.persistentDataPath;

	public ReportingUtils()
	{
		EventBus.Subscribe(this);
		m_ReportCombatLogManager = new ReportCombatLogManager(ApplicationPaths.persistentDataPath, "combatLog.txt", this);
		Owlcat.Runtime.Core.Logging.Logger.Instance.AddLogger(new ReportingUberLoggerFilter(new UberLoggerFile(GetPlatform().ToLower() + "_reporting_util.txt", null, includeCallStacks: false, 5248000, append: true)));
		Logger.Log("");
		Logger.Log("Instantiate ReportingUtils");
		m_ReportFilesMd5Manager = new ReportFilesMd5Manager();
		ReportFilesMd5Manager reportFilesMd5Manager = m_ReportFilesMd5Manager;
		reportFilesMd5Manager.ReportError = (Action<string>)Delegate.Combine(reportFilesMd5Manager.ReportError, new Action<string>(LogReporterError));
		m_ReportPrivacyManager = new ReportPrivacyManager();
		m_GameVersion = GameVersion.GetVersion();
		CrashReportHandler.SetUserMetadata("Store", StoreManager.Store.ToString());
		_bugreportService = CreateBugreportService();
	}

	public void Dispose()
	{
		if (m_ReportFilesMd5Manager != null)
		{
			ReportFilesMd5Manager reportFilesMd5Manager = m_ReportFilesMd5Manager;
			reportFilesMd5Manager.ReportError = (Action<string>)Delegate.Remove(reportFilesMd5Manager.ReportError, new Action<string>(LogReporterError));
		}
		EventBus.Unsubscribe(this);
		m_ReportCombatLogManager.Dispose();
		m_Cts.Cancel();
		m_Cts.Dispose();
		_bugreportService.Dispose();
	}

	private IBugreportService CreateBugreportService()
	{
		return BugreportServiceBuilder.Create("WH2", ReportBuildInfo.Version, ReportBuildInfo.Revision).Configure(delegate(BugreportConfiguration o)
		{
			o.DeveloperMode = BuildModeUtility.IsDevelopment;
			o.Store = StoreManager.Store.ToString();
			o.SendReportsEnabled = true;
			o.RootDirectory = new DirectoryInfo(BugReportsPath);
		}).ConfigureErrorHandling(delegate(ErrorHandlingConfiguration c)
		{
			c.SetErrorHandler(HandleReportErrors);
		})
			.ConfigureAttachments(delegate(AttachmentsCollection a)
			{
				a.Add(Path.Combine(ApplicationPaths.LogsDir, "GameLog.txt"), required: false).Add(Path.Combine(ApplicationPaths.LogsDir, "GameLogFullPrev.txt"), required: false).Add(Path.Combine(ApplicationPaths.LogsDir, "GameLogFull.txt"), required: false)
					.Add(Path.Combine(Application.dataPath, "..", "EditorLogFull.txt"), Application.isEditor)
					.Add(Path.Combine(Application.dataPath, "..", "EditorLog.txt"), Application.isEditor)
					.Add(SettingsController.Instance.GeneralSettingsProviderPath, "general_settings.json")
					.Add(Path.Combine(ApplicationPaths.LogsDir, "ConsoleLogFull.txt"), required: false)
					.Add(Path.Combine(ApplicationPaths.LogsDir, "ConsoleLogFullPrev.txt"), required: false)
					.Add(m_ReportCombatLogManager, delegate(ReportCombatLogManager combatLogManager, string reportDirectory, CancellationToken _)
					{
						string path = Path.Combine(reportDirectory, "combatLog.txt");
						combatLogManager.CopyFile(path);
						return Task.CompletedTask;
					})
					.Add(async delegate(string reportDirectory, CancellationToken ct)
					{
						string contents = JsonConvert.SerializeObject(GameMetaData.Create(ReportDllChecksumManager.GetDllCRC(), ReportDllChecksumManager.IsUnityModManagerActive()), new JsonSerializerSettings
						{
							PreserveReferencesHandling = PreserveReferencesHandling.None
						});
						await File.WriteAllTextAsync(Path.Combine(reportDirectory, "GameMetadata.json"), contents, ct);
					})
					.Add(this, delegate(ReportingUtils reportingUtils, string reportDirectory, CancellationToken _)
					{
						string archiveFileName = reportingUtils.CreateSaveFileForBugReport(reportDirectory);
						string destinationFileName = Path.Combine(reportDirectory, "history");
						using ZipArchive zipArchive = ZipFile.OpenRead(archiveFileName);
						zipArchive.GetEntry("history")?.ExtractToFile(destinationFileName);
						return Task.CompletedTask;
					})
					.Add(delegate
					{
						Utilities.CreateGameHistoryLog();
						return Task.FromResult(Path.Combine(ApplicationPaths.persistentDataPath, "game-history.txt"));
					}, required: true);
			})
			.Build();
	}

	private string CreateSaveFileForBugReport(string outputPath)
	{
		try
		{
			SaveInfo latestSave = Game.Instance.SaveManager.GetLatestSave();
			if (latestSave == null)
			{
				Game.Instance.SaveManager.UpdateSaveListIfNeeded();
				latestSave = Game.Instance.SaveManager.GetLatestSave();
			}
			Logger.Log("Create save file with name: " + latestSave.FolderName + "/" + latestSave.FileName);
			if (latestSave != null && File.Exists(latestSave.FolderName))
			{
				bool flag = !string.IsNullOrEmpty(m_TemporarySaveLabel) && latestSave.Name.Contains(m_TemporarySaveLabel);
				string text = Path.Combine(outputPath, flag ? "saveatthemoment.zks" : "save.zks");
				File.Copy(latestSave.FolderName, text);
				Logger.Log("Copy save file " + latestSave.FileName + " to " + text);
				if (flag)
				{
					Game.Instance.SaveManager.DeleteSave(latestSave);
				}
				m_TemporarySaveLabel = string.Empty;
				return text;
			}
			if (latestSave != null)
			{
				LogReporterError("Failed to add save file: " + latestSave.Name + " has no real save file on disk");
				return "";
			}
			LogReporterError("Failed to add save file: no latest save in save manager");
			return "";
		}
		catch (Exception ex)
		{
			LogReporterError("Failed to add save file: \n" + ex.Message + "\n" + ex.StackTrace);
			return "";
		}
	}

	public string CopySaveFile([NotNull] SaveInfo saveInfo, [NotNull] string outputPath, string outputName = null)
	{
		try
		{
			string text = Path.Combine(outputPath, outputName ?? (Path.GetFileNameWithoutExtension(saveInfo.FolderName) + ".zks"));
			Logger.Log("Copy save file " + saveInfo.FileName + " to " + text);
			if (File.Exists(saveInfo.FolderName))
			{
				File.Copy(saveInfo.FolderName, text);
				return text;
			}
			LogReporterError("Failed to copy save file: " + saveInfo.Name + " has no real save file on disk");
		}
		catch (Exception ex)
		{
			LogReporterError("Failed to copy save file: \n" + ex.Message + "\n" + ex.StackTrace);
		}
		return "";
	}

	private string SetReportParameters(Bugreport<WHBugReportParameters> report, string email, string uniqueIdentifier, string issueType, string message, List<string> historyLog, string additionalContacts = "", bool isSendMarketing = false)
	{
		try
		{
			BlueprintAreaPart currentAreaPart = CheatsJira.GetCurrentAreaPart();
			string text = CheatsJira.GetCurrentAreaPart()?.StaticScene.SceneName;
			string lightScene = CheatsJira.GetLightScene();
			string text2 = "";
			string text3 = "";
			try
			{
				text2 = new PartyContext(m_ItemUseHistory, m_SpellCastHistory).ToString();
				text3 = new ExtendedContext(historyLog).ToString();
			}
			catch
			{
			}
			try
			{
				text2 = Regex.Unescape(text2);
				text3 = Regex.Unescape(text3);
			}
			catch
			{
			}
			string modifiedSaveFiles;
			if (ReportingCheats.IsDebugSaveFileChecksumEnabled)
			{
				try
				{
					using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
					StringBuilder builder = pooledStringBuilder.Builder;
					modifiedSaveFiles = (from f in report.Directory.GetFiles("*.zks")
						select f.FullName into f
						select m_ReportFilesMd5Manager.GetCompareChecksumsString(f)).Aggregate(builder, (StringBuilder current, string s) => current.Append(s)).ToString();
				}
				catch (Exception ex)
				{
					modifiedSaveFiles = ex.Message;
				}
			}
			else
			{
				modifiedSaveFiles = "check_disabled";
			}
			string text4;
			try
			{
				text4 = new ModContext().ToString();
			}
			catch (Exception ex2)
			{
				text4 = ex2.Message;
			}
			if (string.IsNullOrEmpty(text4))
			{
				text4 = "None";
			}
			string otherContext;
			try
			{
				otherContext = new OtherContext(m_ContextVariants, m_Context).ToString();
			}
			catch (Exception ex3)
			{
				otherContext = ex3.Message;
			}
			List<string> list = new List<string>();
			string coopPlayersCount = "";
			foreach (KeyValuePair<string, bool> item in m_labelsDictionary)
			{
				if (item.Value)
				{
					list.Add(item.Key);
				}
			}
			if (NetworkingManager.IsActive)
			{
				list.Add("Coop");
				coopPlayersCount = NetworkingManager.PlayersCount.ToString();
			}
			if (m_Context.Type == "Debug")
			{
				list.Add("Debug");
			}
			if (m_Context.Type == "Encounter")
			{
				list.Add(m_Context.ContextObject.NameSafe() + "_Encounters");
			}
			if (m_Context.Type == "Dialog")
			{
				switch (m_Context.Aspect)
				{
				case "Code":
				case "Mechanics":
				case "UI":
				case "Sound":
				case "None":
					list.Add("Dialog");
					break;
				}
			}
			if (m_Context.Type == "Item")
			{
				list.Add("Loot");
			}
			if (m_Context.Type == "Career")
			{
				list.Add("Careers");
			}
			if (!string.IsNullOrEmpty(m_UiFeatureName))
			{
				list.Add(ReportingRaycaster.Instance.GetJiraLabel(m_UiFeatureName));
			}
			if (BuildModeUtility.IsDevelopment && m_Context.Aspect != "None" && m_Context.Aspect != "UI" && m_Context.Aspect != "Visual")
			{
				list.Add(m_Context.Aspect);
			}
			if (BuildModeUtility.IsPlayTest)
			{
				list.Add("Playtest");
			}
			string empty = string.Empty;
			string text5 = ((currentAreaPart == null) ? empty : GetChapterNormalOrCrash().ToString());
			if (Regex.IsMatch(text5, "^\\d+$"))
			{
				text5 = "Chapter0" + text5;
			}
			WHBugReportParameters parameters = report.Parameters;
			parameters.BuildDateTime = ReportBuildInfo.DateTime;
			parameters.Context = m_Context.Type;
			parameters.Aspect = m_Context.Aspect;
			parameters.Email = (string.IsNullOrEmpty(email) ? empty : email);
			parameters.Discord = (string.IsNullOrEmpty(additionalContacts) ? empty : additionalContacts);
			parameters.FixVersion = m_SelectedFixVersion;
			parameters.SuggestedAssignee = ((m_SuggestedAssignee != "Assignee to me") ? m_SuggestedAssignee : email.Split("@")[0]);
			parameters.Labels.AddRange(list);
			parameters.IsSendMarketingMats = isSendMarketing;
			parameters.PlayerLanguage = LocalizationManager.Instance.CurrentLocale.ToString();
			parameters.UniqueIdentifier = (string.IsNullOrEmpty(uniqueIdentifier) ? empty : uniqueIdentifier);
			parameters.Blueprint = m_Context.GetContextObjectBlueprintName();
			parameters.BlueprintArea = ((currentAreaPart == null) ? empty : Utilities.GetBlueprintName(currentAreaPart));
			parameters.Chapter = text5;
			parameters.IssueType = (m_IsFeedback ? "Task" : "Bug");
			parameters.Priority = (string.IsNullOrEmpty(issueType) ? empty : issueType);
			parameters.StaticScene = (string.IsNullOrEmpty(text) ? empty : text);
			parameters.LightScene = (string.IsNullOrEmpty(lightScene) ? empty : lightScene);
			parameters.MainCharacter = $"{Utilities.GetBlueprintName(GetBlueprintRace())}/{GetGender()}";
			parameters.CurrentDialog = m_Context.GetDialogGuid();
			parameters.AreaDesigner = Utilities.GetDesigner(CheatsJira.GetCurrentArea());
			parameters.ExtendedContext = text3;
			parameters.PartyContext = text2;
			parameters.OtherContext = otherContext;
			parameters.Cutscenes = CheatsJira.GetCutscenesInfo();
			parameters.ModifiedSaveFiles = modifiedSaveFiles;
			parameters.ModManagerMods = text4;
			parameters.ControllerModeType = Convert.ToString(Game.Instance.ControllerMode);
			parameters.Platform = ApplicationHelper.RunningPlatform;
			parameters.ConsoleHardwareType = GetConsoleHardwareType();
			parameters.Exception = JsonConvert.SerializeObject(m_Exception);
			parameters.CoopPlayersCount = coopPlayersCount;
			if (CameraRig.Instance != null)
			{
				parameters.CameraPosition = Utilities.FormatPositionAndRotation(CameraRig.Instance.transform);
			}
			string header = m_Context.GetHeader();
			string contextLink = m_Context.GetContextLink();
			message = AddErrorMessages(message);
			message = message.Replace("\v", "\r\n");
			string text6 = BuildScreenshotAttachment();
			string text7 = BuildAttachmentsHyperlinks("save.zks", "saveatthemoment.zks", "lastAutosave.zks", m_SelectedManualSave?.FileName);
			string text8 = ((text7.Length > 0) ? ("Save files: " + text7) : "");
			string text9 = BuildLogDescription();
			string[] array = message.Split("\n");
			string text10 = array[0];
			string text11 = string.Join("\n", array, 1, array.Length - 1);
			return header + " " + text10 + "\n\n" + text11 + "\n\n" + text9 + "\n\n" + text8 + "\n" + text6 + "\n\n" + contextLink;
		}
		catch (Exception ex4)
		{
			LogReporterError("Failed create parameters file: \n" + ex4.Message + "\n" + ex4.StackTrace);
			return message;
		}
		static string GetConsoleHardwareType()
		{
			return string.Empty;
		}
	}

	private string BuildScreenshotAttachment(string format = "thumbnail")
	{
		if (TryBuildPictureAttachment("edited_screen.png", out var attachment, format))
		{
			return attachment;
		}
		if (TryBuildPictureAttachment("screen.png", out var attachment2, format))
		{
			return attachment2;
		}
		return "";
	}

	private bool TryBuildPictureAttachment(string pictureName, out string attachment, string format = "thumbnail")
	{
		if (File.Exists(Path.Combine(CurrentReportFolder, pictureName)))
		{
			attachment = "!" + pictureName + "|" + format + "!";
			return true;
		}
		attachment = null;
		return false;
	}

	private string BuildAttachmentsHyperlinks(params string[] attachmentsNames)
	{
		if (attachmentsNames == null || attachmentsNames.Length == 0)
		{
			return "";
		}
		IEnumerable<string> source = attachmentsNames.Where((string attachmentName) => !string.IsNullOrWhiteSpace(attachmentName) && File.Exists(Path.Combine(CurrentReportFolder, attachmentName)));
		return string.Join(", ", source.Select((string a) => "[^" + a + "]"));
	}

	private string BuildLogDescription()
	{
		if (!BuildModeUtility.IsDevelopment)
		{
			return "";
		}
		if (m_Context.Aspect != "LogError")
		{
			return "";
		}
		if (UberLoggerAppWindow.Instance == null)
		{
			return "";
		}
		LogInfo selectedLog = UberLoggerAppWindow.Instance.GetSelectedLog();
		if (selectedLog == null)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("-------------- Call stack --------------");
		stringBuilder.AppendLine("{code:C#}");
		stringBuilder.Append(selectedLog.Message);
		if (selectedLog.Callstack != null)
		{
			foreach (LogStackFrame item in selectedLog.Callstack)
			{
				stringBuilder.AppendLine(item.GetFormattedMethodName());
			}
		}
		stringBuilder.Append("{code}\n\n");
		return stringBuilder.ToString();
	}

	private static BlueprintRace GetBlueprintRace()
	{
		return (Game.Instance?.Player.MainCharacterEntity)?.Blueprint.Race;
	}

	private static Gender GetGender()
	{
		return (Game.Instance?.Player.MainCharacterEntity)?.Blueprint.Gender ?? Gender.Male;
	}

	private void SaveScreenshot(Texture2D screenShot)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(CurrentReportFolder))
			{
				Logger.Error("Can't save screenshot: Report Directory not set");
				return;
			}
			string text = Path.Combine(CurrentReportFolder, "screen.png");
			if (screenShot != null)
			{
				byte[] bytes = screenShot.EncodeToPNG();
				File.WriteAllBytes(text, bytes);
			}
			string text2 = Path.Combine(ApplicationPaths.persistentDataPath, "crash_screen.png");
			if (File.Exists(text2))
			{
				File.Move(text2, text);
			}
		}
		catch (Exception ex)
		{
			LogReporterError("Failed create screenshot file: \n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	private Texture2D CreateScreenshotSafe()
	{
		try
		{
			return ScreenCapture.CaptureScreenshotAsTexture();
		}
		catch (Exception ex)
		{
			Logger.Exception(ex, "Failed create screenshot");
			return null;
		}
	}

	private IEnumerator CreateSave()
	{
		try
		{
			Logger.Log("Create save for bug report");
			m_TemporarySaveLabel = string.Empty;
			if (Game.Instance.SaveManager.IsSaveAllowed(SaveInfo.SaveType.Bugreport))
			{
				m_TemporarySaveLabel = "SavedFromBug";
				SaveInfo saveInfo = Game.Instance.SaveManager.CreateNewSave(m_TemporarySaveLabel);
				saveInfo.Type = SaveInfo.SaveType.Bugreport;
				m_WaitForSave = true;
				Game.Instance.GameCommandQueue.SaveGame(saveInfo, null, delegate
				{
					m_WaitForSave = false;
				});
			}
		}
		catch (Exception ex)
		{
			LogReporterError("Failed create save: \n" + ex.Message + "\n" + ex.StackTrace);
		}
		while (m_WaitForSave)
		{
			yield return null;
		}
	}

	private void CopyLastAutosave()
	{
		try
		{
			Logger.Log("Copy last autosave for bug report");
			SaveInfo latestSave = Game.Instance.SaveManager.GetLatestSave((SaveInfo save) => save.Type == SaveInfo.SaveType.Auto && save.GameId == Game.Instance.Player.GameId);
			if (latestSave == null)
			{
				Logger.Log("No autosave found");
			}
			else if (string.IsNullOrEmpty(latestSave.FolderName))
			{
				Logger.Log("Folder name is " + ((latestSave.FolderName == null) ? "<null>" : "empty"));
			}
			else
			{
				CopySaveFile(latestSave, CurrentReportFolder, "lastAutosave.zks");
			}
		}
		catch (Exception ex)
		{
			LogReporterError("Failed copy last autoSave: \n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	private void AddCrushDump()
	{
		string @string = PlayerPrefs.GetString("LatestCrashdump", "");
		Logger.Log("Add crush dump for bug report");
		string[] array = new string[3] { "crash.dmp", "error.log", "Player.log" };
		foreach (string path in array)
		{
			try
			{
				string text = Path.Combine(@string, path);
				if (File.Exists(text))
				{
					File.Copy(text, Path.Combine(CurrentReportFolder, path), overwrite: true);
				}
			}
			catch (Exception ex)
			{
				LogReporterError("Failed copy file: \n" + ex.Message + "\n" + ex.StackTrace);
			}
		}
	}

	private IEnumerator LoadReportOptions()
	{
		Task<BugreportOptions> task = _bugreportService.GetReportOptions(m_Cts.Token);
		yield return new WaitUntil(() => task.IsCompleted);
		if (task.IsCompletedSuccessfully)
		{
			ReportOptions = task.Result;
		}
		else
		{
			Logger.Error(task.Exception, "Failed to load report options");
		}
	}

	private static IEnumerable<BugContext> MakeBugContextList(params BlueprintScriptableObject[] objects)
	{
		yield break;
	}

	private void CollectBugContextVariants(NewReportOptions options)
	{
		try
		{
			m_Context = null;
			m_ContextVariants.Clear();
			if (options.WithCrashDump)
			{
				m_ContextVariants.Add(new BugContext("Crash"));
				m_Context = m_ContextVariants[0];
				return;
			}
			if (m_Exception != null)
			{
				m_ContextVariants.Add(new BugContext("Exception"));
				m_Context = m_ContextVariants[0];
				return;
			}
			BlueprintDialog blueprintDialog = Game.Instance.Controllers?.DialogController.Dialog;
			if (blueprintDialog != null)
			{
				m_ContextVariants.Add(new BugContext(blueprintDialog));
			}
			m_ContextVariants.AddRange(MakeBugContextList(CheatsJira.Tooltip()));
			if (ReportUIDetector.TryGetUiFeatureName(out var featureName))
			{
				m_ContextVariants.Add(CreateUIContextFromFeature(featureName));
			}
			if (!string.IsNullOrEmpty(m_UiFeatureName))
			{
				try
				{
					List<string> source = m_UiFeatureName.Split(' ').ToList();
					m_ContextVariants.AddRange(source.Select(CreateUIContextFromFeature));
				}
				catch
				{
					m_UiFeatureName = string.Empty;
				}
			}
			BlueprintUnit blueprintUnit = null;
			bool flag = false;
			if (m_ActiveFullScreenUIType == FullScreenUIType.Unknown)
			{
				blueprintUnit = m_HoveredPortraitUnitBlueprint;
				if (blueprintUnit == null)
				{
					BaseUnitEntity unitUnderMouse = Utilities.GetUnitUnderMouse();
					if (unitUnderMouse != null)
					{
						flag = unitUnderMouse.IsInCombat;
						blueprintUnit = unitUnderMouse.Blueprint;
						if (blueprintUnit != null)
						{
							m_ContextVariants.Add(new BugContext(blueprintUnit));
						}
					}
				}
				else
				{
					flag = true;
					m_ContextVariants.Add(new BugContext(blueprintUnit));
				}
			}
			if (GameUIState.Instance.IsInMainMenu || Game.Instance.CurrentModeType == GameModeType.GameOver || m_ActiveFullScreenUIType == FullScreenUIType.Journal || m_ActiveFullScreenUIType == FullScreenUIType.Encyclopedia || m_ActiveFullScreenUIType == FullScreenUIType.Chargen || Game.Instance.CurrentModeType == GameModeType.Default)
			{
				BlueprintScriptableObject underMouseBlueprint = ReportingRaycaster.Instance.GetUnderMouseBlueprint();
				if (underMouseBlueprint != null)
				{
					BugContext item = new BugContext(underMouseBlueprint);
					m_ContextVariants.Add(item);
				}
			}
			if (options.Tooltip != null)
			{
				try
				{
					TooltipBaseTemplate mainTemplate = options.Tooltip.MainTemplate;
					BugContext item2 = BugReportService.GetAbilityContext(mainTemplate);
					m_ContextVariants.Add(item2);
				}
				catch (Exception ex)
				{
					PFLog.Default.Log("BugReport collect tooltip exception: " + ex.Message + "\n" + ex.StackTrace);
				}
			}
			BlueprintArea currentArea = CheatsJira.GetCurrentArea();
			if (currentArea != null)
			{
				m_ContextVariants.Add(new BugContext(currentArea));
			}
			if (options.OtherUiFeatureName == "Ingame Menu")
			{
				BugContext item3 = new BugContext("Interface")
				{
					ActiveFullScreenUIType = m_ActiveFullScreenUIType,
					OtherUiFeature = options.OtherUiFeatureName
				};
				m_ContextVariants.Add(item3);
			}
			if (Game.Instance.Player.IsInCombat)
			{
				m_ContextVariants.Add(new BugContext("SurfaceCombat"));
			}
			if (NetworkingManager.IsActive)
			{
				m_ContextVariants.Add(new BugContext("Coop"));
			}
			if (PhotonManager.Initialized && PhotonManager.Sync.HasDesync)
			{
				m_ContextVariants.Add(new BugContext("Desync"));
			}
			m_ContextVariants.Remove((BugContext x) => x.Type == "Area" && x.ContextObject == null);
			m_ContextVariants.Sort();
			if (!flag && blueprintUnit != null)
			{
				List<BugContext> collection = new List<BugContext>(m_ContextVariants);
				try
				{
					BugContext bugContext = m_ContextVariants.FirstOrDefault((BugContext x) => x.Type == "Area");
					if (bugContext != null)
					{
						List<BugContext> list = m_ContextVariants.SkipWhile(delegate(BugContext x)
						{
							string type = x.Type;
							int num3;
							switch (type)
							{
							default:
								num3 = ((!(type == "Item")) ? 1 : 0);
								break;
							case "None":
							case "Crash":
							case "Exception":
							case "Dialog":
								num3 = 0;
								break;
							}
							return num3 == 0;
						}).ToList();
						list.Remove(bugContext);
						m_ContextVariants.Clear();
						m_ContextVariants.Add(bugContext);
						m_ContextVariants.AddRange(list);
					}
				}
				catch
				{
					m_ContextVariants.Clear();
					m_ContextVariants.AddRange(collection);
				}
			}
			if (m_ContextVariants.Count((BugContext x) => x.Type == "Unit" && x.ContextObject != null) > 1)
			{
				int num = 0;
				for (int num2 = m_ContextVariants.Count - 1; num2 > 0; num2--)
				{
					if (m_ContextVariants[num2].Type == "Unit")
					{
						num = num2;
						break;
					}
				}
				if (num > 0)
				{
					m_ContextVariants.RemoveAt(num);
				}
			}
			if (options.OtherUiFeatureName == "Bark")
			{
				BugContext item4 = new BugContext("Interface")
				{
					ActiveFullScreenUIType = m_ActiveFullScreenUIType,
					OtherUiFeature = options.OtherUiFeatureName
				};
				m_ContextVariants.Add(item4);
			}
			if (ExceptionSource != null && ExceptionSource is BlueprintCutscene)
			{
				m_ContextVariants.Add(new BugContext(ExceptionSource));
				ExceptionSource = null;
			}
			BugContext bugContext2 = m_ContextVariants.FirstOrDefault((BugContext x) => x.UiFeature == "EscMenu");
			if (bugContext2 != null)
			{
				m_ContextVariants.Remove(bugContext2);
				m_ContextVariants.Insert(0, bugContext2);
			}
			if (GameVersion.Mode == BuildMode.Development)
			{
				m_ContextVariants.Add(new BugContext("Generic"));
				m_ContextVariants.Add(new BugContext("Debug"));
			}
			if (m_Context == null && m_ContextVariants.Count > 0)
			{
				m_Context = m_ContextVariants[0];
			}
		}
		catch (Exception ex2)
		{
			LogReporterError("Failed to collect bug contexts: \n" + ex2.Message + "\n" + ex2.StackTrace);
		}
		finally
		{
			if (m_Context == null)
			{
				m_Context = new BugContext("None");
				m_ContextVariants.Clear();
				m_ContextVariants.Add(m_Context);
			}
			m_ContextVariants.Sort(new BugContextComparer());
		}
	}

	private void CheckForActiveLogger()
	{
		if (BuildModeUtility.IsDevelopment && !(UberLoggerAppWindow.Instance == null) && UberLoggerAppWindow.Instance.IsShown && UberLoggerAppWindow.Instance.GetSelectedLog() != null)
		{
			m_InitialAspect = "LogError";
			UberLoggerAppWindow.Instance.Hide();
		}
	}

	public IEnumerator MakeNewReport(bool withScreenshot, bool withSave, bool withCrashDump)
	{
		TooltipData tooltipData = MouseHoverBlueprintSystem.Instance.TooltipData;
		string otherUiFeatureName = ReportingRaycaster.Instance.GetOtherUiFeatureName();
		Texture2D screenshot = (withScreenshot ? CreateScreenshotSafe() : null);
		using NewReportOptions options = new NewReportOptions(tooltipData, screenshot, otherUiFeatureName, withSave, withCrashDump);
		yield return MakeNewReport(options);
	}

	public IEnumerator MakeNewReport(NewReportOptions options)
	{
		Logger.Log("Create new bug report");
		CheckForActiveLogger();
		if (ReportOptions == null)
		{
			yield return LoadReportOptions();
		}
		if (options.WithSave)
		{
			yield return CreateSave();
		}
		Task<Bugreport<WHBugReportParameters>?> newReportTask = _bugreportService.CreateReport<WHBugReportParameters>();
		yield return new WaitUntil(() => newReportTask.IsCompleted);
		if (!newReportTask.IsCompletedSuccessfully)
		{
			AggregateException exception = newReportTask.Exception;
			LogReporterError("Failed to MakeNewReport: \n" + exception?.Message + "\n" + exception?.StackTrace);
			yield break;
		}
		try
		{
			_currentReport = newReportTask.Result;
			SaveScreenshot(options.Screenshot);
			CollectBugContextVariants(options);
		}
		catch (Exception ex)
		{
			LogReporterError("Failed to MakeNewReport: \n" + ex.Message + "\n" + ex.StackTrace);
		}
		try
		{
			if (options.WithSave)
			{
				CopyLastAutosave();
			}
			if (options.WithCrashDump)
			{
				AddCrushDump();
			}
		}
		catch (Exception ex2)
		{
			LogReporterError("Failed to MakeNewReport: \n" + ex2.Message + "\n" + ex2.StackTrace);
		}
	}

	public async void SendReport(string message, string email, string uniqueIdentifier, string issueType, string additionalContacts = "", bool isSendMarketing = false, string id = null)
	{
		if (id == null && NetworkingManager.IsMultiplayer)
		{
			id = Guid.NewGuid().ToString("N");
			message = "[" + id + "] " + message;
			PFLog.Net.Log($"New multiplayer bug report id={id} player={NetworkingManager.LocalNetPlayer.Index}/{NetworkingManager.PlayersCount}");
		}
		if (_currentReport == null)
		{
			Logger.Error("Can't send empty report.");
			return;
		}
		try
		{
			List<string> historyLog = new List<string>();
			string historyLogPath = Path.Combine(CurrentReportFolder, "history");
			if (File.Exists(historyLogPath))
			{
				historyLog = (await File.ReadAllLinesAsync(historyLogPath)).ToList();
				File.Delete(historyLogPath);
			}
			SettingsController.Instance.ConfirmAllTempValues();
			SettingsController.Instance.SaveAll();
			if (m_SelectedManualSave != null)
			{
				CopySaveFile(m_SelectedManualSave, CurrentReportFolder, m_SelectedManualSave.FileName);
			}
			string message2 = SetReportParameters(_currentReport, email, uniqueIdentifier, issueType, message, historyLog, additionalContacts, isSendMarketing);
			_currentReport.Message = message2;
			await _currentReport.Send();
			PlayerPrefs.SetInt("BugReportMarketingMaterialsToggle", isSendMarketing ? 1 : 0);
		}
		catch (Exception ex)
		{
			Logger.Exception(ex, "Failed send report {0}", _currentReport.Id);
			_currentReport.Delete();
		}
		finally
		{
			_currentReport = null;
		}
		PhotonManager.BugReport.Sync(id, message, issueType);
	}

	public void DiscardReport()
	{
		_currentReport?.Delete();
		_currentReport = null;
	}

	public void Clear()
	{
		try
		{
			if (!string.IsNullOrEmpty(m_TemporarySaveLabel))
			{
				SaveInfo latestSave = Game.Instance.SaveManager.GetLatestSave();
				if (latestSave != null && File.Exists(latestSave.FolderName) && latestSave.Name.Contains(m_TemporarySaveLabel))
				{
					Game.Instance.SaveManager.DeleteSave(latestSave);
				}
				m_TemporarySaveLabel = string.Empty;
			}
		}
		catch (Exception ex)
		{
			Logger.Exception(ex, "Failed to clear");
		}
	}

	public void InitializeManualSaves(Action<SaveInfo> actionWithSave)
	{
		m_ManualSaves.Clear();
		foreach (SaveInfo item in from item in Game.Instance.SaveManager
			orderby item.SystemSaveTime descending
			where item.Type == SaveInfo.SaveType.Manual
			select item)
		{
			m_ManualSaves.Add(item);
			actionWithSave(item);
		}
	}

	public (string Description, List<(string Aspect, int AspectIndex, string Assignee)> Assignees)[] GetContextDescriptions()
	{
		List<(string, List<(string, int, string)>)> list = new List<(string, List<(string, int, string)>)>();
		foreach (BugContext contextVariant in m_ContextVariants)
		{
			string contextDescription = contextVariant.GetContextDescription();
			List<(string, int, string)> contextAspectAssignees = contextVariant.GetContextAspectAssignees();
			contextAspectAssignees.Add((null, -1, "Assignee to me"));
			list.Add((contextDescription, contextAspectAssignees));
		}
		if (list.Count == 0)
		{
			list.Add(("None", null));
		}
		return list.ToArray();
	}

	public int GetInitialAspectIndex()
	{
		if (m_InitialAspect == null)
		{
			return 0;
		}
		int aspectIndex = m_Context.GetAspectIndex(m_InitialAspect);
		m_InitialAspect = null;
		return aspectIndex;
	}

	public void SelectManualSave(int saveIdx)
	{
		m_SelectedManualSave = ((saveIdx >= 0 && saveIdx < m_ManualSaves.Count) ? m_ManualSaves[saveIdx] : null);
	}

	public void SelectContext(int contextIdx)
	{
		if (contextIdx >= 0 && contextIdx < m_ContextVariants.Count)
		{
			m_Context = m_ContextVariants[contextIdx];
		}
	}

	public void SelectAspect(int aspectIdx)
	{
		m_Context?.SelectAspect(aspectIdx);
	}

	public void SelectAssignee(string assignee)
	{
		m_SuggestedAssignee = assignee;
	}

	public void SelectIsFeedback(bool isFeedback)
	{
		m_IsFeedback = isFeedback;
	}

	public void SelectFixVersion(int versionIdx)
	{
		if (versionIdx >= 0 && versionIdx < ReportOptions.Jira.FixVersions.Count)
		{
			m_SelectedFixVersion = ReportOptions.Jira.FixVersions[versionIdx];
		}
	}

	public IEnumerable<string> GetCurrentAspects()
	{
		IEnumerable<string> enumerable = m_Context?.Aspects;
		return enumerable ?? Enumerable.Empty<string>();
	}

	public int GetCurrentFixVersionIndex()
	{
		return ((IEnumerable<string>)ReportOptions.Jira.FixVersions).IndexOf(m_SelectedFixVersion);
	}

	public bool CheckCrashDumpFound()
	{
		try
		{
			string fullPath = Path.GetFullPath(Path.Combine(ApplicationPaths.persistentDataPath, "..\\..\\..\\Local\\Temp\\Owlcat Games\\Pathfinder Wrath Of The Righteous\\Crashes\\"));
			if (Application.platform == RuntimePlatform.OSXPlayer)
			{
				fullPath = Path.GetFullPath(Path.Combine(ApplicationPaths.persistentDataPath, "\\Users\\g\\Library\\Logs\\DiagnosticReports\\"));
			}
			string @string = PlayerPrefs.GetString("LatestCrashdump", "");
			if (@string.IsNullOrEmpty())
			{
				if (Directory.Exists(fullPath))
				{
					foreach (string item in Directory.EnumerateDirectories(fullPath))
					{
						Directory.Delete(item, recursive: true);
					}
				}
				PlayerPrefs.SetString("LatestCrashdump", "we are ok, no dumps!");
				PlayerPrefs.Save();
			}
			else
			{
				if (!Directory.Exists(fullPath))
				{
					return false;
				}
				IEnumerable<string> source = Directory.EnumerateDirectories(fullPath);
				if (source.Empty())
				{
					return false;
				}
				string text = source.OrderBy((string x) => Directory.GetCreationTime(x)).Last();
				if (@string != text)
				{
					PlayerPrefs.SetString("LatestCrashdump", text);
					PlayerPrefs.Save();
					return true;
				}
			}
		}
		catch (Exception ex)
		{
			LogReporterError("Failed to check for available crash dumps: \n" + ex.Message + "\n" + ex.StackTrace);
		}
		return false;
	}

	public void HandleFullScreenUiChanged(bool active, FullScreenUIType fullScreenUIType)
	{
		m_ActiveFullScreenUIType = (active ? fullScreenUIType : FullScreenUIType.Unknown);
	}

	public void HandleFullScreenUIItJustWorks(bool active, FullScreenUIType fullScreenUIType)
	{
		HandleFullScreenUiChanged(active, fullScreenUIType);
	}

	public void LogReporterError(string message)
	{
		Logger.Log(message);
	}

	public void HandlePortraitHover(bool hover)
	{
		m_HoveredPortraitUnitBlueprint = (hover ? EventInvokerExtensions.BaseUnitEntity.Blueprint : null);
	}

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
		string contextName = evt.Ability.Blueprint.OriginalBlueprint.NameSafe();
		string assetGuid = evt.Ability.Blueprint.OriginalBlueprint.AssetGuid;
		string partyMemberName = evt.Ability.Caster.GetDescriptionOptional()?.Name ?? evt.Ability.Caster.Blueprint.Name;
		PartyContext.ReportParameterHelper item = new PartyContext.ReportParameterHelper
		{
			Guid = assetGuid,
			ContextName = contextName,
			PartyMemberName = partyMemberName,
			ContextType = BugContext.InnerContextType.Spell.ToString()
		};
		if (m_SpellCastHistory.Count > 500)
		{
			m_SpellCastHistory.RemoveRange(0, 100);
		}
		m_SpellCastHistory.Add(item);
		string text = evt.Ability.SourceItem?.Blueprint.AssetGuid ?? string.Empty;
		if (!(text == string.Empty))
		{
			string contextName2 = evt.Ability.SourceItem?.Blueprint.NameSafe();
			PartyContext.ReportParameterHelper item2 = new PartyContext.ReportParameterHelper
			{
				Guid = text,
				ContextName = contextName2,
				PartyMemberName = partyMemberName,
				ContextType = BugContext.InnerContextType.Item.ToString()
			};
			if (m_ItemUseHistory.Count > 500)
			{
				m_ItemUseHistory.RemoveRange(0, 100);
			}
			m_ItemUseHistory.Add(item2);
		}
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
	}

	private static BaseUnitEntity GetSelectedPartyCharUnitEntityData()
	{
		BaseUnitEntity value = Game.Instance.Controllers.SelectionCharacter.SelectedUnitInUI.Value;
		if (!value.Faction.IsPlayer)
		{
			return null;
		}
		return value;
	}

	public void HandleException(Exception exception)
	{
		m_Exception = exception;
	}

	public void HandleErrorMessages(string[] errorMessages)
	{
		m_ErrorMessages = errorMessages;
	}

	public void HandleBugReportOpen(bool showBugReportOnly)
	{
	}

	public void HandleBugReportCanvasHotKeyOpen()
	{
	}

	public void HandleBugReportShow()
	{
	}

	public void HandleBugReportHide()
	{
		m_Exception = null;
		m_ErrorMessages = null;
	}

	public void HandleUIElementFeature(string featureName)
	{
		m_UiFeatureName = featureName;
	}

	public void HandleCrushDumpReport()
	{
	}

	private string AddErrorMessages(string message)
	{
		if (m_ErrorMessages == null || m_ErrorMessages.Length == 0)
		{
			return message;
		}
		try
		{
			string[] array = m_ErrorMessages[0].Split('\n');
			string text = array[0];
			if (text.StartsWith("Error: "))
			{
				text += array[1];
			}
			if (text.Contains("Object reference not set to an instance of an object"))
			{
				text = "Error: Object reference not set at " + array[1].Split(' ')[1];
			}
			return text + "\n" + message + "\n\n\n----------- Call stack -----------\n{noformat}\n" + string.Join('\n', m_ErrorMessages) + "\n{noformat}";
		}
		catch
		{
			return string.Join('\n', m_ErrorMessages) + "\n" + message;
		}
	}

	private static string HumanReadableNumbers(long input)
	{
		try
		{
			string text = input.ToString();
			for (int num = text.Length - 3; num >= 0; num -= 3)
			{
				text = text.Insert(num, " ");
			}
			return text + " B";
		}
		catch
		{
			return input.ToString();
		}
	}

	public (bool promo, bool priv) PrivacyStuffGetEmailAgreements(string email)
	{
		return m_ReportPrivacyManager.GetEmailAgreements(email);
	}

	public void PrivacyStuffManage(string email, bool promo, bool privacy, bool isSend)
	{
		m_ReportPrivacyManager.ManageClose(email, promo, privacy, isSend);
	}

	public bool IsReportWithMods(bool isCrash, string callstack = null)
	{
		try
		{
			string[] source = new string[5] { "_Patch", "Postfix", "Prefix", "(wrapper dynamic-method)", "ToyBox" };
			if (isCrash)
			{
				string @string = PlayerPrefs.GetString("LatestCrashdump", "");
				string[] array = new string[2]
				{
					Path.Combine(@string, "error.log"),
					Path.Combine(@string, "Player.log")
				};
				foreach (string path in array)
				{
					if (File.Exists(path))
					{
						string @object = File.ReadAllText(path);
						if (source.Any(@object.Contains))
						{
							return true;
						}
					}
				}
				return false;
			}
			return (!string.IsNullOrEmpty(callstack) && source.Any(callstack.Contains)) || (m_Exception != null && source.Any(m_Exception.StackTrace.Contains));
		}
		catch
		{
			return false;
		}
	}

	public bool IsDiskFreeSpaceCrash(bool isCrash)
	{
		try
		{
			if (isCrash)
			{
				string @string = PlayerPrefs.GetString("LatestCrashdump", "");
				string[] array = new string[2]
				{
					Path.Combine(@string, "error.log"),
					Path.Combine(@string, "Player.log")
				};
				foreach (string path in array)
				{
					if (File.Exists(path))
					{
						string text = File.ReadAllText(path);
						if (text.Contains("Error: array too small. numBytes/offset wrong.\nParameter name: array\n  at System.IO.FileStream.Dispose") || text.Contains("Error: array too small. numBytes/offset wrong.\r\nParameter name: array\r\n  at System.IO.FileStream.Dispose"))
						{
							return true;
						}
					}
				}
			}
			else
			{
				DriveInfo[] drives = DriveInfo.GetDrives();
				string value = DefaultPath.Substring(0, 2);
				DriveInfo[] array2 = drives;
				foreach (DriveInfo driveInfo in array2)
				{
					if (driveInfo.Name.Contains(value))
					{
						return driveInfo.AvailableFreeSpace < 31457280;
					}
				}
			}
			return false;
		}
		catch (Exception ex)
		{
			LogReporterError("Bugreport: " + ex.Message + "\n" + ex.StackTrace);
			return false;
		}
	}

	public bool IsOutOfMemoryCrash()
	{
		try
		{
			string @string = PlayerPrefs.GetString("LatestCrashdump", "");
			string[] array = new string[2]
			{
				Path.Combine(@string, "error.log"),
				Path.Combine(@string, "Player.log")
			};
			foreach (string path in array)
			{
				if (File.Exists(path) && File.ReadAllText(path).Contains("Could not allocate memory:"))
				{
					return true;
				}
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	public bool IsCorruptedBundleCrash(bool isCrash)
	{
		try
		{
			string path = (isCrash ? "GameLogFullPrev.txt" : "GameLogFull.txt");
			string path2 = Path.Combine(ApplicationPaths.persistentDataPath, path);
			if (File.Exists(path2))
			{
				string @object = File.ReadAllText(path2);
				if (new string[3] { "couldn't be loaded because it has not been added to the build settings or the AssetBundle has not been loaded", "Failed to read data for the AssetBundle", "is corrupted! Remove it and launch unity again!" }.Any(@object.Contains))
				{
					return true;
				}
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	private int GetChapterNormalOrCrash()
	{
		try
		{
			if (m_Context.Type != "Exception" && m_Context.Type != "Crash")
			{
				return Game.Instance.Player.Chapter;
			}
			if (string.IsNullOrEmpty(CurrentReportFolder))
			{
				return -1;
			}
			string[] files = Directory.GetFiles(CurrentReportFolder, "*.zks");
			foreach (string path in files)
			{
				string archiveFileName = Path.Combine(path2: Path.GetFileName(path), path1: Path.GetDirectoryName(path));
				try
				{
					using ZipArchive zipArchive = ZipFile.OpenRead(archiveFileName);
					foreach (ZipArchiveEntry item in zipArchive.Entries.Where((ZipArchiveEntry e) => Path.GetFileNameWithoutExtension(e.Name) == "player"))
					{
						string extension = Path.GetExtension(item.Name);
						if (extension == ".json")
						{
							using (Stream stream = item.Open())
							{
								return StandardSerializer.DeserializeFromJson<Player>(new JsonInputArchive(stream)).Chapter;
							}
						}
						if (extension == ".owl")
						{
							using (Stream data = item.Open())
							{
								return StandardSerializer.DeserializeFromBinary<Player>(new BinaryInputArchive(data)).Chapter;
							}
						}
					}
				}
				catch (Exception ex)
				{
					LogReporterError("Report Getting Chapter Exception: " + ex.Message + "\n" + ex.StackTrace);
				}
			}
			return -1;
		}
		catch
		{
			return -1;
		}
	}

	public void FillLabelsDictionary()
	{
		if (m_labelsDictionary.Count > 0)
		{
			return;
		}
		foreach (string label in ReportOptions.Jira.Labels)
		{
			m_labelsDictionary.Add(label, value: false);
		}
	}

	public Dictionary<string, bool> GetLabelsList()
	{
		return m_labelsDictionary;
	}

	public void ResetLabelsList()
	{
		foreach (string item in m_labelsDictionary.Keys.ToList())
		{
			m_labelsDictionary[item] = false;
		}
	}

	public void LabelChangeValue(string label, bool isOn)
	{
		if (!m_labelsDictionary.ContainsKey(label))
		{
			LogReporterError("Cannot find label: " + label);
		}
		else
		{
			m_labelsDictionary[label] = isOn;
		}
	}

	private string GetPlatform()
	{
		if (Application.isEditor)
		{
			return "Editor";
		}
		return Application.platform.ToString("G").Replace("Player", "");
	}

	private static void HandleReportErrors(ErrorKind errorKind, string message, Exception exception)
	{
		if (errorKind == ErrorKind.Fatal)
		{
			Logger.Error(exception, message);
		}
		else
		{
			Logger.Warning(exception, message);
		}
	}

	private static BugContext CreateUIContextFromFeature(string featureName)
	{
		return new BugContext("Interface")
		{
			UiFeature = featureName
		};
	}
}
