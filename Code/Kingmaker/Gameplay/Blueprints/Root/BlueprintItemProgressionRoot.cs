using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Gameplay.Features.Items.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Blueprints.Root;

[Serializable]
[TypeId("24b4730d3f1245f59faee465106249eb")]
public sealed class BlueprintItemProgressionRoot : BlueprintScriptableObject
{
	public delegate void ImportDelegate(BlueprintItemProgressionRoot root);

	[Serializable]
	public sealed class WeaponProgressionEntry
	{
		public WeaponProgressionType Type;

		public WeaponStatsEntry[] PowerLevelToStats = new WeaponStatsEntry[0];
	}

	[Serializable]
	public sealed class WeaponStatsEntry
	{
		public static readonly WeaponStatsEntry Empty = new WeaponStatsEntry();

		public int DamageMin;

		public int DamageMax;

		public int DamageVital;

		public int HitChanceBonus;

		public int OverpenetrationChance;

		public int BrutalDamage;

		public int DestructiveDamage;
	}

	[Serializable]
	public sealed class ArmorProgressionEntry
	{
		public ArmorProgressionType Type;

		public ArmorStatsEntry[] PowerLevelToStats = new ArmorStatsEntry[0];
	}

	[Serializable]
	public sealed class ArmorStatsEntry
	{
		public static readonly ArmorStatsEntry Empty = new ArmorStatsEntry();

		public int DamageReduction;

		public int Durability;
	}

	public static ImportDelegate OnImportWeapons;

	public static ImportDelegate OnImportArmors;

	public bool EnableForWeapons;

	public WeaponProgressionEntry[] Weapon = new WeaponProgressionEntry[0];

	public bool EnableForArmor;

	public ArmorProgressionEntry[] Armor = new ArmorProgressionEntry[0];

	[NotNull]
	public WeaponStatsEntry Get(WeaponProgressionType type, ItemPowerLevel powerLevel)
	{
		WeaponProgressionEntry[] weapon = Weapon;
		foreach (WeaponProgressionEntry weaponProgressionEntry in weapon)
		{
			if (weaponProgressionEntry.Type == type)
			{
				if (!weaponProgressionEntry.PowerLevelToStats.TryGet((int)powerLevel, out var element))
				{
					return WeaponStatsEntry.Empty;
				}
				return element;
			}
		}
		return WeaponStatsEntry.Empty;
	}

	[NotNull]
	public ArmorStatsEntry Get(ArmorProgressionType type, ItemPowerLevel powerLevel)
	{
		ArmorProgressionEntry[] armor = Armor;
		foreach (ArmorProgressionEntry armorProgressionEntry in armor)
		{
			if (armorProgressionEntry.Type == type)
			{
				if (!armorProgressionEntry.PowerLevelToStats.TryGet((int)powerLevel, out var element))
				{
					return ArmorStatsEntry.Empty;
				}
				return element;
			}
		}
		return ArmorStatsEntry.Empty;
	}

	[BlueprintButton(Name = "Import Weapons")]
	private void ImportWeapons()
	{
		OnImportWeapons?.Invoke(this);
	}

	[BlueprintButton(Name = "Import Armors")]
	private void ImportArmor()
	{
		OnImportArmors?.Invoke(this);
	}
}
