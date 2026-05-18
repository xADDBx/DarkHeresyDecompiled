using System;
using System.Collections;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.Networking;

namespace Kingmaker.Code.UI.MVVM;

public class MainMenuWelcomeWidgetVM : ViewModel, ILocalizationHandler, ISubscriber
{
	private readonly ReactiveCommand<Unit> m_LanguageChanged = new ReactiveCommand<Unit>();

	private readonly MainMenuVM m_MainMenu;

	public Observable<Unit> LanguageChanged => m_LanguageChanged;

	private BlueprintFeedbackConfig FeedbackConfig => UIConfig.Instance.FeedbackConfig;

	public MainMenuWelcomeWidgetVM(MainMenuVM mainMenu)
	{
		m_MainMenu = mainMenu;
		EventBus.Subscribe(this).AddTo(this);
	}

	public void OpenUrl(FeedbackPopupItemType itemType)
	{
		FeedbackPopupItem feedbackPopupItem = FeedbackPopupConfigLoader.Instance.Items.FirstOrDefault((FeedbackPopupItem i) => i.ItemType == itemType);
		FeedbackConfig.TryGetFallbackValue(FeedbackPopupItemType.Website, out var item);
		Application.OpenURL((feedbackPopupItem != null) ? feedbackPopupItem.Url : item.Url);
	}

	private static bool TryGetIntroductoryLocalFileText(string path, out string text)
	{
		text = string.Empty;
		if (!File.Exists(path))
		{
			return false;
		}
		try
		{
			LocalizedStringData localizedStringData = JsonConvert.DeserializeObject<LocalizedStringData>(File.ReadAllText(path));
			if (localizedStringData == null)
			{
				PFLog.UI.Error("Introductory Text File is bad on the path " + path);
				return false;
			}
			text = localizedStringData.GetText(LocalizationManager.Instance.CurrentLocale);
			return true;
		}
		catch (Exception ex)
		{
			PFLog.UI.Exception(ex);
			return false;
		}
	}

	public void GetIntroductoryText(Action<string> callback)
	{
		string uRL = FeedbackConfig.IntroductoryTextDefault.URL;
		string fallbackFilename = FeedbackConfig.IntroductoryTextDefault.FallbackFilename;
		MainThreadDispatcher.StartCoroutine(DownloadText(uRL, callback, fallbackFilename));
	}

	private static IEnumerator DownloadText(string url, Action<string> callback, string fallbackFileName)
	{
		using UnityWebRequest www = UnityWebRequest.Get(url);
		string text = Path.Combine(ApplicationPaths.streamingAssetsPath, fallbackFileName);
		string cachedFilePath = Path.Combine(ApplicationPaths.persistentDataPath, fallbackFileName);
		if (!TryGetIntroductoryLocalFileText(cachedFilePath, out var text2) && !TryGetIntroductoryLocalFileText(text, out text2))
		{
			PFLog.UI.Error("Failed to load Introductory Text from default path: " + text);
		}
		callback?.Invoke(text2);
		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			PFLog.UI.Warning("Skipping download of introductory text from URL: {0}. Internet is not connected", url);
			yield break;
		}
		PFLog.UI.Log("Downloading introductory text from URL: " + url);
		yield return www.SendWebRequest();
		if (www.result == UnityWebRequest.Result.Success)
		{
			string text3 = www.downloadHandler.text;
			try
			{
				File.WriteAllText(cachedFilePath, text3);
			}
			catch (Exception ex)
			{
				PFLog.UI.Error("JSON not saved in: " + cachedFilePath + ": " + ex.Message);
			}
			try
			{
				string obj = JsonConvert.DeserializeObject<LocalizedStringData>(text3)?.GetText(LocalizationManager.Instance.CurrentLocale);
				callback?.Invoke(obj);
				yield break;
			}
			catch (Exception ex2)
			{
				PFLog.UI.Error("Error deserializing introductory text: " + ex2.Message);
				callback?.Invoke(string.Empty);
				yield break;
			}
		}
		PFLog.UI.Error("Error downloading introductory text: " + www.error);
	}

	public void HandleLanguageChanged()
	{
		m_LanguageChanged.Execute();
	}

	public void ShowLicense()
	{
		m_MainMenu.ShowLicense();
	}

	public void ShowFeedback()
	{
		m_MainMenu.ShowFeedback();
	}
}
