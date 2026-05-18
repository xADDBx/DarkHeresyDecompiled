using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class NotificationListenerBase : ViewModel
{
	protected readonly DialogUIType m_DialogUIType;

	public abstract bool HasData { get; }

	public abstract int Order { get; }

	public abstract NotificationCategory Category { get; }

	public abstract DialogNotificationSoundType SoundType { get; }

	public virtual LocalizedString HeaderText { get; }

	public virtual bool HasNewItems { get; }

	protected NotificationListenerBase(DialogUIType dialogUIType)
	{
		m_DialogUIType = dialogUIType;
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		Clear();
		base.OnDispose();
	}

	public abstract List<DialogNotificationVM> CreateNotifications();

	public abstract void Clear();
}
