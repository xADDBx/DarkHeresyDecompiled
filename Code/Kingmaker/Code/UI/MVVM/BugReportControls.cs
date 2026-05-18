using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public static class BugReportControls
{
	private static bool s_LeftStickBtnPressed;

	private static bool s_RightStickBtnPressed;

	public static CompositeDisposable AddBugReportControls()
	{
		return new CompositeDisposable();
	}

	private static void TryToOpenBugReport()
	{
		if (s_LeftStickBtnPressed && s_RightStickBtnPressed)
		{
			s_LeftStickBtnPressed = false;
			s_RightStickBtnPressed = false;
			EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
			{
				h.HandleBugReportCanvasHotKeyOpen();
			});
		}
	}
}
