using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Items;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipsDataCache : ViewModel
{
	private static TooltipsDataCache s_Instance;

	private readonly Dictionary<ItemEntity, ItemTooltipData> m_ItemTooltipDataCache = new Dictionary<ItemEntity, ItemTooltipData>();

	private readonly Dictionary<BlueprintItem, ItemTooltipData> m_BlueprintItemTooltipDataCache = new Dictionary<BlueprintItem, ItemTooltipData>();

	public static TooltipsDataCache Instance => s_Instance;

	public TooltipsDataCache()
	{
		s_Instance = this;
		Game.Instance.Controllers.SelectionCharacter.SelectedUnitInUI.Subscribe(delegate
		{
			Clear();
		}).AddTo(this);
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		Clear();
		s_Instance = null;
	}

	public void Clear()
	{
		m_ItemTooltipDataCache.Clear();
		m_BlueprintItemTooltipDataCache.Clear();
	}

	public ItemTooltipData GetItemTooltipData(ItemEntity item, bool forceUpdate = false, bool replenishing = false)
	{
		if (!m_ItemTooltipDataCache.TryGetValue(item, out var value) || forceUpdate)
		{
			value = UIUtilityItem.GetItemTooltipData(item, replenishing);
			m_ItemTooltipDataCache[item] = value;
		}
		return value;
	}

	public ItemTooltipData GetItemTooltipData(BlueprintItem blueprintItem, bool forceUpdate = false)
	{
		if (!m_BlueprintItemTooltipDataCache.TryGetValue(blueprintItem, out var value) || forceUpdate)
		{
			value = UIUtilityItem.GetItemTooltipData(blueprintItem);
			m_BlueprintItemTooltipDataCache[blueprintItem] = value;
		}
		return value;
	}
}
