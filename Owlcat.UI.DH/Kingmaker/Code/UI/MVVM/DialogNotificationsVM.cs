using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class DialogNotificationsVM : ViewModel, IDialogCueHandler, ISubscriber, IBookPageHandler
{
	public struct NotificationsData
	{
		public List<DialogNotificationVM> DetectiveNotifications;

		public List<DialogNotificationVM> CommonNotifications;

		public DialogNotificationSoundType SoundType;

		public LocalizedString HeaderText;

		public bool IsNewItem;
	}

	private readonly List<NotificationListenerBase> m_Listeners = new List<NotificationListenerBase>();

	public ReactiveCommand<Unit> PushNotificationsCommand { get; } = new ReactiveCommand<Unit>();


	public DialogNotificationsVM(DialogUIType dialogUIType)
	{
		RegisterListener(new CasesOpenedListener(dialogUIType).AddTo(this));
		RegisterListener(new CasesClosedListener(dialogUIType).AddTo(this));
		RegisterListener(new CluesReceivedListener(dialogUIType).AddTo(this));
		RegisterListener(new AddendumsReceivedListener(dialogUIType).AddTo(this));
		RegisterListener(new ConclusionsConstructedListener(dialogUIType).AddTo(this));
		RegisterListener(new ItemReceivedListener(dialogUIType).AddTo(this));
		RegisterListener(new ItemLostListener(dialogUIType).AddTo(this));
		RegisterListener(new CustomNotificationListener(dialogUIType).AddTo(this));
		RegisterListener(new DamageDealtListener(dialogUIType).AddTo(this));
		RegisterListener(new AlignmentShiftListener(dialogUIType).AddTo(this));
		RegisterListener(new FactionReputationLostListener(dialogUIType).AddTo(this));
		RegisterListener(new FactionReputationReceivedListener(dialogUIType).AddTo(this));
		RegisterListener(new AbilityAddedListener(dialogUIType).AddTo(this));
		RegisterListener(new BuffAddedListener(dialogUIType).AddTo(this));
		m_Listeners = m_Listeners.OrderBy((NotificationListenerBase l) => l.Order).ToList();
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		m_Listeners.ForEach(delegate(NotificationListenerBase l)
		{
			l.Dispose();
		});
		m_Listeners.Clear();
	}

	private void RegisterListener(NotificationListenerBase listener)
	{
		m_Listeners.Add(listener);
	}

	private void OnUpdate()
	{
		PushNotificationsCommand.Execute(Unit.Default);
	}

	public NotificationsData RequestNotifications()
	{
		List<DialogNotificationVM> list = new List<DialogNotificationVM>();
		List<DialogNotificationVM> list2 = new List<DialogNotificationVM>();
		IEnumerable<NotificationListenerBase> enumerable = m_Listeners.Where((NotificationListenerBase l) => l.HasData);
		foreach (NotificationListenerBase item in enumerable)
		{
			List<DialogNotificationVM> list3 = item.CreateNotifications();
			if (list3.Any())
			{
				NotificationCategory category = item.Category;
				if (category != 0 && category == NotificationCategory.Detective)
				{
					list.AddRange(list3);
				}
				else
				{
					list2.AddRange(list3);
				}
			}
		}
		DialogNotificationSoundType soundType = enumerable.FirstOrDefault()?.SoundType ?? DialogNotificationSoundType.Custom;
		LocalizedString headerText = enumerable.FirstOrDefault((NotificationListenerBase l) => l.HeaderText != null)?.HeaderText;
		bool isNewItem = enumerable.Any((NotificationListenerBase l) => l.HasNewItems);
		m_Listeners.ForEach(delegate(NotificationListenerBase l)
		{
			l.Clear();
		});
		NotificationsData result = default(NotificationsData);
		result.DetectiveNotifications = list;
		result.CommonNotifications = list2;
		result.SoundType = soundType;
		result.HeaderText = headerText;
		result.IsNewItem = isNewItem;
		return result;
	}

	public void HandleOnCueShow(CueShowData cueShowData)
	{
		OnUpdate();
	}

	public void HandleOnBookPageShow(BlueprintBookPage page, List<CueShowData> cues, List<BlueprintAnswer> answers)
	{
		OnUpdate();
	}
}
