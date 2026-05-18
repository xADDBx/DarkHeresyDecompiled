using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

public class StatsStrings : StringsContainer, ISerializationCallbackReceiver
{
	[NotNull]
	public Entry[] Entries = Array.Empty<Entry>();

	[NotNull]
	[SerializeField]
	private ArmorCategoryEntry[] ArmorCategoryEntriesEntries = Array.Empty<ArmorCategoryEntry>();

	[NotNull]
	[SerializeField]
	private WeaponStatEntry[] WeaponStatEntries = Array.Empty<WeaponStatEntry>();

	[NonSerialized]
	[NotNull]
	private IReadOnlyDictionary<StatType, LocalizedString> m_StatsCache = EmptyDictionary<StatType, LocalizedString>.Instance;

	[NonSerialized]
	[NotNull]
	private IReadOnlyDictionary<StatType, LocalizedString> m_StatsShortCache = EmptyDictionary<StatType, LocalizedString>.Instance;

	[NonSerialized]
	[NotNull]
	private IReadOnlyDictionary<StatType, LocalizedString> m_StatsBonusCache = EmptyDictionary<StatType, LocalizedString>.Instance;

	[NonSerialized]
	[NotNull]
	private IReadOnlyDictionary<WarhammerArmorCategory, LocalizedString> m_ArmorCategoryCache = EmptyDictionary<WarhammerArmorCategory, LocalizedString>.Instance;

	[NonSerialized]
	[NotNull]
	private IReadOnlyDictionary<WeaponStat, LocalizedString> m_WeaponStatCache = EmptyDictionary<WeaponStat, LocalizedString>.Instance;

	public string GetText(StatType stat)
	{
		if (!m_StatsCache.TryGetValue(stat, out var value))
		{
			return stat.ToString();
		}
		return value;
	}

	public string GetShortText(StatType stat)
	{
		if (!m_StatsShortCache.TryGetValue(stat, out var value))
		{
			return GetText(stat);
		}
		return value;
	}

	public string GetBonusText(StatType stat)
	{
		if (!m_StatsBonusCache.TryGetValue(stat, out var value))
		{
			return GetText(stat);
		}
		return value;
	}

	public string GetText(WarhammerArmorCategory category)
	{
		if (!m_ArmorCategoryCache.TryGetValue(category, out var value))
		{
			return category.ToString();
		}
		return value;
	}

	public string GetText(WeaponStat stat)
	{
		if (!m_WeaponStatCache.TryGetValue(stat, out var value))
		{
			return stat.ToString();
		}
		return value;
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		m_ArmorCategoryCache = ArmorCategoryEntriesEntries.Distinct(EqualitySelector.Create((ArmorCategoryEntry v) => v.Category)).ToDictionary((ArmorCategoryEntry v) => v.Category, (ArmorCategoryEntry v) => v.Text);
		m_StatsCache = Entries.Distinct(EqualitySelector.Create((Entry v) => v.Stat)).ToDictionary((Entry v) => v.Stat, (Entry v) => v.Text);
		m_StatsShortCache = Entries.Distinct(EqualitySelector.Create((Entry v) => v.Stat)).ToDictionary((Entry v) => v.Stat, (Entry v) => v.ShortText);
		m_StatsBonusCache = Entries.Distinct(EqualitySelector.Create((Entry v) => v.Stat)).ToDictionary((Entry v) => v.Stat, (Entry v) => v.BonusText);
		m_WeaponStatCache = WeaponStatEntries.Distinct(EqualitySelector.Create((WeaponStatEntry v) => v.Stat)).ToDictionary((WeaponStatEntry v) => v.Stat, (WeaponStatEntry v) => v.Text);
	}
}
