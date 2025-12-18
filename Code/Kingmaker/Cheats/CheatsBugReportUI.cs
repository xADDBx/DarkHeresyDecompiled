using System;
using System.Linq;
using Core.Cheats;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;

namespace Kingmaker.Cheats;

internal class CheatsBugReportUI
{
	[Cheat(Name = "BugReportOpenSingleMode", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void BugReportOpenSingleMode(bool showBugReportOnly = true)
	{
		EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
		{
			h.HandleBugReportOpen(showBugReportOnly);
		});
	}

	[Cheat(Name = "BugReportHotkeyOpen", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void BugReportHotkeyOpen()
	{
		EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
		{
			h.HandleBugReportCanvasHotKeyOpen();
		});
	}

	[Cheat(Name = "BugReportShow", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void BugReportShow()
	{
		EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
		{
			h.HandleBugReportShow();
		});
	}

	[Cheat(Name = "BugReportHide", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void BugReportHide()
	{
		EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
		{
			h.HandleBugReportHide();
		});
	}

	[Cheat(Name = "BugReportFeature", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void BugReportFeature()
	{
		EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
		{
			h.HandleUIElementFeature("Test Feature Name");
		});
	}

	[Cheat(Name = "BugReportCrushDump", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void BugReportCrushDump()
	{
		EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
		{
			h.HandleCrushDumpReport();
		});
	}

	public static void HandleFullScreenUIItJustWorks(bool active, FullScreenUIType fullScreenUIType)
	{
		EventBus.RaiseEvent(delegate(IBugReportDescriptionUIHandler h)
		{
			h.HandleFullScreenUIItJustWorks(active, fullScreenUIType);
		});
	}

	[Cheat(Name = "BugReportException", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void BugReportException()
	{
		Exception lastException = QAModeExceptionReporter.GetLastException() ?? new Exception("Test Exception");
		EventBus.RaiseEvent(delegate(IBugReportDescriptionUIHandler h)
		{
			h.HandleException(lastException);
		});
	}

	[Cheat(Name = "BugReportErrorMessages", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void BugReportErrorMessages()
	{
		string[] messages = (from x in QAModeExceptionReporter.GetMessages()?.Where<(string, bool)>(((string message, bool addToReport) x) => x.addToReport)
			select x.message).ToArray();
		EventBus.RaiseEvent(delegate(IBugReportDescriptionUIHandler h)
		{
			h.HandleErrorMessages(messages);
		});
	}
}
