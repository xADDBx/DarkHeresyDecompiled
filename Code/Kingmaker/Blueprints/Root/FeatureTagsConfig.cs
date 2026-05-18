using System;
using System.Collections.Generic;
using System.Linq;
using Code.View.UI.MVVM;
using Kingmaker.Code.Gameplay.Components.Features;
using Kingmaker.Code.View.Bridge.Enums;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class FeatureTagsConfig
{
	[Serializable]
	private class TagCustomColor<T> where T : Enum
	{
		public T Tag;

		public Color Color;
	}

	[Serializable]
	private class TagIcon<T> where T : Enum
	{
		public T Tag;

		public Sprite Icon;
	}

	[Serializable]
	private class RarityColors
	{
		public Color CommonColor;

		public Color PatternColor;

		public Color RefinedColor;

		public Color UniqueColor;
	}

	[Header("Colors")]
	[SerializeField]
	private RarityColors m_EditorColors;

	[FormerlySerializedAs("m_DefaultWeaponColors")]
	[SerializeField]
	private RarityColors m_WeaponTagColors;

	[SerializeField]
	private RarityColors m_WeaponMountColors;

	[SerializeField]
	private RarityColors m_ArmourTagColors;

	[SerializeField]
	private RarityColors m_ArmourMountColors;

	[Header("Customs colors")]
	[SerializeField]
	private List<TagCustomColor<WeaponTagProperty>> m_WeaponTagCustomColors = new List<TagCustomColor<WeaponTagProperty>>();

	[SerializeField]
	private List<TagCustomColor<ArmourTagProperty>> m_ArmourTagCustomColors = new List<TagCustomColor<ArmourTagProperty>>();

	[Header("Icons")]
	[SerializeField]
	private Sprite m_FallbackIcon;

	[SerializeField]
	private List<TagIcon<WeaponTagProperty>> m_WeaponTagIcons = new List<TagIcon<WeaponTagProperty>>();

	[SerializeField]
	private List<TagIcon<ArmourTagProperty>> m_ArmourTagIcons = new List<TagIcon<ArmourTagProperty>>();

	[Header("Values")]
	[SerializeField]
	public bool ShowTagsDescriptions = true;

	[field: Header("Weapon Tags")]
	[field: FormerlySerializedAs("<CommonProperties>k__BackingField")]
	[field: SerializeField]
	public List<WeaponTagProperty> CommonWeaponProperties { get; private set; } = new List<WeaponTagProperty>();


	[field: FormerlySerializedAs("<PatternProperties>k__BackingField")]
	[field: SerializeField]
	public List<WeaponTagProperty> PatternWeaponProperties { get; private set; } = new List<WeaponTagProperty>();


	[field: FormerlySerializedAs("<UniqueProperties>k__BackingField")]
	[field: SerializeField]
	public List<WeaponTagProperty> UniqueWeaponProperties { get; private set; } = new List<WeaponTagProperty>();


	[field: Header("Armour Tags")]
	[field: SerializeField]
	public List<ArmourTagProperty> CommonArmourProperties { get; private set; } = new List<ArmourTagProperty>();


	[field: SerializeField]
	public List<ArmourTagProperty> PatternArmourProperties { get; private set; } = new List<ArmourTagProperty>();


	[field: SerializeField]
	public List<ArmourTagProperty> RefinedArmourProperties { get; private set; } = new List<ArmourTagProperty>();


	[field: SerializeField]
	public List<ArmourTagProperty> UniqueArmourProperties { get; private set; } = new List<ArmourTagProperty>();


	[Header("Special Weapon Damage Icons")]
	[field: SerializeField]
	public EnumToObjectSelector<SpecialWeaponDamageType, Sprite> SpecialWeaponDamageTypeIcons { get; private set; }

	public Color GetWeaponTagColor(WeaponTagUISettings settings)
	{
		return m_WeaponTagCustomColors.FirstOrDefault((TagCustomColor<WeaponTagProperty> c) => c.Tag == settings.Tag)?.Color ?? GetRarityColorIn(m_WeaponTagColors, settings.Type);
	}

	public Color GetWeaponMountColor(WeaponTagUISettings settings)
	{
		return GetRarityColorIn(m_WeaponMountColors, settings.Type);
	}

	public Color GetArmourTagColor(ArmourTagUISettings settings)
	{
		return m_ArmourTagCustomColors.FirstOrDefault((TagCustomColor<ArmourTagProperty> c) => c.Tag == settings.Tag)?.Color ?? GetRarityColorIn(m_WeaponTagColors, settings.Type);
	}

	public Color GetArmourMountColor(ArmourTagUISettings settings)
	{
		return GetRarityColorIn(m_ArmourMountColors, settings.Type);
	}

	public Sprite GetWeaponTagIcon(WeaponTagProperty tag)
	{
		TagIcon<WeaponTagProperty> tagIcon = m_WeaponTagIcons.FirstOrDefault((TagIcon<WeaponTagProperty> i) => i.Tag == tag);
		if (!(tagIcon?.Icon))
		{
			return m_FallbackIcon;
		}
		return tagIcon.Icon;
	}

	public Sprite GetSpecialWeaponTagIcon(SpecialWeaponDamageType tag)
	{
		return SpecialWeaponDamageTypeIcons.GetEntity(tag) ?? m_FallbackIcon;
	}

	public Sprite GetWeaponTagIcon(WeaponTagUISettings settings)
	{
		return GetWeaponTagIcon(settings.Tag);
	}

	public Sprite GetArmourTagIcon(ArmourTagUISettings settings)
	{
		return m_ArmourTagIcons.FirstOrDefault((TagIcon<ArmourTagProperty> i) => i.Tag == settings.Tag)?.Icon ?? m_FallbackIcon;
	}

	public Color GetPropertyColor(PropertyType type)
	{
		return GetRarityColorIn(m_EditorColors, type);
	}

	private Color GetRarityColorIn(RarityColors color, PropertyType type)
	{
		return type switch
		{
			PropertyType.Common => color.CommonColor, 
			PropertyType.Pattern => color.PatternColor, 
			PropertyType.Refined => color.RefinedColor, 
			PropertyType.Unique => color.UniqueColor, 
			_ => Color.magenta, 
		};
	}
}
