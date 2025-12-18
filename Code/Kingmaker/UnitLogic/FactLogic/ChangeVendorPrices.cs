using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowMultipleComponents]
[TypeId("8f5f3ac296602c54e91a9f7cba1c89b4")]
public class ChangeVendorPrices : BlueprintComponent
{
	[Serializable]
	public class Entry
	{
		[ValidateNotNull]
		[SerializeField]
		[FormerlySerializedAs("Item")]
		private BlueprintItemReference m_Item;

		public long CostOverride;

		public BlueprintItem Item => m_Item?.Get();
	}

	[SerializeField]
	private Entry[] m_Overrides;

	private Dictionary<BlueprintItem, long> m_ItemsToCosts;

	public Dictionary<BlueprintItem, long> Overrides
	{
		get
		{
			if (m_ItemsToCosts == null)
			{
				m_ItemsToCosts = new Dictionary<BlueprintItem, long>();
				Entry[] overrides = m_Overrides;
				foreach (Entry entry in overrides)
				{
					if ((bool)entry.Item)
					{
						m_ItemsToCosts[entry.Item] = entry.CostOverride;
					}
				}
			}
			return m_ItemsToCosts;
		}
	}

	public float GetProfitFactorCost(BlueprintItem item)
	{
		if (Overrides.TryGetValue(item, out var value))
		{
			return value;
		}
		return item.Cost;
	}
}
