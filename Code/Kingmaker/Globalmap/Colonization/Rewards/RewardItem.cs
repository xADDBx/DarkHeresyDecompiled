using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[Obsolete]
[TypeId("1cf6ddd24787436dba7a7e4832eb52ce")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[AllowedOn(typeof(BlueprintAnswer))]
public class RewardItem : Reward
{
	[SerializeField]
	private BlueprintItemReference m_Item;

	[SerializeField]
	private int m_Count;

	public int Count => m_Count;

	public BlueprintItem Item => m_Item?.Get();

	public bool IsMiner
	{
		get
		{
			if (Item != null)
			{
				return Item.ItemType == ItemsItemType.ResourceMiner;
			}
			return false;
		}
	}
}
