using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Area;
using Kingmaker.Enums;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Loot;

[TypeId("0449d0493fd70da4ba79ef76be174b92")]
public class BlueprintLoot : BlueprintScriptableObject
{
	public LootType Type;

	[ShowIf("IsTrashLoot")]
	public TrashLootAmount TrashLootAmount = TrashLootAmount.Percent100;

	public bool Identify;

	[HideIf("IsQuestLoot")]
	public LootSetting Setting;

	[SerializeField]
	[FormerlySerializedAs("Area")]
	private BlueprintAreaReference m_Area;

	public string ContainerName;

	public LootEntry[] Items = new LootEntry[0];

	public BlueprintArea Area => m_Area?.Get();

	[UsedImplicitly]
	private bool IsQuestLoot => Type == LootType.Quest;

	[UsedImplicitly]
	private bool IsTrashLoot => Type == LootType.Trash;

	public float TotalCost => Items.Sum((LootEntry e) => (e.Item != null) ? (e.Item.Cost * e.Count) : 0);

	public override void OnEnable()
	{
		base.OnEnable();
		if (Identify)
		{
			LootEntry[] items = Items;
			for (int i = 0; i < items.Length; i++)
			{
				items[i].Identify = true;
			}
		}
	}
}
