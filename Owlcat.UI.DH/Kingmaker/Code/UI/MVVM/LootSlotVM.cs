using Kingmaker.Blueprints.Loot;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LootSlotVM : ViewModel
{
	public readonly Sprite Icon;

	public readonly int Count;

	private readonly LootEntry m_LootEntity;

	public LootSlotVM(LootEntry lootEntry)
	{
		m_LootEntity = lootEntry;
		Icon = lootEntry.Item.Icon.GetDefaultIfNull(DefaultImageType.Item);
		Count = lootEntry.Count;
	}

	public TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateLootEntity(m_LootEntity);
	}
}
