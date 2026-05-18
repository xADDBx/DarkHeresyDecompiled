using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.GameInfo;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.InputSystems.Enums;
using Kingmaker.Utility;
using Kingmaker.Utility.Reporting.Base;
using Owlcat.Runtime.Core.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BugReportCanvas : MonoBehaviour, IBugReportUIHandler, ISubscriber, IAreaHandler
{
	public TextMeshProUGUI StatusTextMeshPro;

	private int m_SetStatusTmpAttempts;

	private ReportingRaycaster m_ReportingRaycaster;

	private bool m_State;

	private bool m_IsShowReportButton;

	private bool m_IsCrashDumpReport;

	private bool m_IsSendingReportInProgress;

	private float m_SendingReportAfterTimer;

	[SerializeField]
	private TextMeshProUGUI WatermarkText;

	[SerializeField]
	private CanvasGroup m_BugreportCanvasGroup;

	private const string BugReportPlayerPref = "BugReportMessageWasShown";

	public static bool IsBugReportVisible { get; private set; }

	private bool IsShowReportButton
	{
		get
		{
			return m_IsShowReportButton;
		}
		set
		{
			if (m_IsShowReportButton != value)
			{
				m_IsShowReportButton = value;
				m_BugreportCanvasGroup.alpha = (value ? 1f : 0f);
				m_BugreportCanvasGroup.blocksRaycasts = value;
				if (!value && m_State)
				{
					Show(state: false);
				}
			}
		}
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		EventBus.Subscribe(this);
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		UnbindKeyboard(Game.Instance.Keyboard);
		EventBus.Unsubscribe(this);
	}

	public void BindKeyboard(KeyboardAccess keyboardAccess)
	{
		if (keyboardAccess.CanBeRegistered("RapidBugReportWindowOpen", KeyCode.B, GameModesGroup.All, ctrl: false, alt: true, shift: false))
		{
			GameModeType[] gameModesArray = KeyboardAccess.GetGameModesArray(GameModesGroup.All);
			keyboardAccess.RegisterBinding("RapidBugReportWindowOpen", KeyCode.B, gameModesArray, ctrl: false, alt: true, shift: false);
			keyboardAccess.RegisterBinding("ClipboardCopyBuildInfo", KeyCode.C, gameModesArray, ctrl: true, alt: true, shift: true);
		}
		keyboardAccess.Bind("RapidBugReportWindowOpen", OnHotKeyBugReportOpen);
		keyboardAccess.Bind("ClipboardCopyBuildInfo", ClipboardCopyBuildInfoHotkey);
	}

	public void UnbindKeyboard(KeyboardAccess keyboardAccess)
	{
		keyboardAccess.Unbind("RapidBugReportWindowOpen", OnHotKeyBugReportOpen);
		keyboardAccess.Unbind("ClipboardCopyBuildInfo", ClipboardCopyBuildInfoHotkey);
	}

	public void OnHotKeyBugReportOpen()
	{
		m_IsCrashDumpReport = false;
		IsShowReportButton = true;
		HandleBugReportOpen();
	}

	public void HandleCrushDumpReport()
	{
		m_IsCrashDumpReport = true;
		IsShowReportButton = true;
		Show(state: true);
	}

	public void HandleBugReportOpen(bool showBugReportOnly = false)
	{
		if (showBugReportOnly)
		{
			Show(state: true);
			return;
		}
		Game.Instance.RequestPauseUi(isPaused: true);
		if (PlayerPrefs.GetInt("BugReportMessageWasShown") == 1)
		{
			Show(state: true);
			return;
		}
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler h)
		{
			h.HandleOpen(Game.Instance.IsControllerMouse ? UIStrings.Instance.UIBugReport.BugReportStartingMessagePC.Text : UIStrings.Instance.UIBugReport.BugReportStartingMessageConsole.Text, DialogMessageBoxType.Message, delegate(DialogMessageBoxButton value)
			{
				PlayerPrefs.SetInt("BugReportMessageWasShown", 1);
				PlayerPrefs.Save();
				Game.Instance.RequestPauseUi(isPaused: false);
				Show(value == DialogMessageBoxButton.Yes, waitBugReportTutorialHide: true);
			}, null, UIStrings.Instance.UIBugReport.BugReportContinue.Text);
		});
	}

	public void HandleBugReportCanvasHotKeyOpen()
	{
		OnHotKeyBugReportOpen();
	}

	public void HandleBugReportShow()
	{
	}

	public void HandleBugReportHide()
	{
		Show(state: false);
	}

	public void HandleUIElementFeature(string featureName)
	{
	}

	private void Show(bool state, bool waitBugReportTutorialHide = false)
	{
		if (m_State != state)
		{
			m_State = state;
			IsBugReportVisible = state;
			if (state)
			{
				StartCoroutine(ShowReportWindow(waitBugReportTutorialHide));
			}
			else
			{
				HideReportWindow();
			}
		}
	}

	private IEnumerator ShowReportWindow(bool waitBugReportTutorialHide)
	{
		if (m_ReportingRaycaster == null)
		{
			m_ReportingRaycaster = base.gameObject.AddComponent<ReportingRaycaster>();
		}
		try
		{
			EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
			{
				h.HandleUIElementFeature(m_ReportingRaycaster.GetFeatureName());
			});
		}
		catch
		{
		}
		if (waitBugReportTutorialHide)
		{
			yield return null;
		}
		if (m_IsCrashDumpReport)
		{
			m_IsCrashDumpReport = false;
			yield return ReportingUtils.Instance.MakeNewReport(withScreenshot: false, withSave: false, withCrashDump: true);
		}
		else
		{
			yield return ReportingUtils.Instance.MakeNewReport(withScreenshot: true, withSave: true, withCrashDump: false);
		}
		yield return null;
		EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
		{
			h.HandleBugReportShow();
		});
		yield return null;
	}

	private void HideReportWindow()
	{
		EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
		{
			h.HandleBugReportHide();
		});
	}

	public void OnAreaBeginUnloading()
	{
		IsShowReportButton = false;
	}

	public void OnAreaDidLoad()
	{
		HideReportWindow();
		IsShowReportButton = true;
		try
		{
			_ = string.Empty;
			if (Application.isPlaying)
			{
				Game.Instance?.CurrentlyLoadedArea.NameSafe();
			}
		}
		catch
		{
		}
	}

	private void Start()
	{
		if (m_ReportingRaycaster == null)
		{
			m_ReportingRaycaster = base.gameObject.AddComponent<ReportingRaycaster>();
		}
		Show(state: false);
		m_BugreportCanvasGroup.alpha = 0f;
		m_BugreportCanvasGroup.blocksRaycasts = false;
		WatermarkText.text = GameVersion.GetDisplayedVersion();
		WatermarkText.gameObject.SetActive(!string.IsNullOrEmpty(WatermarkText.text));
		try
		{
			KeyboardAccess keyboard = Game.Instance.Keyboard;
			BindKeyboard(keyboard);
		}
		catch
		{
		}
	}

	private void Update()
	{
		IsShowReportButton = true;
		if (StatusTextMeshPro == null && m_SetStatusTmpAttempts < 10)
		{
			m_SetStatusTmpAttempts++;
			try
			{
				GameObject gameObject = Object.Instantiate(base.gameObject.transform.Children().FirstOrDefault((Transform x) => x.gameObject.name == "GameVersion")?.gameObject, base.gameObject.transform);
				gameObject.name = "ReportSendStatusText";
				RectTransform component = gameObject.GetComponent<RectTransform>();
				component.anchorMin = new Vector2(1f, 1f);
				component.anchorMax = new Vector2(1f, 1f);
				component.pivot = new Vector2(0f, 0f);
				component.anchoredPosition = new Vector3(-365f, -80f, 0f);
				component.sizeDelta = new Vector2(165f, 30f);
				StatusTextMeshPro = gameObject.gameObject.transform.Children().FirstOrDefault((Transform x) => x.GetComponent<TextMeshProUGUI>())?.GetComponent<TextMeshProUGUI>();
				StatusTextMeshPro.text = string.Empty;
			}
			catch
			{
				GameObject gameObject2 = GameObject.Find("ReportSendStatusText");
				if (gameObject2 != null)
				{
					Object.Destroy(gameObject2);
				}
			}
		}
		if (!m_IsSendingReportInProgress && StatusTextMeshPro != null)
		{
			if (m_SendingReportAfterTimer > 0f)
			{
				m_SendingReportAfterTimer -= Time.deltaTime;
				return;
			}
			StatusTextMeshPro.text = string.Empty;
			m_SendingReportAfterTimer = 10f;
		}
	}

	private static void ClipboardCopyBuildInfoHotkey()
	{
		GUIUtility.systemCopyBuffer = ReportVersionManager.GetBuildInfo();
	}
}
