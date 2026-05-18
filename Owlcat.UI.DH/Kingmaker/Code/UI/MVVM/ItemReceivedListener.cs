using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Designers;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UI.Sound;

namespace Kingmaker.Code.UI.MVVM;

public class ItemReceivedListener : NotificationListenerBase, IItemsCollectionHandler, ISubscriber
{
	private readonly Dictionary<ItemEntity, int> m_ItemsReceived = new Dictionary<ItemEntity, int>();

	public override bool HasData => m_ItemsReceived.Any((KeyValuePair<ItemEntity, int> k) => k.Value > 0);

	public override int Order => 5;

	public override NotificationCategory Category => NotificationCategory.Common;

	public override DialogNotificationSoundType SoundType => DialogNotificationSoundType.ItemReceived;

	public override bool HasNewItems => true;

	public ItemReceivedListener(DialogUIType dialogUIType)
		: base(dialogUIType)
	{
	}

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if (SettingsRoot.Game.Dialogs.ShowItemsReceivedNotification.GetValue() && collection == GameHelper.GetPlayerCharacter().Inventory.Collection && item?.Blueprint != null && !string.IsNullOrWhiteSpace(item.Name) && count != 0)
		{
			if (m_ItemsReceived.TryGetValue(item, out var _))
			{
				m_ItemsReceived[item] += count;
			}
			else
			{
				m_ItemsReceived.Add(item, count);
			}
		}
	}

	public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
	{
	}

	public override List<DialogNotificationVM> CreateNotifications()
	{
		List<KeyValuePair<ItemEntity, int>> list = m_ItemsReceived.Where((KeyValuePair<ItemEntity, int> k) => k.Value > 0).ToList();
		if (!list.Any())
		{
			return new List<DialogNotificationVM>();
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			NotificationFormatter.SmartAppend(new KeyValuePair<string, int>(NotificationFormatter.GenerateLink(list[i].Key.Name, "i:" + list[i].Key.UniqueId), list[i].Value), stringBuilder);
		}
		string text = string.Format(UINotificationTexts.Instance.ItemsRecievedFormat, stringBuilder);
		return new List<DialogNotificationVM>
		{
			new DialogNotificationVM(NotificationFormatter.FormatText(text, NotificationType.Positive, m_DialogUIType))
		};
	}

	public override void Clear()
	{
		m_ItemsReceived.Clear();
	}
}
