using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Sound;

namespace Kingmaker.Code.UI.MVVM;

public class CustomNotificationListener : NotificationListenerBase, IDialogNotificationHandler, ISubscriber
{
	private readonly List<string> m_CustomNotifications = new List<string>();

	public override bool HasData => m_CustomNotifications.Any();

	public override int Order => 7;

	public override NotificationCategory Category => NotificationCategory.Common;

	public override DialogNotificationSoundType SoundType => DialogNotificationSoundType.Custom;

	public CustomNotificationListener(DialogUIType dialogUIType)
		: base(dialogUIType)
	{
	}

	public void AddCustomNotification(string text)
	{
		m_CustomNotifications.Add(text);
	}

	public override List<DialogNotificationVM> CreateNotifications()
	{
		if (m_CustomNotifications.Count == 0)
		{
			return new List<DialogNotificationVM>();
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string customNotification in m_CustomNotifications)
		{
			stringBuilder.Append(customNotification);
			stringBuilder.Append("\n");
		}
		return new List<DialogNotificationVM>
		{
			new DialogNotificationVM(NotificationFormatter.FormatText(stringBuilder.ToString()))
		};
	}

	public override void Clear()
	{
		m_CustomNotifications.Clear();
	}
}
