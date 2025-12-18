using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using TMPro;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UtilityMessageBox
{
	public static void ShowMessageBox(string messageText, DialogMessageBoxType boxType, Action<DialogMessageBoxButton> onClose, Action<TMP_LinkInfo> onLinkInvoke = null, string yesLabel = null, string noLabel = null, int waitTime = 0)
	{
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
		{
			w.HandleOpen(messageText, boxType, onClose, onLinkInvoke, yesLabel, noLabel, null, null, null, waitTime);
		});
	}

	public static void SendWarning(string message)
	{
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(message);
		});
		PFLog.UI.Log("Send UI Warning: " + message);
	}
}
