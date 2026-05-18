using System;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Enums;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class DialogConfig
{
	[field: SerializeField]
	public DialogCueColors DefaultDialogCueColors { get; private set; }

	[field: SerializeField]
	public DialogNotificationColors DialogNotificationColors { get; private set; }

	[field: SerializeField]
	public DialogNotificationColors BookEventNotificationColors { get; private set; }

	public DialogNotificationColors GetNotificationColors(DialogUIType uiType)
	{
		return uiType switch
		{
			DialogUIType.Dialog => DialogNotificationColors, 
			DialogUIType.BookEvent => BookEventNotificationColors, 
			DialogUIType.DetectiveEpilogue => BookEventNotificationColors, 
			_ => throw new ArgumentOutOfRangeException("uiType", uiType, null), 
		};
	}
}
