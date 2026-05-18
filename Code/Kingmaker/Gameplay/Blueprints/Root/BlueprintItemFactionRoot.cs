using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Blueprints.Root;

[Serializable]
[TypeId("2bf5620918f05c24f9648e367fd6d021")]
public sealed class BlueprintItemFactionRoot : BlueprintScriptableObject
{
	[Serializable]
	public sealed class FactionConfig
	{
		public ItemFaction faction;

		public StatFactionModifierConfig[] Modifiers = Array.Empty<StatFactionModifierConfig>();
	}

	public FactionConfig[] WeaponFactions = Array.Empty<FactionConfig>();

	public FactionConfig[] ArmorFactions = Array.Empty<FactionConfig>();

	public StatFactionModifierConfig[] GetWeaponModifiers(ItemFaction faction)
	{
		return WeaponFactions.FirstOrDefault((FactionConfig c) => c.faction == faction)?.Modifiers ?? Array.Empty<StatFactionModifierConfig>();
	}

	public StatFactionModifierConfig[] GetArmorModifiers(ItemFaction faction)
	{
		return ArmorFactions.FirstOrDefault((FactionConfig c) => c.faction == faction)?.Modifiers ?? Array.Empty<StatFactionModifierConfig>();
	}
}
