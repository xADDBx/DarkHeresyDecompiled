using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UI.Sound;

namespace Kingmaker.Code.UI.MVVM;

public class ItemLostListener : NotificationListenerBase, IItemsCollectionHandler, ISubscriber
{
	private readonly Dictionary<ItemEntity, int> m_ItemsLost = new Dictionary<ItemEntity, int>();

	public override bool HasData => m_ItemsLost.Any((KeyValuePair<ItemEntity, int> k) => k.Value > 0);

	public override int Order => 6;

	public override NotificationCategory Category => NotificationCategory.Common;

	public override DialogNotificationSoundType SoundType => DialogNotificationSoundType.ItemLost;

	public ItemLostListener(DialogUIType dialogUIType)
		: base(dialogUIType)
	{
	}

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
	}

	public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
	{
		if (SettingsRoot.Game.Dialogs.ShowItemsReceivedNotification.GetValue() && collection.IsPlayerInventory && item.IsLootable && item.Blueprint != null && !string.IsNullOrWhiteSpace(item.Name) && count != 0)
		{
			if (m_ItemsLost.TryGetValue(item, out var _))
			{
				m_ItemsLost[item] += count;
			}
			else
			{
				m_ItemsLost.Add(item, count);
			}
		}
	}

	public override List<DialogNotificationVM> CreateNotifications()
	{
		List<KeyValuePair<ItemEntity, int>> list = m_ItemsLost.Where((KeyValuePair<ItemEntity, int> k) => k.Value > 0).ToList();
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
			NotificationFormatter.SmartAppend(new KeyValuePair<string, int>(NotificationFormatter.GenerateLink(list[i].Key.Name, "ib:" + list[i].Key.Blueprint.AssetGuid), list[i].Value), stringBuilder);
		}
		string text = string.Format(UINotificationTexts.Instance.ItemsLostFormat, stringBuilder);
		return new List<DialogNotificationVM>
		{
			new DialogNotificationVM(NotificationFormatter.FormatText(text, NotificationType.Negative))
		};
	}

	public override void Clear()
	{
		m_ItemsLost.Clear();
	}
}
