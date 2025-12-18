using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility;
using UnityEngine.SceneManagement;

namespace Kingmaker.Code.View.UI.MVVM.BugReport;

public class MainMenuCrushDump
{
	private static bool MainMenuIsLoaded => SceneManager.GetSceneByName(GameScenes.MainMenu).isLoaded;

	public static void CheckCrushReportingUtils()
	{
		if (!MainMenuIsLoaded || !ReportingUtils.Instance.CheckCrashDumpFound())
		{
			return;
		}
		if (ReportingUtils.Instance.IsReportWithMods(isCrash: true))
		{
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
			{
				w.HandleOpen(UIStrings.Instance.MainMenu.CrashDumpFoundDialogueMods);
			});
			return;
		}
		if (ReportingUtils.Instance.IsOutOfMemoryCrash())
		{
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
			{
				w.HandleOpen(UIStrings.Instance.MainMenu.CrashDumpFoundDialogueRam);
			});
			return;
		}
		if (ReportingUtils.Instance.IsCorruptedBundleCrash(isCrash: true))
		{
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
			{
				w.HandleOpen(UIStrings.Instance.MainMenu.CrashDumpFoundDialogueCorrupted);
			});
			return;
		}
		if (ReportingUtils.Instance.IsDiskFreeSpaceCrash(isCrash: true))
		{
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
			{
				w.HandleOpen(UIStrings.Instance.MainMenu.CrashDumpFoundDialogueFreeSpace);
			});
			return;
		}
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
		{
			w.HandleOpen(UIStrings.Instance.MainMenu.CrashDumpFoundDialogue, DialogMessageBoxType.Dialog, delegate
			{
				EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
				{
					h.HandleCrushDumpReport();
				});
			});
		});
	}

	public static void CheckCrushDumpMessage()
	{
		if (!MainMenuIsLoaded || CrushDumpMessage.Exception == null)
		{
			return;
		}
		if (ReportingUtils.Instance.IsReportWithMods(isCrash: false))
		{
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
			{
				w.HandleOpen(UIStrings.Instance.MainMenu.CrashDumpFoundDialogueMods);
			});
		}
		else if (ReportingUtils.Instance.IsCorruptedBundleCrash(isCrash: false))
		{
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
			{
				w.HandleOpen(UIStrings.Instance.MainMenu.ExceptionDialogueCorrupted);
			});
		}
		else if (ReportingUtils.Instance.IsDiskFreeSpaceCrash(isCrash: false))
		{
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
			{
				w.HandleOpen(UIStrings.Instance.MainMenu.ExceptionDialogueFreeSpace);
			});
		}
		else
		{
			Exception exception = CrushDumpMessage.Exception;
			string dialogMessage = exception.ToString();
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
			{
				w.HandleOpen(dialogMessage, DialogMessageBoxType.Message, delegate(DialogMessageBoxButton b)
				{
					if (b == DialogMessageBoxButton.Yes)
					{
						EventBus.RaiseEvent(delegate(IBugReportDescriptionUIHandler h)
						{
							h.HandleException(exception);
						});
						EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
						{
							h.HandleCrushDumpReport();
						});
					}
				}, null, UIStrings.Instance.CommonTexts.ReportButton);
			});
		}
		CrushDumpMessage.Exception = null;
	}
}
