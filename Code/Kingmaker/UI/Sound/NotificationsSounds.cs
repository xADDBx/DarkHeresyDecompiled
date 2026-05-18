using System;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine;

namespace Kingmaker.UI.Sound;

[Serializable]
public class NotificationsSounds
{
	[Serializable]
	public class UINotificationsDialog
	{
		[field: SerializeField]
		public EnumSoundList<DialogNotificationSoundType> SoundList { get; private set; }
	}

	[Serializable]
	public class UISoundNotifications
	{
		[field: SerializeField]
		public UISound NewQuest { get; private set; }

		[field: SerializeField]
		public UISound NewInformation { get; private set; }

		[field: SerializeField]
		public UISound CompletedQuest { get; private set; }

		[field: SerializeField]
		public UISound NewDetectiveCase { get; private set; }

		[field: SerializeField]
		public UISound NewDetectiveInformation { get; private set; }
	}

	[Serializable]
	public class UISystemNotifications
	{
		[field: SerializeField]
		public EnumSoundList<WarningNotificationType> SoundList { get; private set; }

		[field: SerializeField]
		public UISound DefaultSound { get; private set; }

		[field: SerializeField]
		public UISound WarningSound { get; private set; }

		[field: SerializeField]
		public UISound AttentionSound { get; private set; }

		public UISound GetSound(WarningNotificationType type)
		{
			return SoundList.GetSound(type);
		}

		public UISound GetSound(WarningNotificationFormat warningType)
		{
			return warningType switch
			{
				WarningNotificationFormat.Common => DefaultSound, 
				WarningNotificationFormat.Warning => WarningSound, 
				WarningNotificationFormat.Attention => AttentionSound, 
				_ => throw new ArgumentOutOfRangeException("warningType", warningType, null), 
			};
		}
	}

	public static NotificationsSounds Instance => Services.GetInstance<UISounds>().Sounds.NotificationsSounds;

	[field: SerializeField]
	public UINotificationsDialog DialogNotifications { get; private set; }

	[field: SerializeField]
	public UISoundNotifications Notifications { get; private set; }

	[field: SerializeField]
	public UISystemNotifications SystemNotifications { get; private set; }
}
