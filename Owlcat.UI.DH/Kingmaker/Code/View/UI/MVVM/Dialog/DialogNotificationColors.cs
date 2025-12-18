using System;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.Dialog;

[Serializable]
public class DialogNotificationColors
{
	[Header("NotificationColors")]
	public Color32 NotificationPositive;

	public Color32 NotificationNegative;

	public Color32 NotificationNeutral;

	public Color32 LinkColor;
}
