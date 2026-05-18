using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Items;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateItem : TooltipBaseTemplate
{
	private readonly ItemEntity m_Item;

	private readonly List<ItemEntity> m_FewItems;

	private readonly ItemEntity m_BaseItem;

	private readonly BlueprintItem m_BlueprintItem;

	private readonly List<BlueprintItem> m_FewBlueprintsItems = new List<BlueprintItem>();

	private readonly BlueprintItem m_BaseBlueprintItem;

	private readonly bool m_ForceUpdateCache;

	private readonly bool m_Replenishing;

	private readonly ItemTooltipData m_PrecomputedTooltipData;

	private ItemTooltipData m_ItemTooltipData;

	private BaseItemPart m_ItemPart;

	private bool m_Comparative;

	private readonly bool m_IsScreenTooltip;

	private readonly bool m_IsFewItems;

	private readonly List<ItemTooltipData> m_FewItemsTooltipDatas = new List<ItemTooltipData>();

	private readonly List<BaseItemPart> m_FewItemsParts = new List<BaseItemPart>();

	public ItemEntity Item => m_Item;

	public override TooltipBackground Background => GetBackgroundColor();

	public TooltipTemplateItem(ItemEntity item, ItemEntity baseItem = null, bool forceUpdateCache = false, bool replenishing = false, List<ItemEntity> fewItems = null, bool isScreenTooltip = false)
	{
		if (item == null && (fewItems == null || !fewItems.Any()))
		{
			return;
		}
		if (item != null)
		{
			m_Item = item;
			m_BlueprintItem = item.Blueprint;
			m_IsFewItems = false;
		}
		else if (fewItems.Any())
		{
			m_FewItems = fewItems;
			m_FewBlueprintsItems.AddRange(fewItems.Select((ItemEntity i) => i.Blueprint));
			m_IsFewItems = true;
		}
		m_BaseItem = baseItem;
		m_ForceUpdateCache = forceUpdateCache;
		m_Replenishing = replenishing;
		m_IsScreenTooltip = isScreenTooltip;
		ContentSpacing = 5f;
	}

	public TooltipTemplateItem(BlueprintItem blueprintItem, BlueprintItem baseBlueprintItem = null, bool forceUpdateCache = false)
	{
		if (blueprintItem != null)
		{
			m_BlueprintItem = blueprintItem;
			m_BaseBlueprintItem = baseBlueprintItem;
			m_ForceUpdateCache = forceUpdateCache;
			ContentSpacing = 5f;
		}
	}

	public TooltipTemplateItem(BlueprintItem blueprintItem, ItemTooltipData precomputedTooltipData)
	{
		if (blueprintItem != null)
		{
			m_BlueprintItem = blueprintItem;
			m_PrecomputedTooltipData = precomputedTooltipData;
			ContentSpacing = 5f;
		}
	}

	public override void Prepare(TooltipTemplateType type)
	{
		if (m_BlueprintItem == null && (m_FewBlueprintsItems == null || !m_FewBlueprintsItems.Any()))
		{
			return;
		}
		m_Comparative = m_BaseItem != null || m_BaseBlueprintItem != null;
		bool forceUpdateCache = m_ForceUpdateCache;
		ItemTooltipData itemTooltipData = ((m_BaseItem != null) ? TooltipsDataCache.Instance.GetItemTooltipData(m_BaseItem, forceUpdateCache, m_Replenishing) : ((m_BaseBlueprintItem != null) ? TooltipsDataCache.Instance.GetItemTooltipData(m_BaseBlueprintItem, forceUpdateCache) : null));
		if (!m_IsFewItems)
		{
			if (m_PrecomputedTooltipData != null)
			{
				m_ItemTooltipData = m_PrecomputedTooltipData;
				m_ItemPart = (BaseItemPart)Activator.CreateInstance(GetItemPartTemplateType(), m_BlueprintItem, m_ItemTooltipData, itemTooltipData, m_IsScreenTooltip);
			}
			else
			{
				m_ItemTooltipData = ((m_Item != null) ? TooltipsDataCache.Instance.GetItemTooltipData(m_Item, forceUpdateCache, m_Replenishing) : TooltipsDataCache.Instance.GetItemTooltipData(m_BlueprintItem, forceUpdateCache));
				m_ItemPart = ((m_Item != null) ? ((BaseItemPart)Activator.CreateInstance(GetItemPartTemplateType(), m_Item, m_ItemTooltipData, itemTooltipData, m_IsScreenTooltip)) : ((BaseItemPart)Activator.CreateInstance(GetItemPartTemplateType(), m_BlueprintItem, m_ItemTooltipData, itemTooltipData, m_IsScreenTooltip)));
			}
		}
		else
		{
			for (int i = 0; i < m_FewItems.Count; i++)
			{
				m_FewItemsTooltipDatas.Add((m_FewItems[i] != null) ? TooltipsDataCache.Instance.GetItemTooltipData(m_FewItems[i], forceUpdateCache, m_Replenishing) : TooltipsDataCache.Instance.GetItemTooltipData(m_FewBlueprintsItems[i], forceUpdateCache));
				m_FewItemsParts.Add((m_FewItems[i] != null) ? ((BaseItemPart)Activator.CreateInstance(GetItemPartTemplateType(m_FewBlueprintsItems[i]), m_FewItems[i], m_FewItemsTooltipDatas[i], itemTooltipData, m_IsScreenTooltip)) : ((BaseItemPart)Activator.CreateInstance(GetItemPartTemplateType(m_FewBlueprintsItems[i]), m_FewBlueprintsItems[i], m_FewItemsTooltipDatas[i], itemTooltipData, m_IsScreenTooltip)));
			}
		}
		if (m_Item != null && !m_Item.IsDisposed && type == TooltipTemplateType.Info)
		{
			m_Item.OnOpenDescription();
		}
	}

	private Type GetItemPartTemplateType()
	{
		BlueprintItem blueprintItem = m_BlueprintItem;
		if (!(blueprintItem is BlueprintItemWeapon))
		{
			if (!(blueprintItem is BlueprintItemEquipmentUsable))
			{
				if (blueprintItem is BlueprintItemArmor)
				{
					return typeof(ArmorItemPart);
				}
				return typeof(BaseItemPart);
			}
			return typeof(UsableItemPart);
		}
		return typeof(WeaponItemPart);
	}

	private Type GetItemPartTemplateType(BlueprintItem blueprintItem)
	{
		if (!(blueprintItem is BlueprintItemWeapon))
		{
			if (!(blueprintItem is BlueprintItemEquipmentUsable))
			{
				if (blueprintItem is BlueprintItemArmor)
				{
					return typeof(ArmorItemPart);
				}
				return typeof(BaseItemPart);
			}
			return typeof(UsableItemPart);
		}
		return typeof(WeaponItemPart);
	}

	private TooltipBackground GetBackgroundColor()
	{
		if (m_Comparative)
		{
			return TooltipBackground.Gray;
		}
		if (UtilityParty.GetCurrentSelectedUnit() == null)
		{
			return TooltipBackground.White;
		}
		if (!m_IsFewItems)
		{
			bool[] equipPosibility = UIUtilityItem.GetEquipPosibility(m_Item);
			if (!equipPosibility[0] && !equipPosibility[1])
			{
				return TooltipBackground.Red;
			}
			if (equipPosibility[1])
			{
				return TooltipBackground.Yellow;
			}
		}
		else
		{
			List<bool[]> source = m_FewItems.Select(UIUtilityItem.GetEquipPosibility).ToList();
			if (source.Any((bool[] ep) => !ep[0] && !ep[1]))
			{
				return TooltipBackground.Red;
			}
			if (source.Any((bool[] ep) => ep[1]))
			{
				return TooltipBackground.Yellow;
			}
		}
		return TooltipBackground.White;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		if ((m_ItemTooltipData == null && (m_FewItemsTooltipDatas == null || !m_FewItemsTooltipDatas.Any())) || m_IsFewItems)
		{
			yield break;
		}
		foreach (ITooltipBrick item in m_ItemPart.GetHeader(type))
		{
			yield return item;
		}
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		if (m_ItemTooltipData == null && (m_FewItemsTooltipDatas == null || !m_FewItemsTooltipDatas.Any()))
		{
			yield break;
		}
		if (!m_IsFewItems)
		{
			foreach (ITooltipBrick item in m_ItemPart.GetBody(type))
			{
				yield return item;
			}
			yield break;
		}
		for (int i = 0; i < m_FewItems.Count; i++)
		{
			(string, ItemHeaderType) itemHeaderText = UIUtilityItem.GetItemHeaderText(m_FewItems[i]);
			if (!string.IsNullOrEmpty(itemHeaderText.Item1))
			{
				yield return new BrickItemHeaderVM(itemHeaderText.Item1, itemHeaderText.Item2);
			}
			foreach (ITooltipBrick item2 in m_FewItemsParts[i].GetHeader(type))
			{
				yield return item2;
			}
			foreach (ITooltipBrick item3 in m_FewItemsParts[i].GetBody(type))
			{
				yield return item3;
			}
			yield return new BrickSeparatorVM(TooltipBrickElementType.Small);
			foreach (ITooltipBrick item4 in m_FewItemsParts[i].GetFooter(type))
			{
				yield return item4;
			}
			if (i != m_FewItems.Count - 1)
			{
				yield return new BrickSeparatorVM(TooltipBrickElementType.Medium);
			}
		}
	}

	public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		if ((m_ItemTooltipData == null && (m_FewItemsTooltipDatas == null || !m_FewItemsTooltipDatas.Any())) || m_IsFewItems)
		{
			yield break;
		}
		foreach (ITooltipBrick item in m_ItemPart.GetFooter(type))
		{
			yield return item;
		}
	}
}
