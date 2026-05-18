using System;
using System.Collections.Generic;
using System.Text;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.UI.Common;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public static class NotificationFormatter
{
	public static string GenerateLink(string text, string link, DialogUIType dialogUIType = DialogUIType.Dialog)
	{
		DialogNotificationColors notificationColors = UIConfig.Instance.DialogConfig.GetNotificationColors(dialogUIType);
		return GenerateLink(text, link, notificationColors.LinkColor);
	}

	public static string GenerateLink(string text, string link, Color? color)
	{
		string text2 = "<b><link=\"" + link + "\">" + text + "</link></b>";
		if (color.HasValue)
		{
			text2 = "<color=#" + color.Value.HTML() + ">" + text2 + "</color>";
		}
		return text2;
	}

	public static string FormatText(string text, NotificationType type = NotificationType.Neutral, DialogUIType dialogUIType = DialogUIType.Dialog)
	{
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}
		DialogNotificationColors notificationColors = UIConfig.Instance.DialogConfig.GetNotificationColors(dialogUIType);
		string text2 = type switch
		{
			NotificationType.Positive => notificationColors.NotificationPositive.HTML(), 
			NotificationType.Negative => notificationColors.NotificationNegative.HTML(), 
			NotificationType.Neutral => notificationColors.NotificationNeutral.HTML(), 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
		return "<color=#" + text2 + ">" + text + "</color>";
	}

	public static void SmartAppend(KeyValuePair<string, int> pair, StringBuilder stringBuilder)
	{
		KeyValuePair<string, int> keyValuePair = pair;
		var (text2, num2) = (KeyValuePair<string, int>)(ref keyValuePair);
		stringBuilder.Append((num2 > 1) ? $"{text2} x{num2}" : text2);
	}

	public static void AppendClue(BlueprintClue clue, StringBuilder stringBuilder, DialogUIType dialogUIType = DialogUIType.Dialog, string additionalInfo = null)
	{
		DialogNotificationColors notificationColors = UIConfig.Instance.DialogConfig.GetNotificationColors(dialogUIType);
		string value = GenerateLink("<b><color=#" + notificationColors.LinkColor.HTML() + ">" + clue.GetUIData().Name.Text + "</color></b>", $"{EntityLink.Type.Detective}:{clue.AssetGuid}:{additionalInfo}");
		stringBuilder.Append(value);
	}
}
